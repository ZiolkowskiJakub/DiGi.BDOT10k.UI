using DiGi.BDOT10k.Classes;
using DiGi.BDOT10k.Interfaces;
using DiGi.BDOT10k.UI.Classes;
using DiGi.BDOT10k.UI.Interfaces;

namespace DiGi.BDOT10k.UI
{
    public static partial class Convert
    {
        public static IObiektGeometryczny ToDiGi(this IOT_ObiektGeometryczny oT_ObiektGeometryczny)
        {
            if(oT_ObiektGeometryczny == null)
            {
                return null;
            }

            if(oT_ObiektGeometryczny is OT_ADMS_A)
            {
                return new ADMS_A((OT_ADMS_A)oT_ObiektGeometryczny, null);
            }
            else if (oT_ObiektGeometryczny is OT_BUBD_A)
            {
                return new BUBD_A((OT_BUBD_A)oT_ObiektGeometryczny, null);
            }

            throw new System.NotImplementedException();
        }
    }
}
