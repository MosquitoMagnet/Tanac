using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Text;

namespace MV.Core.I18n
{
    public class TranslationSource : INotifyPropertyChanged
    {
        public static TranslationSource Instance { get; } = new TranslationSource();

        private readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();

        public string this[string key]
        {
            get
            {
                Tuple<string, string> tuple = SplitName(key);
                string translation = null;
                if (resourceManagerDictionary.ContainsKey(tuple.Item1))
                    translation = resourceManagerDictionary[tuple.Item1].GetString(tuple.Item2, currentCulture);
                return translation ?? key;
            }
        }

        private CultureInfo currentCulture = CultureInfo.InstalledUICulture;
        public CultureInfo CurrentCulture
        {
            get { return currentCulture; }
            set
            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                    // string.Empty/null indicates that all properties have changed
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        public event PropertyChangedEventHandler PropertyChanged;

        public void AddResourceManager(ResourceManager resourceManager)
        {
            if (!resourceManagerDictionary.ContainsKey(resourceManager.BaseName))
            {
                resourceManagerDictionary.Add(resourceManager.BaseName, resourceManager);
            }
        }

        public static Tuple<string, string> SplitName(string local)
        {
            int idx = local.ToString().LastIndexOf(".");
            var tuple = new Tuple<string, string>(local.Substring(0, idx), local.Substring(idx + 1));
            return tuple;
        }
    }
}
