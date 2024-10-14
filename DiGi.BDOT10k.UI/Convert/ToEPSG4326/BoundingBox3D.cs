using DiGi.Geometry.Planar.Classes;
using DiGi.Geometry.Spatial.Classes;

namespace DiGi.BDOT10k.UI
{
    public static partial class Convert
    {
        public static BoundingBox3D ToEPSG4326(this BoundingBox2D boundingBox2D)
        {
            if(boundingBox2D == null)
            {
                return null;
            }

            return new BoundingBox3D(boundingBox2D.Min.ToEPSG4326(), boundingBox2D.Max.ToEPSG4326());
        }
    }
}
