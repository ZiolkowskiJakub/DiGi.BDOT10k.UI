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

namespace DiGi.BDOT10k.UI.Application.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SlownikObiektowGeometrycznych slownikObiektowGeometrycznych = new SlownikObiektowGeometrycznych();

        public MainWindow()
        {
            InitializeComponent();

            OT_ADMS_A oT_ADMS_A = null;
        }

        private void Convert_New()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "zip files (*.zip)|*.zip|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog(this);
            if (result == null || !result.HasValue || !result.Value)
            {
                return;
            }

            string path = openFileDialog.FileName;

            string directory = Path.GetDirectoryName(path);
            directory = Path.Combine(directory, "Results");
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileName = "progressReport.txt";

            File.AppendAllText(Path.Combine(directory, fileName), string.Format("[{0}] {1}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "START"));

            using (ZipArchive zipArchive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    DeflateStream deflateStream = zipArchiveEntry.Open() as DeflateStream;
                    if(deflateStream == null)
                    {
                        continue;
                    }

                    using (ZipArchive zipArchive_ZipArchieve = new ZipArchive(deflateStream))
                    {
                        foreach (ZipArchiveEntry zipArchiveEntry_Zip in zipArchive_ZipArchieve.Entries)
                        {
                            File.AppendAllText(Path.Combine(directory, fileName), string.Format("[{0}] {1} -> {2}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), zipArchiveEntry.Name, zipArchiveEntry_Zip.Name));

                            string path_Data = Path.Combine(directory, Path.GetFileNameWithoutExtension(zipArchiveEntry_Zip.Name) + ".txt");
                            if(File.Exists(path_Data))
                            {
                                continue;
                            }

                            DeflateStream deflateStream_Zip = zipArchiveEntry_Zip.Open() as DeflateStream;
                            if(deflateStream_Zip == null)
                            {
                                continue;
                            }

                            ZipArchive zipArchive_Files = new ZipArchive(deflateStream_Zip);

                            SlownikObiektowGeometrycznych slownikObiektowGeometrycznych = new SlownikObiektowGeometrycznych();

                            foreach (ZipArchiveEntry zipArchiveEntry_File in zipArchive_Files.Entries)
                            {
                                if (zipArchiveEntry_File.Name.EndsWith("__OT_ADMS_A.xml") || zipArchiveEntry_File.Name.EndsWith("__OT_BUBD_A.xml"))
                                {
                                    slownikObiektowGeometrycznych.Load(zipArchiveEntry_File.Open());
                                }
                            }


                            string path_Report = Path.Combine(directory, string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(zipArchiveEntry_Zip.Name), "Report") + ".txt");

                            if (slownikObiektowGeometrycznych.GetObiektGeometryczny<BUBD_A>() == null || slownikObiektowGeometrycznych.GetObiektGeometryczny<ADMS_A>() == null)
                            {
                                File.WriteAllText(path, string.Empty);
                            }
                            else
                            {
                                Report report = new Report();
                                ValuesCollection valuesCollection = slownikObiektowGeometrycznych.ToDiGi(report);
                                valuesCollection?.Write(path_Data);
                                if (!report.IsEmpty())
                                {
                                    report.Write(path_Report);
                                }
                            }
                        };
                    }
                }
            }

            File.AppendAllText(Path.Combine(directory, fileName), string.Format("[{0}] {1}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "END"));
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

            List<BUBD_A> bUBD_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<BUBD_A>();
            foreach(BUBD_A bUBD_A in bUBD_As)
            {
                string? id = bUBD_A.OT_PowierzchniowyObiektGeometryczny.identyfikatorEGiB?.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                Dictionary<int, byte[]> dictionary = await Query.BytesDictionary(bUBD_A, range); 
                if(dictionary == null)
                {
                    continue;
                }

                string directory_BUBD_A = Path.Combine(directory, id);
                if (!Directory.Exists(directory_BUBD_A))
                {
                    Directory.CreateDirectory(directory_BUBD_A);
                }

                string directory_Orto = Path.Combine(directory_BUBD_A, "orto");
                if (!Directory.Exists(directory_Orto))
                {
                    Directory.CreateDirectory(directory_Orto);
                }

                foreach (KeyValuePair<int, byte[]> keyValuePair in dictionary)
                {
                    if(keyValuePair.Value == null)
                    {
                        continue;
                    }

                    string path_Orto = Path.Combine(directory_Orto, string.Format("{0}.jpeg", keyValuePair.Key));
                    using (MemoryStream memoryStream = new MemoryStream(keyValuePair.Value))
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromStream(memoryStream);
                        if (image != null)
                        {
                            image.Save(path_Orto);
                        }
                    }
                }
            }

            return true;
        }



        private void Convert_Old()
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            bool? result = openFolderDialog.ShowDialog(this);
            if (result != true)
            {
                return;
            }

            ValuesCollection valuesCollection = slownikObiektowGeometrycznych.ToDiGi();

            valuesCollection.Write(Path.Combine(openFolderDialog.FolderName, "result.txt"));
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Load_Old();
        }

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            await Load_Old();
        }
    }
}