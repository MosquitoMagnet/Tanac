using Mv.Core.Interfaces;

namespace Mv.Core
{
    public class ConfigManager<T> where T:new()
    {
        T config=new T();
        public T Get()
        {
            return config;
        }
        IConfigureFile configureFile ;
        public ConfigManager(IConfigureFile configureFile)
        {
            this.configureFile = configureFile;
            var c = this.configureFile.GetValue<T>(nameof(T));
            if (c != null)
                Set(c);
        }
        public void Set(T config)
        {
            this.config = config;
            configureFile.SetValue(nameof(T), config);
        }
    }
}