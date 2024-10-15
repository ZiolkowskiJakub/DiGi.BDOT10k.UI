using DiGi.BDOT10k.UI.Classes;
using DiGi.Core.Classes;
using DiGi.Geometry.Planar.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiGi.BDOT10k.UI
{
    public static partial class Query
    {
        public static async Task<Dictionary<int, byte[]>> BytesDictionary(this BUBD_A bUBD_A, Range<int> years, double offset = 5, double width = 300)
        {
            if(bUBD_A == null || years == null)
            {
                return null;
            }

            List<int> years_Temp = new List<int>();
            for (int i = years.Min; i <= years.Max; i++)
            {
                years_Temp.Add(i);
            }

            return await BytesDictionary(bUBD_A, years_Temp, offset, width);
        }

        public static async Task<Dictionary<int, byte[]>> BytesDictionary(this BUBD_A bUBD_A, IEnumerable<int> years, double offset = 5, double width = 300)
        {
            if(years == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = bUBD_A?.BoundingBox2D;
            if (boundingBox2D == null || boundingBox2D.GetArea() < 1)
            {
                return null;
            }

            boundingBox2D.Offset(offset);

            Point2D min = boundingBox2D.Min;
            Point2D max = boundingBox2D.Max;

            double deltaX = max.X - min.X;
            double deltaY = max.Y - min.Y;

            int width_Int = System.Convert.ToInt32(width);
            int height_Int = System.Convert.ToInt32(width / deltaX * deltaY);

            Func<int, Task<byte[]>> func = new Func<int, Task<byte[]>>(async year =>
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string url = string.Format("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolutionTime?REQUEST=GetMap&TRANSPARENT=TRUE&FORMAT=image%2Fjpeg&VERSION=1.1.0&LAYERS=Raster&STYLES=&EXCEPTIONS=application%2Fvnd.ogc.se_xml&TIME={0}&SRS=EPSG:2180&width={1}&height={2}&SERVICE=WMS&BBOX={3},{4},{5},{6}", year, width_Int, height_Int, min.X.ToString(CultureInfo.InvariantCulture), min.Y.ToString(CultureInfo.InvariantCulture), max.X.ToString(CultureInfo.InvariantCulture), max.Y.ToString(CultureInfo.InvariantCulture));

                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(url);
                        if (response == null || !response.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                return memoryStream.ToArray();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                    }
                }

                return null;
            });

            List<Task<byte[]>> tasks = new List<Task<byte[]>>();
            List<int> years_Temp = new List<int>();

            foreach (int year in years)
            {
                years_Temp.Add(year);
                tasks.Add(func(year));
            }

            byte[][] bytesArray = await Task.WhenAll(tasks);

            Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();
            for (int i = 0; i < bytesArray.Length; i++)
            {
                result[years_Temp[i]] = bytesArray[i];
            }

            return result;
        }
    }
}
