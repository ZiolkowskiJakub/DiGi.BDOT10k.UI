using DiGi.Core;
using DiGi.Geometry.Planar.Classes;
using DiGi.GML.Classes;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI
{
    public static partial class Convert
    {
        public static PolygonalFace2D ToDiGi(this Surface surface)
        {
            if(surface == null)
            {
                return null;
            }

            if(surface is Polygon)
            {
                Polygon polygon = (Polygon)surface;
                Polygon2D externalPolygon2D = polygon.exterior?.ToDiGi();
                if(externalPolygon2D == null)
                {
                    return null;
                }

                List<Polygon2D> internalPolygon2Ds = null;
                if(polygon.interior != null)
                {
                    internalPolygon2Ds = new List<Polygon2D>();
                    foreach(LinearRing linearRing in polygon.interior)
                    {
                        Polygon2D internalPolygon2D = linearRing?.ToDiGi();
                        if(internalPolygon2D == null)
                        {
                            continue;
                        }

                        internalPolygon2Ds.Add(internalPolygon2D);
                    }
                }

                return Geometry.Planar.Create.PolygonalFace2D(externalPolygon2D, internalPolygon2Ds);
            }

            throw new System.NotImplementedException();
        }
    }
}
