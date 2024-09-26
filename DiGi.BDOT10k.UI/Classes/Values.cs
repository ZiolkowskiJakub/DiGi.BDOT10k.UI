using System.Collections.Generic;
using System.Linq;

namespace DiGi.BDOT10k.UI.Classes
{
    public class Values
    {
        private Dictionary<string, object> dictionary = new Dictionary<string, object>();

        public Values()
        {

        }

        public object this[string key]
        {
            get
            {
                if(key == null)
                {
                    return null;
                }

                if(dictionary.TryGetValue(key, out object result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                if(key == null)
                {
                    return;
                }

                dictionary[key] = value;
            }
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            if(key == null)
            {
                return false;
            }

            if(!dictionary.TryGetValue(key, out object @object))
            {
                return false;
            }

            return Core.Query.TryConvert(@object, out value);
        }

        public HashSet<string> Keys
        {
            get
            {
                return dictionary?.Keys == null ? null : new HashSet<string>(dictionary.Keys);
            }
        }

        public string ToString(IEnumerable<string> keys, string separator = "\t")
        {
            if(dictionary == null)
            {
                return string.Empty;
            }

            List<string> values = new List<string>();
            foreach (string key in keys)
            {
                if(!TryGetValue(key, out string value) || value == null)
                {
                    value = string.Empty;
                }

                values.Add(value);
            }

            return string.Join(separator, values);
        }
    }
}
