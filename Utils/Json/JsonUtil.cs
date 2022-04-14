using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;

namespace BeatLeader.Utils.Json
{
    public static class JsonUtil
    {
        //public static T FromJson<T>(string json)
        //{
        //    T type = (T)Activator.CreateInstance(typeof(T));
        //    foreach (var item in type.GetFields())
        //    {
        //
        //    }
        //}
        public static string ToJson<T>(T type)
        {
            var fields = type.GetFields();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            if (fields != null)
            {
                int itemIdx = 0;
                foreach (var item in fields)
                {
                    itemIdx++;
                    if (fields.Count != itemIdx)
                        sb.AppendLine($"   {ParseType(item, item.GetValue(type), type, 0)},");
                    else
                        sb.AppendLine($"   {ParseType(item, item.GetValue(type), type, 0)}");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
        private static string ParseField(FieldInfo field, object value, int hierarchyIndex)
        {
            hierarchyIndex++;
            if (field.FieldType == typeof(string))
                return $"\"{field.Name}\" : \"{field.GetValue(value)}\"";
            else if (field.FieldType == typeof(char))
                return $"\"{field.Name}\" : '{field.GetValue(value)}'";
            else if (IsInstance(field))
                return $"\"{field.Name}\" : {ParseType(field, field.GetValue(value), value, hierarchyIndex)}";
            else
                return $"\"{field.Name}\" : {field.GetValue(value)}";
        }
        private static string ParseType<T>(FieldInfo field, T value, object instance, int hierarchyIndex)
        {
            if (field == null) return null;
            hierarchyIndex++;
            if (field.FieldType.IsArray)
            {
                return $"{field.Name} : {ParseAsArray((T[])field.GetValue(instance), hierarchyIndex)}";
            }
            else if (!IsInstance(field))
            {
                return $"{ParseField(field, value, hierarchyIndex)}";
            }
            else
            {
                return $"{field.Name} : {ParseAsInstance(value, hierarchyIndex)}";
            }
        }
        private static string ParseAsArray<T>(T[] array, int hierarchyIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[");

            if (array.Length == 0 & array == null)
            {
                sb.Append("]");
                return sb.ToString();
            }

            int arrayIndex = 0;
            foreach (var item in array)
            {
                arrayIndex++;
                if (array.Length != arrayIndex)
                    sb.AppendLine($"{GetSpacesByHierarchyIndex(hierarchyIndex)}{ParseAsInstance(item, hierarchyIndex)},");
                else
                    sb.AppendLine($"{GetSpacesByHierarchyIndex(hierarchyIndex)}{ParseAsInstance(item, hierarchyIndex)}");
            }

            sb.Append($"{GetSpacesByHierarchyIndex(hierarchyIndex)}]");
            return sb.ToString();
        }
        private static string ParseAsInstance<T>(T type, int hierarchyIndex)
        {
            hierarchyIndex++;
            var fields = type.GetFields();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            if (fields != null)
            {
                int itemIdx = 0;
                foreach (var field in fields)
                {
                    itemIdx++;
                    if (fields.Count != itemIdx)
                        sb.AppendLine($"{GetSpacesByHierarchyIndex(hierarchyIndex)}{ParseField(field, type, hierarchyIndex)},");
                    else
                        sb.AppendLine($"{GetSpacesByHierarchyIndex(hierarchyIndex)}{ParseField(field, type, hierarchyIndex)}");
                }
            }

            sb.Append("}");
            return sb.ToString();
        }
        private static string GetSpacesByHierarchyIndex(int index)
        {
            string value = "";
            for (int i = 0; i < index; i++)
                value += "   ";
            return value;
        }
        private static bool IsInstance(FieldInfo field)
        {
            var items = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (items != null && items.Length > 0)
                return true;
            else
                return false;
        }
    }
}
