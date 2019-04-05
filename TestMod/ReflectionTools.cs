using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace OutwardTestMod1
{
    static class ReflectionTools
    {
        private static Dictionary<string, MemberInfo>  reflectedInfo = new Dictionary<string, MemberInfo>();

        /// <summary>
        /// Get a private method from the class passed in.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(Type instance, string methodName)
        {
            if (reflectedInfo.ContainsKey(methodName))
                if (reflectedInfo[methodName] is MethodInfo)
                    return (MethodInfo)reflectedInfo[methodName];
                else
                    throw new Exception(String.Format("{0} was expected to be a MethodInfo, but wasn't", methodName));

            MethodInfo toAdd = instance.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            reflectedInfo.Add(methodName, toAdd);
            return toAdd;
        }

        /// <summary>
        /// Get a private field from the class passed in.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static FieldInfo GetField(Type instance, string fieldName)
        {
            if (reflectedInfo.ContainsKey(fieldName))
                if (reflectedInfo[fieldName] is FieldInfo)
                    return (FieldInfo)reflectedInfo[fieldName];
                else
                    throw new Exception(String.Format("{0} was expected to be a FieldInfo, but wasn't", fieldName));

            FieldInfo toAdd = instance.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            reflectedInfo.Add(fieldName, toAdd);
            return toAdd;
        }
    }
}
