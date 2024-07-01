using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader {
    public class StringToArrayConverter : StringConverter {
        protected override bool CanConvertTo(Type targetType) {
            return targetType.IsArray || 
                (targetType.IsInterface && targetType.GetGenericTypeDefinition() is var gen
                && (gen == typeof(IEnumerable<>) || gen == typeof(IList<>)));
        }

        protected override object ConvertTo(string str, Type targetType) {
            targetType = targetType.GetElementType() ?? targetType.GetGenericArguments()[0];
            var arr = str.Split(',')
                .Select(static x => x.Trim())
                .Where(static x => !string.IsNullOrEmpty(x))
                .Select(x => Convert(x, targetType))
                .ToArray();
            var castedArr = Array.CreateInstance(targetType, arr.Length);
            Array.Copy(arr, castedArr, arr.Length);
            return castedArr;
        }
    }
}