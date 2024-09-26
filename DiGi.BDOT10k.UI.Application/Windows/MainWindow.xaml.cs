using Microsoft.Win32;
using System.Windows;
using DiGi.BDOT10k.UI.Classes;
using DiGi.BDOT10k.Classes;
using DiGi.Geometry.Planar.Classes;
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

        private void Load_New()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "zip files (*.zip)|*.zip|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog(this);
            if (result == null || !result.HasValue || !result.Value)
            {
                return;
            }

            string path = openFileDialog.FileName;

            using (ZipArchive zipArchive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    Stream stream = zipArchiveEntry.Open();
                    using (ZipArchive zipArchive_ZipArchieve = new ZipArchive(stream))
                    {
                        foreach (ZipArchiveEntry zipArchiveEntry_Zip in zipArchive_ZipArchieve.Entries)
                        {
                            string name = zipArchiveEntry_Zip.Name;
                            string fullName = zipArchiveEntry_Zip.FullName;
                        }
                    }
                }
            }
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

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Load_New();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            bool? result = openFolderDialog.ShowDialog(this);
            if (result != true)
            {
                return;
            }

            List<ADMS_A> ADMS_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<ADMS_A>();

            List<Tuple<string, ADMS_A, List<BUBD_A>>> tuples = new List<Tuple<string, ADMS_A, List<BUBD_A>>>();

            List<BUBD_A> BUBD_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<BUBD_A>();
            foreach (BUBD_A bUBD_A in BUBD_As)
            {
                OT_BUBD_A oT_BUBD_A = bUBD_A.OT_PowierzchniowyObiektGeometryczny;
                if (oT_BUBD_A == null)
                {
                    continue;
                }

                if (oT_BUBD_A.funkcjaOgolnaBudynku != Enums.OT_FunOgolnaBudynku.budynki_mieszkalne)
                {
                    continue;
                }

                if (oT_BUBD_A.funkcjaSzczegolowaBudynku == null)
                {
                    continue;
                }

                if(!oT_BUBD_A.funkcjaSzczegolowaBudynku.Contains(Enums.OT_FunSzczegolowaBudynkuType.budynek_jednorodzinny) && !oT_BUBD_A.funkcjaSzczegolowaBudynku.Contains(Enums.OT_FunSzczegolowaBudynkuType.budynek_wielorodzinny))
                {
                    continue;
                }

                if (oT_BUBD_A.kategoriaIstnienia != Enums.OT_KatIstnienia.eksploatowany)
                {
                    continue;
                }

                Point2D point2D = bUBD_A.InternalPoint2D;
                if(point2D == null)
                {
                    continue;
                }

                List<ADMS_A> aDMS_As_BUBD_A = ADMS_As.FindAll(x => x.BoundingBox2D.InRange(point2D) && x.Geometry.InRange(point2D));
                if(aDMS_As_BUBD_A == null || BUBD_As.Count == 0)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (aDMS_As_BUBD_A.Count > 1)
                    {
                        aDMS_As_BUBD_A.Sort((x, y) => x.Area.CompareTo(y.Area));
                    }

                    ADMS_A aDMS_A = aDMS_As_BUBD_A[0];

                    OT_ADMS_A oT_ADMS_A = aDMS_A.OT_PowierzchniowyObiektGeometryczny;
                    if (oT_ADMS_A == null)
                    {
                        throw new NotImplementedException();
                    } 
                    else
                    {
                        string uniqueId = oT_ADMS_A.identyfikatorSIMC;

                        Tuple<string, ADMS_A, List<BUBD_A>> tuple = tuples.Find(x => x.Item1 == uniqueId);
                        if (tuple == null)
                        {
                            tuple = new Tuple<string, ADMS_A, List<BUBD_A>>(uniqueId, aDMS_A, new List<BUBD_A>());
                            tuples.Add(tuple);
                        }

                        tuple.Item3.Add(bUBD_A);
                    }
                }
            }

            ValuesCollection valuesCollection = new ValuesCollection();
            foreach(Tuple<string, ADMS_A, List<BUBD_A>> tuple in tuples)
            {
                OT_ADMS_A oT_ADMS_A = tuple?.Item2?.OT_PowierzchniowyObiektGeometryczny;
                if(oT_ADMS_A == null || oT_ADMS_A.liczbaMieszkancow == null || !oT_ADMS_A.liczbaMieszkancow.HasValue)
                {
                    throw new NotImplementedException();
                }

                uint liczbaMieszkancow = oT_ADMS_A.liczbaMieszkancow.Value;
                double area = 0;
                foreach(BUBD_A bUBD_A in tuple.Item3)
                {
                    area += bUBD_A.Area * bUBD_A.OT_PowierzchniowyObiektGeometryczny.liczbaKondygnacji.Value;
                }

                double factor = liczbaMieszkancow / area;

                Dictionary<BUBD_A, int> dictionary = new Dictionary<BUBD_A, int>();

                int count = 0;
                foreach (BUBD_A bUBD_A in tuple.Item3)
                {
                    int count_BUBD_A = System.Convert.ToInt32(Math.Floor(bUBD_A.Area * factor));
                    if(count + count_BUBD_A > liczbaMieszkancow)
                    {
                        count_BUBD_A = System.Convert.ToInt32(liczbaMieszkancow) - count;
                    }

                    dictionary[bUBD_A] = count_BUBD_A;
                    count += count_BUBD_A;

                    if (count >= liczbaMieszkancow)
                    {
                        break;
                    }
                }

                if(count < liczbaMieszkancow)
                {
                    Random random = new Random((int)liczbaMieszkancow);

                    Core.Classes.Range<int> range = new Core.Classes.Range<int>(0, tuple.Item3.Count - 1);

                    while(count < liczbaMieszkancow)
                    {
                        int index = Core.Query.Random(random, range);
                        dictionary[tuple.Item3[index]]++;
                        count++;
                    }
                }

                foreach (BUBD_A bUBD_A in tuple.Item3)
                {
                    OT_BUBD_A oT_BUBD_A = bUBD_A.OT_PowierzchniowyObiektGeometryczny;

                    Values values = new Values();
                    values["przestrzenNazw"] = oT_BUBD_A.przestrzenNazw;
                    values["kodKarto10k"] = oT_BUBD_A.kodKarto10k;
                    values["identyfikatorEGiB"] = oT_BUBD_A.identyfikatorEGiB?.FirstOrDefault();
                    values["liczbaKondygnacji"] = oT_BUBD_A.liczbaKondygnacji;
                    values["funkcjaOgolnaBudynku"] = oT_BUBD_A.funkcjaOgolnaBudynku == null ? null : GML.Query.Description(oT_BUBD_A.funkcjaOgolnaBudynku);
                    values["przewazajacaFunkcjaBudynku"] = oT_BUBD_A.przewazajacaFunkcjaBudynku == null ? null : GML.Query.Description(oT_BUBD_A.przewazajacaFunkcjaBudynku);
                    values["funkcjaSzczegolowaBudynku"] = oT_BUBD_A.funkcjaSzczegolowaBudynku == null ? null : string.Join(";", oT_BUBD_A.funkcjaSzczegolowaBudynku?.ConvertAll(x => GML.Query.Description(x)));

                    values["lokalizacjaX"] = Math.Round(bUBD_A.InternalPoint2D.X, 2);
                    values["lokalizacjaY"] = Math.Round(bUBD_A.InternalPoint2D.Y, 2);
                    values["powierzchniaPiętra"] = Math.Round(bUBD_A.Area, 2);
                    values["powierzchnia"] = bUBD_A.Area * oT_BUBD_A.liczbaKondygnacji;
                    values["liczbaMieszkancowBudynku"] = dictionary[bUBD_A];

                    values["miejscowosc"] = oT_ADMS_A.nazwa;
                    values["rodzajMiejscowosci"] = GML.Query.Description(oT_ADMS_A.rodzaj);
                    values["liczbaMieszkancow"] = oT_ADMS_A.liczbaMieszkancow;

                    valuesCollection.Add(values);
                }
            }

            valuesCollection.Write(System.IO.Path.Combine(openFolderDialog.FolderName, "result.txt"));
        }
    }
}