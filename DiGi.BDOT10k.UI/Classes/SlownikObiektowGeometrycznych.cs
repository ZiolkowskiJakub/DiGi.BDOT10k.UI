using DiGi.BDOT10k.UI.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI.Classes
{
    public class SlownikObiektowGeometrycznych
    {
        private Dictionary<Type, List<IObiektGeometryczny>> dictionary = new Dictionary<Type, List<IObiektGeometryczny>>();

        public SlownikObiektowGeometrycznych()
        {

        }

        public bool Add(IObiektGeometryczny obiektGeometryczny)
        {
            if(obiektGeometryczny == null)
            {
                return false;
            }

            Type type = obiektGeometryczny.GetType();

            if(!dictionary.TryGetValue(type, out List<IObiektGeometryczny> obiektyGeometryczne) || obiektyGeometryczne == null)
            {
                obiektyGeometryczne = new List<IObiektGeometryczny>();
                dictionary[type] = obiektyGeometryczne;
            }

            obiektyGeometryczne.Add(obiektGeometryczny);
            return true;
        }

        public List<T> GetObiektyGeometryczne<T>(Func<T, bool> func = null) where T : IObiektGeometryczny
        {
            if(dictionary == null)
            {
                return null;
            }

            Type type = typeof(T);

            List<T> result = new List<T>();
            foreach (KeyValuePair<Type, List<IObiektGeometryczny>> keyValuePair in dictionary)
            {
                if(!type.IsAssignableFrom(keyValuePair.Key))
                {
                    continue;
                }

                foreach(IObiektGeometryczny obiektGeometryczny in keyValuePair.Value)
                {
                    if(!(obiektGeometryczny is T))
                    {
                        continue;
                    }

                    T t  = (T)obiektGeometryczny;

                    if(func != null && !func.Invoke(t))
                    {
                        continue;
                    }

                    result.Add(t);
                }

                return result;
            }

            return result;
        }

        public T GetObiektGeometryczny<T>(Func<T, bool> func = null) where T : IObiektGeometryczny
        {
            if (dictionary == null)
            {
                return default;
            }

            Type type = typeof(T);

            foreach (KeyValuePair<Type, List<IObiektGeometryczny>> keyValuePair in dictionary)
            {
                if (!type.IsAssignableFrom(keyValuePair.Key))
                {
                    continue;
                }

                foreach (IObiektGeometryczny obiektGeometryczny in keyValuePair.Value)
                {
                    if (!(obiektGeometryczny is T))
                    {
                        continue;
                    }

                    T t = (T)obiektGeometryczny;

                    if (func != null && !func.Invoke(t))
                    {
                        continue;
                    }

                    return t;
                }
            }

            return default;
        }
    }
}
