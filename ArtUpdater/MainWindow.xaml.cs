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

namespace ArtUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.ZipFilePath) ||
                string.IsNullOrEmpty(App.InstallPath))
            {
                // TODO Hiển thị không thể update
                return;
            }
            Updater.ApplyUpdate(App.ZipFilePath, App.InstallPath);
        }
    }


    class Updater
    {
        public static bool ApplyUpdate(string zipFilePath, string installPath)
        {
            if (!File.Exists(zipFilePath))
            {
                Console.WriteLine("Update file not found!");
                return false;
            }

            string backupPath = Path.Combine(installPath, "backup");
            string extractPath = Path.Combine(Path.GetTempPath(), "Updater_Extract");
            List<string> movedFiles = new List<string>();

            try
            {
                // Đếm số lượng file thực tế (bỏ qua thư mục)
                int totalFiles;
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    totalFiles = archive.Entries.Count(e => !e.FullName.EndsWith("/"));
                }

                Console.WriteLine($"Total files in update: {totalFiles}");

                // Backup thư mục cài đặt hiện tại
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                Directory.CreateDirectory(backupPath);

                foreach (string file in Directory.GetFiles(installPath))
                {
                    string backupFile = Path.Combine(backupPath, Path.GetFileName(file));
                    File.Copy(file, backupFile, true);
                }

                // Xóa thư mục extract nếu đã tồn tại
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                Directory.CreateDirectory(extractPath);

                // Giải nén file ZIP vào thư mục tạm
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);

                // Đếm số file đã giải nén (bỏ qua thư mục)
                int extractedFiles = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories).Length;
                Console.WriteLine($"Extracted files: {extractedFiles}");

                // Kiểm tra nếu số file extract được không khớp với ZIP
                if (extractedFiles != totalFiles)
                {
                    Console.WriteLine("Extraction failed! Reverting changes...");
                    RevertUpdate(backupPath, installPath);
                    return false;
                }

                // Di chuyển file từ extractPath vào installPath
                foreach (string file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
                {
                    string destFile = Path.Combine(installPath, Path.GetFileName(file));
                    try
                    {
                        File.Move(file, destFile, true);
                        movedFiles.Add(destFile);
                    }
                    catch (IOException)
                    {
                        string processName = GetProcessLockingFile(destFile);
                        Console.WriteLine($"Error: Unable to replace {destFile}. It is currently in use by process: {processName}");
                        RevertMovedFiles(movedFiles, backupPath);
                        return false;
                    }
                }

                // Cleanup
                Directory.Delete(extractPath, true);
                Directory.Delete(backupPath, true);

                Console.WriteLine("Update applied successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}. Reverting changes...");
                RevertUpdate(backupPath, installPath);
                return false;
            }
        }

        private static void RevertUpdate(string backupPath, string installPath)
        {
            if (!Directory.Exists(backupPath))
                return;

            foreach (string file in Directory.GetFiles(backupPath))
            {
                string originalFile = Path.Combine(installPath, Path.GetFileName(file));
                File.Copy(file, originalFile, true);
            }

            Directory.Delete(backupPath, true);
        }

        private static void RevertMovedFiles(List<string> movedFiles, string backupPath)
        {
            Console.WriteLine("Reverting moved files...");
            foreach (string file in movedFiles)
            {
                string backupFile = Path.Combine(backupPath, Path.GetFileName(file));
                if (File.Exists(backupFile))
                {
                    File.Copy(backupFile, file, true);
                    Console.WriteLine($"Restored: {file}");
                }
            }
        }

        private static string GetProcessLockingFile(string filePath)
        {
            try
            {
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