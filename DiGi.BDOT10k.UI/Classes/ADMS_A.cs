using DiGi.BDOT10k.Classes;
using DiGi.Geometry.Visual.Core.Interfaces;

namespace DiGi.BDOT10k.UI.Classes
{
    public class ADMS_A : PowierzchniowyObiektGeometryczny<OT_ADMS_A>
    {
        public ADMS_A(OT_ADMS_A oT_PowierzchniowyObiektGeometryczny, ISurfaceAppearance surfaceAppearance) 
            : base(oT_PowierzchniowyObiektGeometryczny, surfaceAppearance)
        {
        }
    }
}
