
using DiGi.BDOT10k.UI.Classes;
using DiGi.Geometry.Planar.Classes;
using System;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI
{
    public static partial class Query
    {
        public static ADMS_A ADMS_A(this IEnumerable<ADMS_A> aDMS_As, BUBD_A bUBD_A)
        {
            if(aDMS_As == null || bUBD_A == null)
            {
                return null;
            }

            Point2D point2D = bUBD_A.InternalPoint2D;
            if (point2D == null)
            {
                return null;
            }

            List<ADMS_A> aDMS_As_Temp = new List<ADMS_A>(aDMS_As);

            List<ADMS_A> aDMS_As_BUBD_A = aDMS_As_Temp.FindAll(x => x?.BoundingBox2D != null && x.BoundingBox2D.InRange(point2D) && x.Geometry.InRange(point2D));

            if (aDMS_As_BUBD_A == null || aDMS_As_BUBD_A.Count == 0)
            {
                BoundingBox2D boundingBox2D = bUBD_A.BoundingBox2D;
                if (boundingBox2D == null)
                {
                    return null;
                }

                List<Point2D> point2Ds = bUBD_A.Geometry.ExternalEdge?.GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    return null;
                }

                foreach (Point2D point2D_Temp in point2Ds)
                {
                    aDMS_As_BUBD_A = aDMS_As_Temp.FindAll(x => x.BoundingBox2D.InRange(point2D_Temp) && x.Geometry.InRange(point2D_Temp));
                    if (aDMS_As_BUBD_A != null && aDMS_As_BUBD_A.Count > 0)
                    {
                        point2D = point2D_Temp;
                        break;
                    }
                }
            }

            if (aDMS_As_BUBD_A == null || aDMS_As_BUBD_A.Count == 0)
            {
                List<Tuple<double, ADMS_A>> tuples_ADMS_A = aDMS_As_Temp.ConvertAll(x => new Tuple<double, ADMS_A>(x.Geometry.ExternalEdge.Distance(point2D), x));
                tuples_ADMS_A.Sort((x, y) => x.Item1.CompareTo(y.Item1));

                Point2D point2D_Temp = tuples_ADMS_A[0].Item2.Geometry.ExternalEdge.ClosestPoint(point2D);
                aDMS_As_BUBD_A = aDMS_As_Temp.FindAll(x => x.BoundingBox2D.InRange(point2D_Temp) && x.Geometry.InRange(point2D_Temp));
                if (aDMS_As_BUBD_A == null || aDMS_As_BUBD_A.Count == 0)
                {
                    aDMS_As_BUBD_A = new List<ADMS_A>() { tuples_ADMS_A[0].Item2 };
                }
            }

            if (aDMS_As_BUBD_A == null || aDMS_As_BUBD_A.Count == 0)
            {
                return null;
            }

            if (aDMS_As_BUBD_A.Count > 1)
            {
                aDMS_As_BUBD_A.Sort((x, y) => x.Area.CompareTo(y.Area));
            }

            return aDMS_As_BUBD_A[0];
        }
    }
}
