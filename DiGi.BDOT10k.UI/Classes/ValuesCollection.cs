using System.Collections;
using System.Collections.Generic;

namespace DiGi.BDOT10k.UI.Classes
{
    public class ValuesCollection : IEnumerable<Values>
    {
        private List<Values> list = new List<Values>();

        public ValuesCollection(IEnumerable<Values> values)
        {
            list = values == null ? new List<Values>() : new List<Values>(values);
        }
        public ValuesCollection()
        {

        }

        public void Add(Values values)
        {
            list.Add(values);
        }

        public IEnumerator<Values> GetEnumerator()
        {
            return list == null ? new List<Values>().GetEnumerator() : list?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public HashSet<string> GetKeys()
        {
            if(list == null)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string> ();
            foreach(Values values in list)
            {
                HashSet<string> keys = values?.Keys;
                if(keys == null)
                {
                    continue;
                }

                foreach (string key in keys)
                {
                    result.Add (key);
                }
            }
            return result;
        }

        public bool Write(string path, string separator = "\t")
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            HashSet<string> keys = GetKeys();

            return Write(path, keys, separator);

        }

        public bool Write(string path, IEnumerable<string> keys, string separator = "\t")
        {
            if (string.IsNullOrWhiteSpace(path) || keys == null)
            {
                return false;
            }

            List<string> lines = new List<string>() { string.Join(separator, keys) };
            if(list != null)
            {
                foreach (Values values in list)
                {
                    string line = values?.ToString(keys, separator);

                    lines.Add(line == null ? string.Empty : line);
                }
            }

            System.IO.File.WriteAllLines(path, lines);
            return true;
        }
    }
}
