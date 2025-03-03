using System.IO.Compression;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using ArtUpdater.Utils;

namespace ArtUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, UpdaterCallback
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        public void OnError(string step, string message)
        {
            Dispatcher.Invoke(() =>
            {
                TitleContentTextBlock.Text = step;
                ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                OtherTextBlock.Visibility = Visibility.Visible;
                OtherTextBlock.Text = message;

                ConfirmButton.Content = "Thử lại";
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void OnProgressChanged(string step, double currentProgress, string fileExtractedPath)
        {
            Dispatcher.Invoke(() =>
            {
                if (currentProgress < 1)
                {
                    ConfirmButton.Visibility = Visibility.Collapsed;
                }
                if (currentProgress == 1)
                {
                    ConfirmButton.Visibility = Visibility.Visible;
                }
                TitleContentTextBlock.Text = step;
                TaskProgressbar.Value = currentProgress * 100d;
                if (!string.IsNullOrEmpty(fileExtractedPath))
                {
                    ExtractDetailTextBlock.Visibility = Visibility.Visible;
                    ExtractingFileRun.Text = fileExtractedPath;
                }
                else
                {
                    ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                }

            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void OnWait(string message)
        {
            Dispatcher.Invoke(() =>
            {
                OtherTextBlock.Visibility = Visibility.Visible;
                ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                OtherTextBlock.Text += message; 

            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.ZipFilePath) ||
                string.IsNullOrEmpty(App.InstallPath))
            {
                // TODO Hiển thị không thể update
                return;
            }
            Task.Run(async () =>
            {
                await Updater.ApplyUpdate(App.ZipFilePath, App.InstallPath, this);
            });
        }

        private void ConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            switch (Updater.CurrentApplyUpdateStatus)
            {
                case Updater.ApplyUpdateStatus.None:
                case Updater.ApplyUpdateStatus.Failed:
                    Task.Run(async () =>
                    {
                        await Updater.ApplyUpdate(App.ZipFilePath, App.InstallPath, this);
                    });
                    break;
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            switch (Updater.CurrentApplyUpdateStatus)
            {
                case Updater.ApplyUpdateStatus.None:
                case Updater.ApplyUpdateStatus.Failed:
                case Updater.ApplyUpdateStatus.FailedButCanNotRetry:
                    this.Close();
                    break;
            }
        }
    }


    interface UpdaterCallback
    {
        void OnProgressChanged(string step,
            double currentProgress,
            string fileExtractedPath);

        void OnError(string step, string message);

        void OnWait(string message);
    }


    class Updater
    {
        public enum ApplyUpdateStatus
        {
            None,
            Started,
            Failed,
            FailedButCanNotRetry,
            Success,
        }
        private static Logger mLogger = new Logger("Updater");
        private static readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

        public static ApplyUpdateStatus CurrentApplyUpdateStatus { get; private set; } = ApplyUpdateStatus.None;

        public static async Task<bool> ApplyUpdate(string zipFilePath,
            string installPath,
            UpdaterCallback callback,
            int delayOnEachExtractedFileMillisec = 300)
        {

            await _updateLock.WaitAsync(); // Chờ đến khi có thể chạy

            try
            {
                CurrentApplyUpdateStatus = ApplyUpdateStatus.Started;
                if (!File.Exists(zipFilePath))
                {
                    mLogger.E($"Update file {zipFilePath} not found!");
                    CurrentApplyUpdateStatus = ApplyUpdateStatus.Failed;
                    callback.OnError("Lỗi xác minh phiên bản cập nhật.",
                        "Không tìm thấy file cập nhật.");
                    return false;
                }

                string backupPath = Path.Combine(installPath, "backup");
                string extractPath = Path.Combine(Path.GetTempPath(), "Updater_Extract");
                List<string> movedFiles = new List<string>();

                double currentProgress = 0d;
                callback.OnProgressChanged("Đang xác minh phiên bản cập nhật.",
                    currentProgress, "");

                // Đếm số lượng file thực tế (bỏ qua thư mục)
                int totalFiles;
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    totalFiles = archive.Entries.Count(e => !e.FullName.EndsWith("/"));
                }

                mLogger.I($"Total files in update: {totalFiles}");

                // Backup thư mục cài đặt hiện tại
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                Directory.CreateDirectory(backupPath);

                // Xóa thư mục extract nếu đã tồn tại
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                Directory.CreateDirectory(extractPath);

                // Giải nén file ZIP vào thư mục tạm
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);

                // Đếm số file đã giải nén (bỏ qua thư mục)
                int extractedFiles = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories).Length;
                mLogger.I($"Extracted files: {extractedFiles}");

                // Kiểm tra nếu số file extract được không khớp với ZIP
                if (extractedFiles != totalFiles)
                {
                    mLogger.E("Extraction failed!");
                    callback.OnError("Lỗi xác minh phiên bản cập nhật.",
                        "Lỗi xác minh phiên bản cập nhật.");
                    return false;
                }

                // Di chuyển file từ extractPath vào installPath
                Stack<string> filesToMove = new Stack<string>(
                    Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories)
                        .Reverse());
                double extractingFileIndex = 0;
                while (filesToMove.Count > 0)
                {
                    extractingFileIndex++;

                    string file = filesToMove.Pop();
                    string relativeFilePath = Path.GetRelativePath(extractPath, file);
                    string destFile = Path.Combine(installPath, relativeFilePath);

                    #region Backup destFile before override
                    string backupFile = Path.Combine(backupPath, relativeFilePath);
                    if (File.Exists(destFile) && !File.Exists(backupFile))
                    {
                        var backupDir = Path.GetDirectoryName(backupFile);
                        if (!Directory.Exists(backupDir))
                        {
                            Directory.CreateDirectory(backupDir);
                        }
                        File.Copy(destFile, backupFile, true);
                    }
                    #endregion

                    try
                    {
                        string destDir = Path.GetDirectoryName(destFile);
                        if (!Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }
                        File.Move(file, destFile, true);
                        movedFiles.Add(destFile);
                        callback.OnProgressChanged("Đang cài đặt.",
                            extractingFileIndex / totalFiles, relativeFilePath);
                    }
                    catch (IOException)
                    {
                        mLogger.D($"PF: Start getting process locking file for {destFile}...");
                        string processName = GetProcessLockingFile(destFile, App.PrioritySearchingProcess);
                        mLogger.D($"PF: End getting process locking file for {destFile}...");

                        if (processName == "Unknown Process")
                        {
                            mLogger.E($"Error: Unable to replace {destFile}. It is locked by an unknown process.");
                            CurrentApplyUpdateStatus = ApplyUpdateStatus.Failed;
                            callback.OnError("Đang cài đặt.",
                                $"Không thể cập nhật file {destFile}. Vui lòng tắt các tiến trình liên quan và tiến hành cập nhật lại!");
                            RevertMovedFiles(movedFiles, backupPath, installPath);
                            return false;
                        }
                        else
                        {
                            mLogger.I($"{destFile} is in use by {processName}. Waiting for it to close...");

                            Process[] processes = Process.GetProcessesByName(processName);
                            if (processes.Length > 0)
                            {
                                callback.OnWait($"Vui lòng tắt tiến trình {processName} để tiếp tục cập nhật!");
                                await processes[0].WaitForExitAsync(); // Chờ process bị kill
                                filesToMove.Push(file);  // Thêm lại file vào hàng đợi để retry
                            }
                        }
                    }

                    await Task.Delay(delayOnEachExtractedFileMillisec);
                }

                // Cleanup
                callback.OnProgressChanged("Đang dọn dẹp.",
                           1d, "");
                Directory.Delete(extractPath, true);
                Directory.Delete(backupPath, true);

                mLogger.I("Update applied successfully!");
                CurrentApplyUpdateStatus = ApplyUpdateStatus.Success;
                callback.OnProgressChanged("Cập nhật thành công.",
                                   1d, "");
                return true;
            }
            catch (Exception ex)
            {
                CurrentApplyUpdateStatus = ApplyUpdateStatus.FailedButCanNotRetry;
                mLogger.E($"Update failed: {ex.Message}.");
                return false;
            }
            finally
            {
                _updateLock.Release(); // Giải phóng Semaphore để luồng khác có thể chạy
            }
        }


        private static void RevertMovedFiles(List<string> movedFiles, string backupPath, string installPath)
        {
            mLogger.I("Reverting moved files...");
            foreach (string file in movedFiles)
            {
                string backupFile = Path.Combine(backupPath, Path.GetRelativePath(installPath, file));
                if (File.Exists(backupFile))
                {
                    File.Copy(backupFile, file, true);
                    mLogger.I($"Restored: {file}");
                }
            }
        }

        private static string GetProcessLockingFile(string filePath, List<string> prioritizedProcesses)
        {
            try
            {
                // Kiểm tra trước trong danh sách process ưu tiên
                foreach (var processName in prioritizedProcesses)
                {
                    Process[] targetProcesses = Process.GetProcessesByName(processName);
                    foreach (var process in targetProcesses)
                    {
                        try
                        {
                            foreach (ProcessModule module in process.Modules)
                            {
                                if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    return process.ProcessName;
                                }
                            }
                        }
                        catch { }
                    }
                }

                // Nếu không tìm thấy trong danh sách ưu tiên, duyệt toàn bộ process còn lại
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        foreach (ProcessModule module in process.Modules)
                        {
                            if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                            {
                                return process.ProcessName;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return "Unknown Process";
        }
    }
}