﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;

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
           // Debug.Log(String.Format("GetMethod({0}, {1}): exists: {2}", instance, methodName, reflectedInfo.ContainsKey(methodName)));
            if (reflectedInfo.ContainsKey(methodName))
                if (reflectedInfo[methodName] is MethodInfo)
                    return (MethodInfo)reflectedInfo[methodName];
                else
                    throw new Exception(String.Format("{0} ({1}) was expected to be a MethodInfo, but wasn't", methodName, reflectedInfo[methodName].ToString()));

            try
            {
                // methodName can come from an untrusted source, so handle if it doesn't actually exist.
                MethodInfo toAdd = instance.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                reflectedInfo.Add(methodName, toAdd);
                return toAdd;
            }
            catch(NullReferenceException e)
            {
                Debug.Log(String.Format("Method {0} was not found in type {1}", methodName, instance));
                return null;
            }
        }

        /// <summary>
        /// Get a private field from the class passed in.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static FieldInfo GetField(Type instance, string fieldName)
        {
            //Debug.Log(String.Format("GetField({0}, {1}): exists: {2}", instance, fieldName, reflectedInfo.ContainsKey(fieldName)));
            if (reflectedInfo.ContainsKey(fieldName))
                if (reflectedInfo[fieldName] is FieldInfo)
                    return (FieldInfo)reflectedInfo[fieldName];
                else
                    throw new Exception(String.Format("{0} ({1}) was expected to be a FieldInfo, but wasn't", fieldName, reflectedInfo[fieldName].ToString()));

            try
            {
                FieldInfo toAdd = instance.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                reflectedInfo.Add(fieldName, toAdd);
                return toAdd;
            }
            catch (NullReferenceException e)
            {
                Debug.Log(String.Format("Field {0} was not found in type {1}", fieldName, instance));
                return null;
            }
        }
    }
}
