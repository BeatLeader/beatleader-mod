using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    public static class ReflectionUtils
    {
        public static bool ContainsFieldMarkedWithAttribute(this Type type, Type attributeType)
        {
            foreach (var member in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                foreach (var attribute in member.CustomAttributes)
                    if (attribute.AttributeType == attributeType)
                        return true;
            return false;
        }
        public static bool ContainsFieldOfTypeMarkedWithAttribute(this Type type, Type fieldType, Type attributeType)
        {
            return type.ContainsFieldOfTypeMarkedWithAttribute(fieldType, attributeType, out FieldInfo info);
        }
        public static bool ContainsFieldOfTypeMarkedWithAttribute(this Type type, Type fieldType, Type attributeType, out FieldInfo field)
        {
            field = null;
            foreach (var member in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (member.FieldType != fieldType) continue;
                foreach (var attribute in member.CustomAttributes)
                    if (attribute.AttributeType == attributeType)
                    {
                        field = member; 
                        return true;
                    }     
            }
            return false;
        }
        public static List<Type> GetAllTypesMarkedWithAttributeInAssembly(this Assembly assembly, Type attributeType)
        {
            List<Type> types = new List<Type>();
            foreach (var item in assembly.GetTypes())
                foreach (var attribute in item.GetCustomAttributes(false))
                    if (attribute.GetType() == attributeType && !types.Contains(item))
                        types.Add(item);
            return types;
        }
    }
}
