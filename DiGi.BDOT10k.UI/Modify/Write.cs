using DiGi.BDOT10k.UI.Classes;
using DiGi.Core.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace DiGi.BDOT10k.UI
{
    public static partial class Modify
    {
        public static async Task<bool> Write(this string path, Range<int> range = null)
        {
            if(string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            string directory = System.IO.Path.GetDirectoryName(path);
            directory = System.IO.Path.Combine(directory, "Results");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileName = "progressReport.txt";

            File.AppendAllText(System.IO.Path.Combine(directory, fileName), string.Format("[{0}] {1}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "START"));

            using (ZipArchive zipArchive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {
                    DeflateStream deflateStream = zipArchiveEntry.Open() as DeflateStream;
                    if (deflateStream == null)
                    {
                        continue;
                    }

                    using (ZipArchive zipArchive_ZipArchieve = new ZipArchive(deflateStream))
                    {
                        foreach (ZipArchiveEntry zipArchiveEntry_Zip in zipArchive_ZipArchieve.Entries)
                        {
                            File.AppendAllText(System.IO.Path.Combine(directory, fileName), string.Format("[{0}] {1} -> {2}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), zipArchiveEntry.Name, zipArchiveEntry_Zip.Name));

                            string path_Data = System.IO.Path.Combine(directory, System.IO.Path.GetFileNameWithoutExtension(zipArchiveEntry_Zip.Name) + ".txt");

                            DeflateStream deflateStream_Zip = zipArchiveEntry_Zip.Open() as DeflateStream;
                            if (deflateStream_Zip == null)
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

                            string path_Report = System.IO.Path.Combine(directory, string.Format("{0}_{1}", System.IO.Path.GetFileNameWithoutExtension(zipArchiveEntry_Zip.Name), "Report") + ".txt");

                            if (slownikObiektowGeometrycznych.GetObiektGeometryczny<BUBD_A>() == null || slownikObiektowGeometrycznych.GetObiektGeometryczny<ADMS_A>() == null)
                            {
                                File.WriteAllText(path, string.Empty);
                            }
                            else
                            {
                                Report report = new Report();

                                if (!File.Exists(path_Data))
                                {
                                    ValuesCollection valuesCollection = slownikObiektowGeometrycznych.ToDiGi(report);
                                    valuesCollection?.Write(path_Data);
                                }

                                if(range != null)
                                {
                                    string directory_Orto = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path_Data), System.IO.Path.GetFileNameWithoutExtension(path_Data));
                                    if(!Directory.Exists(directory_Orto))
                                    {
                                        Directory.CreateDirectory(directory_Orto);
                                    }

                                    await Write(slownikObiektowGeometrycznych, directory_Orto, range);
                                }

                                if (!report.IsEmpty())
                                {
                                    report.Write(path_Report);
                                }
                            }
                        };
                    }
                }
            }

            File.AppendAllText(System.IO.Path.Combine(directory, fileName), string.Format("[{0}] {1}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "END"));
            return true;
        }   

        public static async Task<bool> Write(this SlownikObiektowGeometrycznych slownikObiektowGeometrycznych, string directory, Range<int> range)
        {
            if(slownikObiektowGeometrycznych == null || string.IsNullOrWhiteSpace(directory) || range == null)
            {
                return false;
            }

            List<BUBD_A> bUBD_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<BUBD_A>();
            if(bUBD_As == null)
            {
                return true;
            }

            foreach (BUBD_A bUBD_A in bUBD_As)
            {
                await Write(bUBD_A, directory, range);
            }

            return true;
        }

        public static async Task<bool> Write(this BUBD_A bUBD_A, string directory, Range<int> range)
        {
            if (bUBD_A == null || string.IsNullOrWhiteSpace(directory) || range == null)
            {
                return false;
            }

            string? id = bUBD_A.OT_PowierzchniowyObiektGeometryczny.identyfikatorEGiB?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            string directory_BUBD_A = System.IO.Path.Combine(directory, id);
            if (!Directory.Exists(directory_BUBD_A))
            {
                Directory.CreateDirectory(directory_BUBD_A);
            }

            string directory_Orto = System.IO.Path.Combine(directory_BUBD_A, "orto");
            if (!Directory.Exists(directory_Orto))
            {
                Directory.CreateDirectory(directory_Orto);
            }

            Dictionary<int, string> dictionary_Path = new Dictionary<int, string>();
            for (int i = range.Min; i <= range.Max; i++)
            {
                string path_Orto = System.IO.Path.Combine(directory_Orto, string.Format("{0}.jpeg", i));
                if (path_Orto == null || File.Exists(path_Orto))
                {
                    continue;
                }

                dictionary_Path[i] = path_Orto;
            }

            if (dictionary_Path.Count == 0)
            {
                return true;
            }

            Dictionary<int, byte[]> dictionary = await Query.BytesDictionary(bUBD_A, dictionary_Path.Keys);
            if (dictionary == null)
            {
                return true;
            }

            foreach (KeyValuePair<int, byte[]> keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                string path_Orto = System.IO.Path.Combine(directory_Orto, string.Format("{0}.jpeg", keyValuePair.Key));

                using (Stream memoryStream = new MemoryStream(keyValuePair.Value), fileStream = new FileStream(path_Orto, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await memoryStream.CopyToAsync(fileStream);
                }
            }

            return true;
        }
    }
}
