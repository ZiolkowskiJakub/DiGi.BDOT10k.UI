using DiGi.BDOT10k.Interfaces;
using DiGi.Geometry.Visual.Core.Interfaces;
using DiGi.Geometry.Visual.Planar.Classes;
using DiGi.BDOT10k.UI.Interfaces;
using DiGi.Geometry.Planar.Classes;

namespace DiGi.BDOT10k.UI.Classes
{
    public abstract class PowierzchniowyObiektGeometryczny<T> : VisualPolygonalFace2D, IObiektGeometryczny<PolygonalFace2D, ISurfaceAppearance> where T: IOT_PowierzchniowyObiektGeometryczny
    {
        protected T oT_PowierzchniowyObiektGeometryczny;

        private double area = double.NaN;
        private BoundingBox2D boundingBox2D = null;
        private Point2D internalPoint2D = null;

        public PowierzchniowyObiektGeometryczny(T oT_PowierzchniowyObiektGeometryczny, ISurfaceAppearance surfaceAppearance)
            : base(Convert.ToDiGi(oT_PowierzchniowyObiektGeometryczny?.geometria), surfaceAppearance)
        {
            this.oT_PowierzchniowyObiektGeometryczny = GML.Query.Clone(oT_PowierzchniowyObiektGeometryczny);
        }

        public T OT_PowierzchniowyObiektGeometryczny
        {
            get
            {
                return GML.Query.Clone(oT_PowierzchniowyObiektGeometryczny);
            }
        }

        public System.Type GetUnderlyingType()
        {
            return oT_PowierzchniowyObiektGeometryczny?.GetType();
        }

        public double Area
        {
            get
            {
                if(double.IsNaN(area) && Geometry != null)
                {
                    area = Geometry.GetArea();
                }

                return area;
            }
        }

        public BoundingBox2D BoundingBox2D
        {
            get
            {
                if (boundingBox2D == null && Geometry != null)
                {
                    boundingBox2D = Geometry.GetBoundingBox();
                }

                return boundingBox2D == null ? null : new BoundingBox2D(boundingBox2D);
            }
        }

        public Point2D InternalPoint2D
        {
            get
            {
                if (internalPoint2D == null && Geometry != null)
                {
                    internalPoint2D = Geometry.GetInternalPoint();
                }

                return internalPoint2D == null ? null : new Point2D(internalPoint2D);
            }
        }
    }
}