using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Utils.Expansions
{
    public static class ZenjectExpansion
    {
        //накидал на скорую руку, не бейте сильно
        public static T InjectAllFields<T>(this T type, DiContainer container)
        {
            if (type == null & container == null) return default;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = type.GetType().GetFields(flags);
            PropertyInfo[] properties = type.GetType().GetProperties(flags);

            foreach (FieldInfo field in fields)
            {
                foreach (var attr in field.GetCustomAttributes())
                {
                    if (attr is InjectAttribute)
                    {
                        field.SetValue(type, container.TryResolve(field.FieldType));
                    }
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite) continue;
                foreach (var attr in property.GetCustomAttributes())
                {
                    if (attr is InjectAttribute)
                    {
                        property.SetValue(type, container.TryResolve(property.PropertyType));
                    }
                }

            }

            return type;
        }
        public static U InjectAllFieldsOfType<T, U>(this U type, DiContainer container)
        {
            if (type == null & container == null) return default;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = type.GetType().GetFields(flags);
            PropertyInfo[] properties = type.GetType().GetProperties(flags);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != typeof(T) & field == null) continue;
                foreach (var attr in field.GetCustomAttributes())
                {
                    if (attr is InjectAttribute)
                    {
                        field.SetValue(type, container.TryResolve(field.FieldType));
                    }
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType != typeof(T) && !property.CanWrite & property == null) continue;
                foreach (var attr in property.GetCustomAttributes())
                {
                    if (attr is InjectAttribute)
                    {
                        property.SetValue(type, container.TryResolve(property.PropertyType));
                    }
                }
            }

            return type;
        }
        public static T InjectAllFieldsOfTypeFromInstance<T, U>(this T type, U instance)
        {
            if (type == null & instance == null) return default;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = type.GetType().GetFields(flags);
            PropertyInfo[] properties = type.GetType().GetProperties(flags);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(U))
                { 
                    Debug.LogWarning(field.FieldType);
                    foreach (var attr in field.GetCustomAttributes())
                    {
                        if (attr is InjectAttribute)
                        {
                            field.SetValue(type, instance);
                        }
                    }
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(U) && property.CanWrite)
                {
                    Debug.LogWarning(property.PropertyType);
                    foreach (var attr in property.GetCustomAttributes())
                    {
                        if (attr is InjectAttribute)
                        {
                            property.SetValue(type, instance);
                        }
                    }
                }
            }

            return type;
        }
    }
}
