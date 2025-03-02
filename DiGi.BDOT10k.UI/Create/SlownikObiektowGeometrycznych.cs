﻿using DiGi.BDOT10k.UI.Classes;
using DiGi.GML.Classes;
using System.Linq;

namespace DiGi.BDOT10k.UI
{
    public static partial class Create
    {
        public static SlownikObiektowGeometrycznych SlownikObiektowGeometrycznych(this FeatureCollection featureCollection)
        {
            if (featureCollection == null)
            {
                return null;
            }

            SlownikObiektowGeometrycznych result = new SlownikObiektowGeometrycznych();
            result.Load(featureCollection);
            return result;
        }

        public static SlownikObiektowGeometrycznych SlownikObiektowGeometrycznych(string path)
        {
            if(path != null && !System.IO.File.Exists(path))
            {
                return null;
            }

            FeatureCollection featureCollection = GML.Convert.ToGML<FeatureCollection>(path)?.FirstOrDefault();
            if(featureCollection == null)
            {
                return null;
            }

            return SlownikObiektowGeometrycznych(featureCollection);
        }
    }
}
