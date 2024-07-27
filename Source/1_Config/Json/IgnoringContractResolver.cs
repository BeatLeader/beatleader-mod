using System;
using Newtonsoft.Json.Serialization;

namespace BeatLeader {
    internal class IgnoringContractResolver<T> : DefaultContractResolver {
        protected override JsonContract CreateContract(Type objectType) {
            var contract = base.CreateContract(objectType);
            if (contract.Converter is T) {
                contract.Converter = null;
            }
            return contract;
        }
    }
}