using System;

namespace VMGuide.FileFormat
{
    public class VirtualBoxFile: XmlHelper
    {
        public VirtualBoxFile(string path): base(path, "http://www.virtualbox.org/") {}

        public bool GetAttribute(string xpath, string name, bool defaultValue) {
            var str = base.GetAttribute(xpath, name, null);
            if (!bool.TryParse(str, out bool value)) return defaultValue;
            else return value;
        }

        public long GetAttribute(string xpath, string name, long defaultValue) {
            var str = base.GetAttribute(xpath, name, null);
            if (!long.TryParse(str, out long value)) return defaultValue;
            else return value;
        }

        public T GetAttribute<T>(string xpath, string name, T defaultValue) where T: struct, Enum {
            var str = base.GetAttribute(xpath, name, null);
            if (!Enum.TryParse(str, out T value)) return defaultValue;
            else return value;
        }

        public void SetAttribute(string xpath, string name, bool value)
            => base.SetAttribute(xpath, name, value.ToString().ToLower());

        public void SetAttribute(string xpath, string name, long value)
            => base.SetAttribute(xpath, name, value.ToString());

        public void SetAttribute<T>(string xpath, string name, T value) where T: struct, Enum
            => base.SetAttribute(xpath, name, value.ToString());

    }
}