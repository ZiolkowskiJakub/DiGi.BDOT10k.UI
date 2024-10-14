using DiGi.BDOT10k.Interfaces;
using DiGi.BDOT10k.UI.Classes;
using DiGi.BDOT10k.UI.Interfaces;
using DiGi.GML.Classes;
using DiGi.GML.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiGi.BDOT10k.UI
{
    public static partial class Create
    {
        public static bool Load(this SlownikObiektowGeometrycznych slownikObiektowGeometrycznych, Stream stream)
        {
            if (slownikObiektowGeometrycznych == null || stream == null)
            {
                return false;
            }

            FeatureCollection featureCollection = GML.Convert.ToGML<FeatureCollection>(stream)?.FirstOrDefault();
            if (featureCollection == null)
            {
                return false;
            }

            return Load(slownikObiektowGeometrycznych, featureCollection);
        }


        public static bool Load(this SlownikObiektowGeometrycznych slownikObiektowGeometrycznych, string path)
        {
            if(slownikObiektowGeometrycznych == null || string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return false;
            }

            FeatureCollection featureCollection = GML.Convert.ToGML<FeatureCollection>(path)?.FirstOrDefault();
            if (featureCollection == null)
            {
                return false;
            }

            return Load(slownikObiektowGeometrycznych, featureCollection);
        }

        public static bool Load(this SlownikObiektowGeometrycznych slownikObiektowGeometrycznych, FeatureCollection featureCollection)
        {
            if (slownikObiektowGeometrycznych == null || featureCollection == null)
            {
                return false;
            }

            List<IFeatureMember> featureMembers = featureCollection.featureMember;
            if (featureMembers == null || featureMembers.Count == 0)
            {
                return false;
            }
            bool result = false;
            foreach (IFeatureMember featureMember in featureMembers)
            {
                if (!(featureMember is IOT_ObiektGeometryczny))
                {
                    continue;
                }

                IObiektGeometryczny obiektGeometryczny = Convert.ToDiGi((IOT_ObiektGeometryczny)featureMember);
                if (obiektGeometryczny == null)
                {
                    continue;
                }

                bool added = slownikObiektowGeometrycznych.Add(obiektGeometryczny);
                if (added)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
