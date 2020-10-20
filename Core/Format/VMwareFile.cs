using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VMGuide.FileFormat
{
    public class VMwareFile
    {
        private Encoding fileEncoding;
        public string Path { get; private set; }

        private Dictionary<string, string> settings = new Dictionary<string, string>();
        public VMwareFile(string path)
        {
            Path = path;
            fileEncoding = DetectEncoding();
            Load();
        }

        private void Load()
        {
            settings.Clear();

            using (var istream = File.OpenRead(Path)) {
                var reader = new StreamReader(istream, fileEncoding);

                while (!reader.EndOfStream)
                {
                    var s = reader.ReadLine();
                    var split = s.Split(new char[] {'='}, 2);
                    if (split.Length < 2) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim().Trim('"');
                    settings[key] = value;
                }
            }
        }

        public string GetValue(string key, string defaultValue)
        {
            settings.TryGetValue(key, out string value);
            if (String.IsNullOrEmpty(value)) return defaultValue;
            else return value;
        }

        public bool GetValue(string key, bool defaultValue)
        {
            string str = GetValue(key, null);
            if (!bool.TryParse(str, out bool value)) return defaultValue;
            else return value;
        }

        public int GetValue(string key, int defaultValue)
        {
            string str = GetValue(key, null);
            if (!int.TryParse(str, out int value)) return defaultValue;
            else return value;
        }

        // VMware VM's BIOS date is stored as a UNIX timestamp.
        public DateTime GetValue(string key, DateTime defaultValue)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0);

            string str = GetValue(key, null);
            if (!int.TryParse(str, out int timeStamp)) return defaultValue;
            else return unixStart.AddSeconds(timeStamp);
        }
        
        public T GetValue<T> (string key, T defaultValue) where T: struct, Enum
        {
            string str = GetValue(key, null);
            if (!Enum.TryParse<T>(str, true, out T value)) return defaultValue;
            else return value;
        }

        public IEnumerable<KeyValuePair<string, string>> GetMatchedValues(string keyRegex, string defaultValue = null)
        {
            var regex = new Regex(keyRegex);
            return settings
                .Where(p => regex.IsMatch(p.Key))
                .Select(p => {
                    if (String.IsNullOrEmpty(p.Value))
                        return new KeyValuePair<string, string>(p.Key, defaultValue);
                    else return p;
                });
        }

        public IEnumerable<KeyValuePair<string, bool>> GetMatchedValues(string keyRegex, bool defaultValue)
        {
            return GetMatchedValues(keyRegex, null)
                .Select(p => {
                    if (!bool.TryParse(p.Value, out bool value))
                        return new KeyValuePair<string, bool>(p.Key, defaultValue);
                    else
                        return new KeyValuePair<string, bool>(p.Key, value);
                });
        }

        public IEnumerable<KeyValuePair<string, T>> GetMatchedValues<T>(string keyRegex, T defaultValue) where T: struct, Enum
        {
            return GetMatchedValues(keyRegex, null)
                .Select(p => {
                    if (!Enum.TryParse(p.Value, true, out T value))
                        return new KeyValuePair<string, T>(p.Key, defaultValue);
                    else
                        return new KeyValuePair<string, T>(p.Key, value);
                });
        }

        public void RemoveValue(string key)
            => settings.Remove(key);
        public void SetValue(string key, string value)
            => settings[key] = value;
        public void SetValue(string key, bool value)
            => settings[key] = value.ToString().ToUpper();
        public void SetValue(string key, int value)
            => settings[key] = value.ToString();
        public void SetValue<T>(string key, T value) where T: struct, Enum
            => settings[key] = value.ToString();
        public void SetValue(string key, DateTime value)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0);
            int timeStamp = (int)(value - unixStart).TotalSeconds;
            settings[key] = timeStamp.ToString();
        }

        public void Save()
        {
            using (var ostream = File.Open(Path, FileMode.Truncate)) {
                var writer = new StreamWriter(ostream, fileEncoding);
                foreach (var kv in settings)
                    writer.WriteLine($"{kv.Key} = \"{kv.Value}\"");
                writer.Flush();
            }
        }

        private Encoding DetectEncoding()
        {
            fileEncoding = Encoding.UTF8;
            Load();
            var charset = settings[".encoding"];

            Encoding encoding;
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                encoding = Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                encoding = Encoding.UTF8;
            }

            return encoding;
        }
    }

}