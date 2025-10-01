using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeatLeader {
    internal class ImplicitTypeConverter<T> : JsonConverter {
        private readonly Type _targetType = typeof(T);
        
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            var token = JToken.Load(reader);
            return token.ToObject<T>();
        }

        public override bool CanConvert(Type objectType) {
            return objectType.IsAssignableFrom(_targetType);
        }
    }
}