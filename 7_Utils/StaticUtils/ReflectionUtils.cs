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
        public static T Cast<T>(this object value, T type)
        {
            return (T)value;
        }
        public static FieldInfo GetField(this Type targetType, Func<FieldInfo, bool> filter = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return targetType.GetFields(filter, flags).FirstOrDefault();
        }
        public static List<FieldInfo> GetFields(this Type targetType, Func<FieldInfo, bool> filter = null, 
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return targetType.GetFields(flags).Where(x => filter is null ? true : filter(x)).ToList();
        }
        public static List<PropertyInfo> GetProperties(this Type targetType, Func<PropertyInfo, bool> filter = null,
           BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return targetType.GetProperties(flags).Where(x => filter is null ? true : filter(x)).ToList();
        }
        public static List<Type> GetTypes(this Assembly assembly, Func<Type, bool> filter)
        {
            return assembly.GetTypes().Where(filter).ToList();
        }
        public static List<T> ScanAndActivateTypes<T>(this Assembly assembly, Func<T, bool> filter = null)
        {
            return GetTypes(assembly, x => x == typeof(T) || x.BaseType == typeof(T)).Where(x => !x.IsAbstract).Select(x => (T)Activator.CreateInstance(x))
                .Where(filter != null ? filter : x => true).ToList();
        }
    }
}
