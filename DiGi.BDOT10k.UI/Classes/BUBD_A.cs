using DiGi.BDOT10k.Classes;
using DiGi.Geometry.Visual.Core.Interfaces;

namespace DiGi.BDOT10k.UI.Classes
{
    public class BUBD_A : PowierzchniowyObiektGeometryczny<OT_BUBD_A>
    {
        public BUBD_A(OT_BUBD_A oT_PowierzchniowyObiektGeometryczny, ISurfaceAppearance surfaceAppearance) 
            : base(oT_PowierzchniowyObiektGeometryczny, surfaceAppearance)
        {
        }
    }
}
