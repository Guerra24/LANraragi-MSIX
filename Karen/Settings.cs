using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Windows.Storage;

namespace Karen
{
    public class Settings : INotifyPropertyChanged
    {
        private static Settings instance = new Settings();

        public static Settings Default => instance;

        public event PropertyChangedEventHandler PropertyChanged;

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public T GetObjectLocal<T>([CallerMemberName] string key = "") => GetObjectLocal<T>(default, key);

        public T GetObjectLocal<T>(T def, [CallerMemberName] string key = "")
        {
            var val = localSettings.Values[key];
            return val != null ? (T)val : def;
        }

        public void StoreObjectLocal(object obj, [CallerMemberName] string key = "") => localSettings.Values[key] = obj;

        public void DeleteObjectLocal(string key) => localSettings.Values.Remove(key);

        public string ContentFolder
        {
            get => GetObjectLocal("");
            set
            {
                StoreObjectLocal(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ContentFolder"));
            }
        }
        public string ThumbnailFolder
        {
            get => GetObjectLocal("");
            set
            {
                StoreObjectLocal(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ThumbnailFolder"));
            }
        }
        public bool StartServerAutomatically
        {
            get => GetObjectLocal(false);
            set => StoreObjectLocal(value);
        }
        public bool StartWithWindows
        {
            get => GetObjectLocal(false);
            set => StoreObjectLocal(value);
        }
        public string NetworkPort
        {
            get => GetObjectLocal("3000");
            set => StoreObjectLocal(value);
        }
        public bool FirstLaunch
        {
            get => GetObjectLocal(true);
            set => StoreObjectLocal(value);
        }
        public bool ForceDebugMode
        {
            get => GetObjectLocal(false);
            set => StoreObjectLocal(value);
        }
        public bool UseWSL2
        {
            get => GetObjectLocal(false);
            set => StoreObjectLocal(value);
        }
        public string Version
        {
            get => GetObjectLocal("");
            set => StoreObjectLocal(value);
        }
        public bool Karen
        {
            get => GetObjectLocal(false);
            set => StoreObjectLocal(value);
        }

        public void Migrate()
        {
            var searchRoot = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Karen"));

            if (!searchRoot.Exists)
                return;

            var files = searchRoot.GetFiles("user.config", SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime);
            if (files.Count() == 0)
                return;

            var file = files.FirstOrDefault();
            if (file == null)
                return;

            var configXml = XDocument.Load(file.FullName);

            Default.FirstLaunch = false;

            foreach (var element in configXml.Element("configuration").Element("userSettings").Element("Karen.Properties.Settings").Elements("setting"))
            {
                var name = element.Attribute("name").Value;
                var value = element.Value;
                switch (name)
                {
                    case "ContentFolder":
                        Default.ContentFolder = value;
                        break;
                    case "StartServerAutomatically":
                        Default.StartServerAutomatically = bool.Parse(value);
                        break;
                    case "StartWithWindows":
                        Default.StartWithWindows = bool.Parse(value);
                        break;
                    case "NetworkPort":
                        Default.NetworkPort = value;
                        break;
                    case "ForceDebugMode":
                        Default.ForceDebugMode = bool.Parse(value);
                        break;
                    case "UseWSL2":
                        Default.UseWSL2 = bool.Parse(value);
                        break;
                    case "ThumbnailFolder":
                        Default.ThumbnailFolder = value;
                        break;
                }
            }
            foreach (var f in files)
                f.Delete();
        }

    }

}