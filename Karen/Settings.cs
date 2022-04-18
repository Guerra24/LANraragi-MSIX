using System.ComponentModel;
using System.Runtime.CompilerServices;
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
                PropertyChanged(this, new PropertyChangedEventArgs("ContentFolder"));
            }
        }
        public string ThumbnailFolder
        {
            get => GetObjectLocal("");
            set
            {
                StoreObjectLocal(value);
                PropertyChanged(this, new PropertyChangedEventArgs("ThumbnailFolder"));
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

    }
}