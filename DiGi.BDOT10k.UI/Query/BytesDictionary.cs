using DiGi.BDOT10k.UI.Classes;
using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiGi.BDOT10k.UI
{
    public static partial class Query
    {
        public static async Task<Dictionary<int, byte[]>> BytesDictionary(this BUBD_A bUBD_A, Range<int> years, double offset = 5, double width = 300)
        {
            BoundingBox2D boundingBox2D = bUBD_A?.BoundingBox2D;
            if (boundingBox2D == null || boundingBox2D.GetArea() < 1)
            {
                return null;
            }

            boundingBox2D.Offset(offset);

            BoundingBox3D boundingBox3D = Convert.ToEPSG4326(boundingBox2D);

            Point2D min = new Point2D(boundingBox3D.Min.X, boundingBox3D.Min.Y);
            Point2D max = new Point2D(boundingBox3D.Max.X, boundingBox3D.Max.Y);

            double deltaX = max.X - min.X;
            double deltaY = max.Y - min.Y;

            int width_Int = System.Convert.ToInt32(width);
            int height_Int = System.Convert.ToInt32(width / deltaX * deltaY);

            Func<int, Task<byte[]>> func = new Func<int, Task<byte[]>>(async year => 
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string url = string.Format("https://mapy.geoportal.gov.pl/wss/service/PZGIK/ORTO/WMS/StandardResolutionTime?REQUEST=GetMap&TRANSPARENT=TRUE&FORMAT=image%2Fjpeg&VERSION=1.1.0&LAYERS=Raster&STYLES=&EXCEPTIONS=application%2Fvnd.ogc.se_xml&TIME={0}&SRS=EPSG%3A4326&width={1}&height={2}&SERVICE=WMS&BBOX={3},{4},{5},{6}", year, width_Int, height_Int, min.X.ToString(CultureInfo.InvariantCulture), min.Y.ToString(CultureInfo.InvariantCulture), max.X.ToString(CultureInfo.InvariantCulture), max.Y.ToString(CultureInfo.InvariantCulture));

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
            for (int i = years.Min; i <= years.Max; i++)
            {
                years_Temp.Add(i);
                tasks.Add(func(i));
            }

            byte[][] bytesArray = await Task.WhenAll(tasks);

            Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();
            for(int i = 0; i < bytesArray.Length; i++)
            {
                result[years_Temp[i]] = bytesArray[i];
            }

            return result;
        }
    }
}
