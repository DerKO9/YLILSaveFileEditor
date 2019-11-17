using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace YLILSaveFileEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string selectedFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string SelectedFilePath 
        { 
            get => selectedFilePath;
            set 
            { 
                selectedFilePath = value; 
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileDisplay)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsPathSelected)));
            }
        }

        public string SelectedFileDisplay { get => SelectedFilePath.Split('\\').LastOrDefault(); }
        public bool IsPathSelected { get { return !string.IsNullOrEmpty(SelectedFilePath); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
        SelectFile:
            if (openFileDialog.ShowDialog() == true)
            {
                if (System.IO.Path.GetFileName(openFileDialog.FileName).Contains("profile"))
                {
                    MessageBoxResult result = MessageBox.Show("Can't edit \"profile.dat\"\n" +
                        "It contains profile data and is not a save file.", @"Can't Open File ¯\_(ツ)_/¯",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.OK)
                        goto SelectFile;
                    else return;
                }
                SelectedFilePath = openFileDialog.FileName;
            }
            else return;

            if (SelectedFilePath.Contains("Decompressed"))
            {
                _compressButton.IsEnabled = true;
                _decompressButton.IsEnabled = false;
            }
            else
            {
                _compressButton.IsEnabled = false;
                _decompressButton.IsEnabled = true;
            }
        }

        private void DecompressFile()
        {
            try
            {
                using (FileStream stream = File.OpenRead(SelectedFilePath))
                {
                    using (MemoryStream memoryStream2 = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            byte[] array = new byte[65535];
                            int num;
                            do
                            {
                                num = gzipStream.Read(array, 0, array.Length);
                                memoryStream2.Write(array, 0, num);
                            }
                            while (num > 0);
                        }
                        String result = Encoding.UTF8.GetString(memoryStream2.ToArray());
                        string decompressedFilePath = SelectedFilePath.Substring(0, selectedFilePath.Length - ".dat".Length) + "-Decompressed.dat";
                        File.WriteAllText(decompressedFilePath, result);

                        SelectedFilePath = decompressedFilePath;
                        _compressButton.IsEnabled = true;
                        _decompressButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not decompress file\n{e.Message}", "Could not decompress file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CompressFile()
        {
            try
            {
                string contents = File.ReadAllText(SelectedFilePath);
                byte[] bytes = Encoding.UTF8.GetBytes(contents);
                byte[] result;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(bytes, 0, bytes.Length);
                        gzipStream.Close();
                        result = memoryStream.ToArray();
                    }
                }
                string compressedFilePath = SelectedFilePath.Substring(0, selectedFilePath.Length - "-Decompressed.dat".Length) + ".dat";
                File.WriteAllBytes(compressedFilePath, result);

                SelectedFilePath = compressedFilePath;
                _compressButton.IsEnabled = false;
                _decompressButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not compress file\n{e.Message}", "Could not compress file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void _loadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadFile();
        }

        private void _decompressButton_Click(object sender, RoutedEventArgs e)
        {
            DecompressFile();
        }

        private void _compressButton_Click(object sender, RoutedEventArgs e)
        {
            CompressFile();
        }
    }
}
