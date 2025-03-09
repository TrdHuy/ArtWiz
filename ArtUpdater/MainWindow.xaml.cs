using System.IO.Compression;
using System.IO;
using System.Windows;
using System.Diagnostics;
using ArtUpdater.Utils;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ArtUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, UpdaterCallback
    {
        private const int WM_NCRBUTTONUP = 0x00A5; // Mã sự kiện chuột phải nhả ra
        private const int HTCAPTION = 2; // Thanh tiêu đề (caption)
        private Storyboard mRotatingAnimation;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SourceInitialized += MainWindow_SourceInitialized;
            mRotatingAnimation = (Storyboard)LogoImage.FindResource("RotationStoryboard");

#if DEBUG
            TestButton.Visibility = Visibility.Visible;
#endif
        }

        #region Native API
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCRBUTTONUP && wParam.ToInt32() == HTCAPTION)
            {
                // Chặn sự kiện mở context menu trên vùng caption 
                handled = true;
            }
            return IntPtr.Zero;
        }
        #endregion

        public void OnError(string step, string message)
        {
            Dispatcher.Invoke(() =>
            {
                StopLoadingAnimation();
                TitleContentTextBlock.Text = step;
                ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                OtherTextBlock.Visibility = Visibility.Visible;
                OtherTextBlock.Text = message;

                ConfirmButton.Content = "Thử lại";
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void OnProgressChanged(string step, double currentProgress,
            string extractedFilePath,
            string copiedFilePath)
        {
            Dispatcher.Invoke(() =>
            {
                if (currentProgress < 1)
                {
                    StartLoadingAnimation();
                    ConfirmButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Visible;
                }
                if (currentProgress == 1)
                {
                    ConfirmButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    StopLoadingAnimation();
                }
                TitleContentTextBlock.Text = step;
                TaskProgressbar.Value = currentProgress * 100d;
                if (!string.IsNullOrEmpty(extractedFilePath))
                {
                    HeaderRun.Text = "Đã giải nén: ";
                    ExtractDetailTextBlock.Visibility = Visibility.Visible;
                    ExtractingFileRun.Text = extractedFilePath;
                    OtherTextBlock.Visibility = Visibility.Collapsed;
                }
                else if (!string.IsNullOrEmpty(copiedFilePath))
                {
                    HeaderRun.Text = "Đã sao chép: ";
                    ExtractDetailTextBlock.Visibility = Visibility.Visible;
                    ExtractingFileRun.Text = copiedFilePath;
                    OtherTextBlock.Visibility = Visibility.Collapsed;
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

        public void OnCancelling(string message, string fileRestoredPath, double progress)
        {
            Dispatcher.Invoke(() =>
            {
                CancelButton.IsEnabled = false;
                ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                TitleContentTextBlock.Text = message;
                OtherTextBlock.Visibility = Visibility.Visible;
                OtherTextBlock.Text = $"Khôi phục file: {fileRestoredPath}";
                TaskProgressbar.Value = progress * 100d;
            }, System.Windows.Threading.DispatcherPriority.Render);
        }
        public void OnCancelled(string message)
        {
            Dispatcher.Invoke(() =>
            {
                CancelButton.IsEnabled = true;
                ExtractDetailTextBlock.Visibility = Visibility.Collapsed;
                OtherTextBlock.Visibility = Visibility.Collapsed;
                StopLoadingAnimation();
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
            RunAppUpdater();
        }

        private void ConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            switch (Updater.CurrentApplyUpdateStatus)
            {
                case Updater.ApplyUpdateStatus.None:
                case Updater.ApplyUpdateStatus.Failed:
                    RunAppUpdater();
                    break;
                case Updater.ApplyUpdateStatus.Success:
                    this.Close();
                    break;
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            switch (Updater.CurrentApplyUpdateStatus)
            {
                case Updater.ApplyUpdateStatus.Cancelled:
                case Updater.ApplyUpdateStatus.None:
                case Updater.ApplyUpdateStatus.Failed:
                case Updater.ApplyUpdateStatus.FailedButCanNotRetry:
                    this.Close();
                    break;
                case Updater.ApplyUpdateStatus.Started:
                    Updater.CancelAppUpdater();
                    break;
            }
        }

        private void TestButtonClick(object sender, RoutedEventArgs e)
        {
            //var isActive = false;
            //try
            //{
            //    isActive = mRotatingAnimation.GetCurrentState(LogoImage) == ClockState.Active;
            //}
            //catch (Exception ex) { }
            //if (isActive)
            //{
            //    mRotatingAnimation.Stop(LogoImage);
            //}
            //else
            //{
            //    mRotatingAnimation.Begin(LogoImage, true);
            //}

            RunAppUpdater();
        }

        private void StartLoadingAnimation()
        {
            var isActive = false;
            try
            {
                isActive = mRotatingAnimation.GetCurrentState(LogoImage) == ClockState.Active;
            }
            catch (Exception ex) { }
            if (!isActive)
            {
                mRotatingAnimation.Begin(LogoImage, true);
            }
        }

        private void StopLoadingAnimation()
        {
            var isActive = false;
            try
            {
                isActive = mRotatingAnimation.GetCurrentState(LogoImage) == ClockState.Active;
            }
            catch (Exception ex) { }
            if (isActive)
            {
                mRotatingAnimation.Stop(LogoImage);
            }
        }

        private void RunAppUpdater()
        {
            Task.Run(async () =>
            {
                await Updater.ApplyUpdateAsync(App.ZipFilePath,
                    App.InstallPath,
                    this);
            });
        }

    }


    interface UpdaterCallback
    {
        void OnProgressChanged(string step,
            double currentProgress,
            string extractedFilePath,
            string copiedFilePath);

        void OnError(string step, string message);

        void OnWait(string message);
        void OnCancelling(string message, string fileRestoredPath, double progress);
        void OnCancelled(string message);
    }


    class Updater
    {
        public enum ApplyUpdateStatus
        {
            None,
            Started,
            Cancelled,
            Failed,
            FailedButCanNotRetry,
            Success,
        }
        private static Logger mLogger = new Logger("Updater");
        private static readonly SemaphoreSlim mUpdateLock = new SemaphoreSlim(1, 1);
        private static CancellationTokenSource? mAppUpdaterCTS;

        public static ApplyUpdateStatus CurrentApplyUpdateStatus { get; private set; } = ApplyUpdateStatus.None;


        public static void CancelAppUpdater()
        {
            if (mAppUpdaterCTS != null && !mAppUpdaterCTS.IsCancellationRequested)
            {
                mAppUpdaterCTS.Cancel();
                mAppUpdaterCTS.Dispose();
                mAppUpdaterCTS = null;
            }
        }

        public static async Task<bool> ApplyUpdateAsync(string zipFilePath,
            string installPath,
            UpdaterCallback callback,
            int delayOnEachExtractedFileMillisec = 300)
        {
            await mUpdateLock.WaitAsync(); // Chờ đến khi có thể chạy

            if (mAppUpdaterCTS != null &&
                mAppUpdaterCTS?.IsCancellationRequested == false)
            {
                return false;
            }
            mAppUpdaterCTS = new CancellationTokenSource();
            var token = mAppUpdaterCTS.Token;

            List<string> movedFiles = new List<string>();
            string backupPath = Path.Combine(installPath, "backup");

            try
            {
                CurrentApplyUpdateStatus = ApplyUpdateStatus.Started;
                if (!File.Exists(zipFilePath))
                {
                    mLogger.E($"Update file {zipFilePath} not found!");
                    CurrentApplyUpdateStatus = ApplyUpdateStatus.FailedButCanNotRetry;
                    callback.OnError("Lỗi xác minh phiên bản cập nhật.",
                        "Không tìm thấy file cập nhật.");
                    return false;
                }

                string extractPath = Path.Combine(Path.GetTempPath(), "Updater_Extract");

                double currentProgress = 0d;
                callback.OnProgressChanged("Đang xác minh phiên bản cập nhật.",
                    currentProgress, "", "");

                // Đếm số lượng file thực tế (bỏ qua thư mục)
                int totalFiles;
                VersionInfo? versionInfo = null;

                // TODO: Đọc file version.json trong zip để nhận thông tin phiên bản
                // extract ra targetExe path để chạy file sau khi cập nhật hoàn tất
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    var versionInfoPath = "version.json";
                    ZipArchiveEntry textFileEntry = archive.Entries
                        .FirstOrDefault(e => e.FullName.EndsWith(versionInfoPath,
                        StringComparison.OrdinalIgnoreCase));
                    if (textFileEntry != null)
                    {
                        using (StreamReader reader = new StreamReader(textFileEntry.Open()))
                        {
                            string content = reader.ReadToEnd();
                            versionInfo = JsonSerializer.Deserialize<VersionInfo>(content);
                        }
                    }
                    else
                    {
                        callback.OnError("Lỗi xác minh phiên bản cập nhật.",
                            "Không tìm thấy version info.");
                        return false;
                    }

                    if (versionInfo != null)
                    {
                        totalFiles = archive.Entries.Count(e => !e.FullName.EndsWith("/")
                            && !versionInfo.ExcludedFiles.Contains(e.FullName));
                    }
                    else
                    {
                        callback.OnError("Lỗi xác minh phiên bản cập nhật.",
                            "Không tìm thấy version info.");
                        return false;
                    }
                }

                mLogger.I($"Total files in update: {totalFiles}");

                double totalProgress = totalFiles * 2;

                // Backup thư mục cài đặt hiện tại
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                Directory.CreateDirectory(backupPath);

                // Xóa thư mục extract nếu đã tồn tại
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                Directory.CreateDirectory(extractPath);

                // Giải nén file ZIP vào thư mục tạm
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/") && !versionInfo.ExcludedFiles.Contains(entry.FullName))
                        {
                            string destinationPath = Path.Combine(extractPath, entry.FullName);

                            // Đảm bảo thư mục đích tồn tại
                            string directoryPath = Path.GetDirectoryName(destinationPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            // Giải nén file
                            entry.ExtractToFile(destinationPath, true);

                            currentProgress++;
                            callback.OnProgressChanged("Đang cài đặt.",
                                currentProgress / totalProgress,
                                extractedFilePath: entry.FullName,
                                copiedFilePath: "");
                            await Task.Delay(delayOnEachExtractedFileMillisec / 2, token);
                        }
                    }

                }

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

                // Cop file từ thư mục giải nén sang thư mục cài đặt
                while (filesToMove.Count > 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                    currentProgress++;

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
                            currentProgress / totalProgress,
                            extractedFilePath: "",
                            copiedFilePath: relativeFilePath);
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
                            await RevertMovedFiles(movedFiles,
                                backupPath,
                                installPath, 0);
                            return false;
                        }
                        else
                        {
                            mLogger.I($"{destFile} is in use by {processName}. Waiting for it to close...");

                            Process[] processes = Process.GetProcessesByName(processName);
                            if (processes.Length > 0)
                            {
                                callback.OnWait($"Vui lòng tắt tiến trình {processName} để tiếp tục cập nhật!");
                                await processes[0].WaitForExitAsync(token); // Chờ process bị kill
                                filesToMove.Push(file);  // Thêm lại file vào hàng đợi để retry
                            }
                        }
                    }

                    await Task.Delay(delayOnEachExtractedFileMillisec, token);
                }

                // Cleanup
                callback.OnProgressChanged("Đang dọn dẹp.",
                           1d, "", "");
                Directory.Delete(extractPath, true);
                Directory.Delete(backupPath, true);

                mLogger.I("Update applied successfully!");
                CurrentApplyUpdateStatus = ApplyUpdateStatus.Success;
                callback.OnProgressChanged("Cập nhật thành công.", 1d, "", "");
                return true;
            }
            catch (TaskCanceledException)
            {
                mLogger.E($"Abort install new version.");
                CurrentApplyUpdateStatus = ApplyUpdateStatus.Cancelled;
                callback.OnCancelling("Hủy cài đặt phiên bản cập nhật.", "", 0);
                await RevertMovedFiles(movedFiles, backupPath, installPath,
                    delayOnEachExtractedFileMillisec,
                    fileRestoredCallback: (filePath, progress) =>
                    {
                        callback.OnCancelling("Đang khôi phục.", filePath, progress);
                    });
                callback.OnCancelled("Khôi phục thành công.");
                return false;
            }
            catch (Exception ex)
            {
                CurrentApplyUpdateStatus = ApplyUpdateStatus.FailedButCanNotRetry;
                mLogger.E($"Update failed: {ex.Message}.");
                return false;
            }
            finally
            {
                if (mAppUpdaterCTS != null && !mAppUpdaterCTS.IsCancellationRequested)
                {
                    mAppUpdaterCTS.Dispose();
                    mAppUpdaterCTS = null;
                }
                mUpdateLock.Release(); // Giải phóng Semaphore để luồng khác có thể chạy
            }
        }


        private static async Task RevertMovedFiles(List<string> movedFiles,
            string backupPath,
            string installPath,
            int delayOnEachExtractedFileMillisec,
            Action<string, double>? fileRestoredCallback = null)
        {
            double progress = 0d;
            double index = 0;
            mLogger.I("Reverting moved files...");
            foreach (string file in movedFiles)
            {
                index++;
                string backupFile = Path.Combine(backupPath, Path.GetRelativePath(installPath, file));
                if (File.Exists(backupFile))
                {
                    File.Copy(backupFile, file, true);
                    progress = index / movedFiles.Count;
                    fileRestoredCallback?.Invoke(Path.GetRelativePath(installPath, file), progress);
                    mLogger.I($"Restored: {file}");

                    await Task.Delay(delayOnEachExtractedFileMillisec);
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

    class VersionInfo
    {
        [JsonPropertyName("startupFile")]
        public string StartupFile { get; set; }

        [JsonPropertyName("excludedFiles")]
        public string[] ExcludedFiles { get; set; }
    }

}