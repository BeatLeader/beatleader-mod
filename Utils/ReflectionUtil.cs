using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class ReflectionUtil
    {
        public static Dictionary<string, Delegate> GetEvents<T>(this T type)
        {
            Dictionary<string, Delegate> events = new Dictionary<string, Delegate>();
            EventInfo[] tlEventsInfo = type.GetType().GetEvents(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (EventInfo eventInfo in tlEventsInfo)
            {
                FieldInfo info = type.GetType().GetField(eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                Delegate extractedDelegate = (Delegate)info.GetValue(type);
                if (extractedDelegate != null)
                {
                    events.Add(info.Name, extractedDelegate);
                }
            }
            return events;
        }
        public static void SetEvents<T>(this T type, Dictionary<string, Delegate> events)
        {
            foreach (var item in events)
            {
                var field = type.GetType().GetField(item.Key, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(type, item.Value);
                }
            }
        }
        public static void CloneEvents<T>(T sender, T reciever)
        {
            var events = GetEvents(sender);
            SetEvents(reciever, events);
        }
        public static List<FieldInfo> GetFields<T>(this T type, BindingFlags flags, Func<FieldInfo, bool> searchFilter)
        {
            List<FieldInfo> sortedFields = new List<FieldInfo>();
            var fields = type.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                if (searchFilter == null)
                {
                    sortedFields.Add(field);
                    continue;
                }
                if (searchFilter(field))
                {
                    sortedFields.Add(field);
                }
            }
            return sortedFields;
        }
        public static List<FieldInfo> GetFields<T>(this T type)
        {
            return GetFields<T>(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null);
        }
    }
}
