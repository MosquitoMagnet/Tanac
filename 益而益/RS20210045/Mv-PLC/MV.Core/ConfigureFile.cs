using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mv.Core.Extensions;
using Mv.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mv.Core
{
    public class ConfigureFile : IConfigureFile
    {
        private JObject _storage;
        private string _filePath = MvFiles.Configure;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public bool Contains(string key) => _storage.Values().Any(token => token.Path == key);

        public T GetValue<T>(string key)
        {

            var result = (_storage[key]?.ToString() ?? string.Empty).ToObject<T>();
            if (result == null && typeof(T).GetConstructors().Any(x => !x.GetParameters().Any()))//GetConstructors()获取当前 Type 的构造函数.GetParameters()获取指定的方法或构造函数的参数。
            {
                result = (T)Activator.CreateInstance(typeof(T)); //Activator.CreateInstance使用最符合指定参数的构造函数创建指定类型的实例
                _storage[key] = result.ToJson(Formatting.Indented);
                Save();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(key));
            }
            return result;
        }


        public IConfigureFile SetValue<T>(string key, T value)
        {
            if (EqualityComparer<T>.Default.Equals(GetValue<T>(key), value)) return this;

            _storage[key] = value.ToJson(Formatting.Indented);
            Save();
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(key));
            return this;
        }

        public IConfigureFile Load(string filePath = null)
        {
            if (!string.IsNullOrEmpty(filePath)) _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                _storage = new JObject(JObject.Parse("{}"));
                Save();
            }
            _storage = JObject.Parse(File.ReadAllText(_filePath));

            return this;
        }

        public IConfigureFile Clear()
        {
            _storage = new JObject();
            Save();
            return this;
        }

        public void Delete()
        {
            Clear();
            File.Delete(_filePath);
        }


        private void Save() => WriteToLocal(_filePath, _storage.ToString(Formatting.Indented));

        private void WriteToLocal(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch (IOException)
            {
                WriteToLocal(path, text);
            }
        }
    }
}