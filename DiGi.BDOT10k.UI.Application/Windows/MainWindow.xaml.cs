using Microsoft.Win32;
using System.Windows;
using DiGi.BDOT10k.UI.Classes;
using DiGi.BDOT10k.Classes;
using System.IO.Compression;
using System.IO;

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

        private void Load_Old() 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog(this);
            if (result == null || !result.HasValue || !result.Value)
            {
                return;
            }

            string path = openFileDialog.FileName;

            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return;
            }

            slownikObiektowGeometrycznych.Load(path);
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

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            Convert_New();
        }
    }
}