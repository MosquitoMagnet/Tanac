using Mv.Core;
using Newtonsoft.Json;

namespace System
{
    public class FileLocatorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fileLocator = (FileLocator)value;
            writer.WriteValue(fileLocator.FullPath);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(FileLocator);

    }
}