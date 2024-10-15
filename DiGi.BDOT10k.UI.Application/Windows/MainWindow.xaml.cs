using Microsoft.Win32;
using System.Windows;
using DiGi.BDOT10k.UI.Classes;
using DiGi.BDOT10k.Classes;
using System.IO.Compression;
using System.IO;
using DiGi.Geometry.Planar.Classes;
using System.Net.Http;
using DiGi.Geometry.Spatial.Classes;
using System.Globalization;
using System.Windows.Controls;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI.Application.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            OT_ADMS_A oT_ADMS_A = null;
        }

        private async Task<bool> Load_Old() 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog(this);
            if (result == null || !result.HasValue || !result.Value)
            {
                return false;
            }

            string path = openFileDialog.FileName;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            SlownikObiektowGeometrycznych slownikObiektowGeometrycznych = new SlownikObiektowGeometrycznych();
            slownikObiektowGeometrycznych.Load(path);

            Core.Classes.Range<int> range = new Core.Classes.Range<int>(2001, 2023);
            string directory = Path.GetDirectoryName(path);

            directory = Path.Combine(directory, "Results");
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            directory = Path.Combine(directory, Path.GetFileNameWithoutExtension(path));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await Modify.Write(slownikObiektowGeometrycznych, directory, range);

            return true;
        }

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "zip files (*.zip)|*.zip|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog(this);
            if (result == null || !result.HasValue || !result.Value)
            {
                return;
            }

            string path = openFileDialog.FileName;
            if(string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            await Modify.Write(path, new Core.Classes.Range<int>(2001, 2023));
        }
    }
}