using DiGi.BDOT10k.Interfaces;
using DiGi.Geometry.Visual.Core.Interfaces;
using DiGi.Geometry.Visual.Planar.Classes;

namespace DiGi.BDOT10k.UI.Classes
{
    public class PowierzchniowyObiektGeometryczny : VisualPolygonalFace2D
    {
        private IOT_PowierzchniowyObiektGeometryczny oT_PowierzchniowyObiektGeometryczny;

        public PowierzchniowyObiektGeometryczny(IOT_PowierzchniowyObiektGeometryczny oT_PowierzchniowyObiektGeometryczny, ISurfaceAppearance surfaceAppearance)
            : base(Convert.ToDiGi(oT_PowierzchniowyObiektGeometryczny?.geometria), surfaceAppearance)
        {
            this.oT_PowierzchniowyObiektGeometryczny = GML.Query.Clone(oT_PowierzchniowyObiektGeometryczny);
        }

        public IOT_PowierzchniowyObiektGeometryczny OT_PowierzchniowyObiektGeometryczny
        {
            get
            {
                return GML.Query.Clone(oT_PowierzchniowyObiektGeometryczny);
            }
        }

    }
}
