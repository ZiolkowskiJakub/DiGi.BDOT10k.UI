using DiGi.BDOT10k.Classes;
using DiGi.BDOT10k.UI.Classes;
using System.Collections.Generic;
using System;
using System.Linq;
using DiGi.Geometry.Planar.Classes;
using System.Net.Http;
using System.IO;

namespace DiGi.BDOT10k.UI
{
    public static partial class Convert
    {
        public static ValuesCollection ToDiGi(this SlownikObiektowGeometrycznych slownikObiektowGeometrycznych, Report report = null)
        {
            if (slownikObiektowGeometrycznych == null)
            {
                return null;
            }

            List<ADMS_A> aDMS_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<ADMS_A>();

            List<ADMS_A> aDMS_As_LiczbaMieszkancow = new List<ADMS_A>();
            foreach(ADMS_A aDMS_A in aDMS_As)
            {
                OT_ADMS_A oT_ADMS_A = aDMS_A?.OT_PowierzchniowyObiektGeometryczny;
                if(oT_ADMS_A == null)
                {
                    continue;
                }

                if(oT_ADMS_A.liczbaMieszkancow == null || !oT_ADMS_A.liczbaMieszkancow.HasValue || oT_ADMS_A.liczbaMieszkancow.Value == 0)
                {
                    continue;
                }

                aDMS_As_LiczbaMieszkancow.Add(aDMS_A);
            }

            List<Tuple<string, ADMS_A, List<Tuple<BUBD_A, ADMS_A>>>> tuples = new List<Tuple<string, ADMS_A, List<Tuple<BUBD_A, ADMS_A>>>>();

            List<BUBD_A> bUBD_As = slownikObiektowGeometrycznych.GetObiektyGeometryczne<BUBD_A>();
            foreach (BUBD_A bUBD_A in bUBD_As)
            {
                OT_BUBD_A oT_BUBD_A = bUBD_A.OT_PowierzchniowyObiektGeometryczny;
                if (oT_BUBD_A == null)
                {
                    continue;
                }

                if (oT_BUBD_A.funkcjaOgolnaBudynku != BDOT10k.Enums.OT_FunOgolnaBudynku.budynki_mieszkalne)
                {
                    continue;
                }

                if (oT_BUBD_A.funkcjaSzczegolowaBudynku == null)
                {
                    continue;
                }

                if (!oT_BUBD_A.funkcjaSzczegolowaBudynku.Contains(BDOT10k.Enums.OT_FunSzczegolowaBudynkuType.budynek_jednorodzinny) && !oT_BUBD_A.funkcjaSzczegolowaBudynku.Contains(BDOT10k.Enums.OT_FunSzczegolowaBudynkuType.budynek_wielorodzinny))
                {
                    continue;
                }

                if (oT_BUBD_A.kategoriaIstnienia != BDOT10k.Enums.OT_KatIstnienia.eksploatowany)
                {
                    continue;
                }

                ADMS_A aDMS_A = Query.ADMS_A(aDMS_As, bUBD_A);
                if(aDMS_A == null)
                {
                    report?.Add(Enums.ReportType.Error, bUBD_A?.OT_PowierzchniowyObiektGeometryczny?.identyfikatorEGiB?.FirstOrDefault(), "Could not find OT_ADMS_A for given OT_BUBD_A");
                    continue;
                    //throw new NotImplementedException();
                }

                ADMS_A aDMS_A_LiczbaMieszkancow = Query.ADMS_A(aDMS_As_LiczbaMieszkancow, bUBD_A);
                if (aDMS_A_LiczbaMieszkancow == null)
                {
                    report?.Add(Enums.ReportType.Error, bUBD_A?.OT_PowierzchniowyObiektGeometryczny?.identyfikatorEGiB?.FirstOrDefault(), "Could not find OT_ADMS_A with number of people for given OT_BUBD_A");
                    continue;
                    //throw new NotImplementedException();
                }

                string uniqueId = aDMS_A_LiczbaMieszkancow.OT_PowierzchniowyObiektGeometryczny?.identyfikatorSIMC;
                if(string.IsNullOrWhiteSpace(uniqueId))
                {
                    report?.Add(Enums.ReportType.Error, bUBD_A?.OT_PowierzchniowyObiektGeometryczny?.identyfikatorEGiB?.FirstOrDefault(), "Could not find OT_ADMS_A value \"identyfikatorSIMC\" for given OT_BUBD_A");
                    continue;
                    //throw new NotImplementedException();
                }

                Tuple<string, ADMS_A, List<Tuple<BUBD_A, ADMS_A>>> tuple = tuples.Find(x => x.Item1 == uniqueId);
                if (tuple == null)
                {
                    tuple = new Tuple<string, ADMS_A, List<Tuple<BUBD_A, ADMS_A>>>(uniqueId, aDMS_A_LiczbaMieszkancow, new List<Tuple<BUBD_A, ADMS_A>>());
                    tuples.Add(tuple);
                }

                tuple.Item3.Add(new Tuple<BUBD_A, ADMS_A>(bUBD_A, aDMS_A));
            }

            ValuesCollection result = new ValuesCollection();
            foreach (Tuple<string, ADMS_A, List<Tuple<BUBD_A, ADMS_A>>> tuple in tuples)
            {
                ADMS_A aDMS_A = tuple.Item2;

                uint? liczbaMieszkancow_Temp = Query.LiczbaMieszkancow(aDMS_As_LiczbaMieszkancow, aDMS_A);
                if (liczbaMieszkancow_Temp == null || !liczbaMieszkancow_Temp.HasValue || liczbaMieszkancow_Temp.Value == 0)
                {
                    report?.Add(Enums.ReportType.Error, aDMS_A?.OT_PowierzchniowyObiektGeometryczny?.identyfikatorSIMC, "Could not calculate number of people for OT_ADMS_A");
                    continue;
                    //throw new NotImplementedException();
                }

                uint liczbaMieszkancow = liczbaMieszkancow_Temp.Value;

                List<BUBD_A> bUBD_As_ADMS_A = tuple.Item3.ConvertAll(x => x.Item1);

                double area = 0;
                foreach (BUBD_A bUBD_A in bUBD_As_ADMS_A)
                {
                    int liczbaKondygnacji = 1;
                    OT_BUBD_A oT_BUBD_A = bUBD_A.OT_PowierzchniowyObiektGeometryczny;
                    if(oT_BUBD_A?.liczbaKondygnacji != null && oT_BUBD_A.liczbaKondygnacji.HasValue)
                    {
                        liczbaKondygnacji = oT_BUBD_A.liczbaKondygnacji.Value;
                    }

                    area += bUBD_A.Area * liczbaKondygnacji;
                }

                double factor = liczbaMieszkancow / area;

                Dictionary<BUBD_A, int> dictionary = new Dictionary<BUBD_A, int>();

                int count = 0;
                foreach (BUBD_A bUBD_A in bUBD_As_ADMS_A)
                {
                    if (count >= liczbaMieszkancow)
                    {
                        dictionary[bUBD_A] = 0;
                        continue;
                    }

                    int count_BUBD_A = System.Convert.ToInt32(System.Math.Floor(bUBD_A.Area * factor));
                    if(count_BUBD_A == 0)
                    {
                        count_BUBD_A = 1;
                    }

                    if (count + count_BUBD_A > liczbaMieszkancow)
                    {
                        count_BUBD_A = System.Convert.ToInt32(liczbaMieszkancow) - count;
                    }

                    dictionary[bUBD_A] = count_BUBD_A;
                    count += count_BUBD_A;
                }

                if (count < liczbaMieszkancow)
                {
                    Random random = new Random((int)liczbaMieszkancow);

                    Core.Classes.Range<int> range = new Core.Classes.Range<int>(0, bUBD_As_ADMS_A.Count - 1);

                    while (count < liczbaMieszkancow)
                    {
                        int index = Core.Query.Random(random, range);
                        dictionary[bUBD_As_ADMS_A[index]]++;
                        count++;
                    }
                }

                foreach (Tuple<BUBD_A, ADMS_A> tuple_BUBD_A in tuple.Item3)
                {
                    BUBD_A bUBD_A = tuple_BUBD_A.Item1;
                    ADMS_A aDMS_A_BUBD_A = tuple_BUBD_A.Item2;

                    OT_BUBD_A oT_BUBD_A = bUBD_A.OT_PowierzchniowyObiektGeometryczny;
                    OT_ADMS_A oT_ADMS_A_BUBD_A = aDMS_A_BUBD_A.OT_PowierzchniowyObiektGeometryczny;

                    Values values = new Values();
                    values["przestrzenNazw"] = oT_BUBD_A.przestrzenNazw;
                    values["kodKarto10k"] = oT_BUBD_A.kodKarto10k;
                    values["identyfikatorEGiB"] = oT_BUBD_A.identyfikatorEGiB?.FirstOrDefault();
                    values["liczbaKondygnacji"] = oT_BUBD_A.liczbaKondygnacji;
                    values["funkcjaOgolnaBudynku"] = oT_BUBD_A.funkcjaOgolnaBudynku == null ? null : GML.Query.Description(oT_BUBD_A.funkcjaOgolnaBudynku);
                    values["przewazajacaFunkcjaBudynku"] = oT_BUBD_A.przewazajacaFunkcjaBudynku == null ? null : GML.Query.Description(oT_BUBD_A.przewazajacaFunkcjaBudynku);
                    values["funkcjaSzczegolowaBudynku"] = oT_BUBD_A.funkcjaSzczegolowaBudynku == null ? null : string.Join(";", oT_BUBD_A.funkcjaSzczegolowaBudynku?.ConvertAll(x => GML.Query.Description(x)));

                    values["powierzchniaPiętra"] = System.Math.Round(bUBD_A.Area, 2);
                    values["powierzchnia"] = bUBD_A.Area * oT_BUBD_A.liczbaKondygnacji;
                    values["liczbaMieszkancowBudynku"] = dictionary[bUBD_A];

                    values["lokalizacjaX"] = System.Math.Round(bUBD_A.InternalPoint2D.X, 2);
                    values["lokalizacjaY"] = System.Math.Round(bUBD_A.InternalPoint2D.Y, 2);

                    values["miejscowosc_1"] = oT_ADMS_A_BUBD_A.nazwa;
                    values["rodzajMiejscowosci_1"] = GML.Query.Description(oT_ADMS_A_BUBD_A.rodzaj);
                    values["liczbaMieszkancow_1"] = oT_ADMS_A_BUBD_A.liczbaMieszkancow;

                    values["miejscowosc_2"] = aDMS_A.OT_PowierzchniowyObiektGeometryczny.nazwa;
                    values["rodzajMiejscowosci_2"] = GML.Query.Description(aDMS_A.OT_PowierzchniowyObiektGeometryczny.rodzaj);
                    values["liczbaMieszkancow_2"] = liczbaMieszkancow;
                    values["wspolczynnik_2"] = factor;

                    result.Add(values);
                }
            }

            return result;
        }
    }
}
