using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Wiki.Core.Extensions;

namespace Wiki.Service.Configuration
{
    public class ServiceConfiguration : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly string _baseDir;
        private readonly string _id;
        private readonly ReaderWriterLockSlim _syncRoot = new ReaderWriterLockSlim();
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public ServiceConfiguration(string id, string baseDir = null)
        {
            this._id = id;
            this._baseDir = baseDir;
            if (string.IsNullOrEmpty(this._baseDir))
                this._baseDir = new FileInfo(typeof (ServiceConfiguration).Assembly.Location).DirectoryName ?? "";
            this._baseDir = Path.Combine(this._baseDir, "Configs");
        }

        public string this[string key]
        {
            get
            {
                using (this._syncRoot.UseReadLock())
                {
                    if (this._values.ContainsKey(key))
                        return this._values[key];
                    return null;
                }
            }
            set
            {
                using (this._syncRoot.UseWriteLock())
                {
                    this._values[key] = value;
                }
            }
        }

        public bool Remove(string key)
        {
            using (this._syncRoot.UseWriteLock())
            {
                return this._values.Remove(key);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                using (this._syncRoot.UseReadLock())
                {
                    return this._values.Keys;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            using (this._syncRoot.UseReadLock())
            {
                return this._values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Save()
        {
            if (!Directory.Exists(this._baseDir))
                Directory.CreateDirectory(this._baseDir);
            var fn = GetConfigPath();
            string str;
            using (this._syncRoot.UseReadLock())
            {
                str = JsonConvert.SerializeObject(this._values, Formatting.Indented);
            }
            File.WriteAllText(fn, str);
        }

        public string GetConfigPath()
        {
            return Path.Combine(this._baseDir, this._id + ".json");
        }

        public void Load()
        {
            var fn = GetConfigPath();
            if (!File.Exists(fn))
                return;
            var str = File.ReadAllText(fn);
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
            using (this._syncRoot.UseWriteLock())
            {
                this._values.Clear();
                foreach (var item in dic)
                {
                    this._values[item.Key] = item.Value;
                }
            }
        }
    }
}