using System;
using Newtonsoft.Json;

namespace BeatLeader {
    internal class TypeHandlingConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            if (value != null) {
                serializer = new JsonSerializer {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    ContractResolver = new IgnoringContractResolver<TypeHandlingConverter>()
                };
            }
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            serializer = new JsonSerializer {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ContractResolver = new IgnoringContractResolver<TypeHandlingConverter>()
            };
            return serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType) {
            return true;
        }
    }
}