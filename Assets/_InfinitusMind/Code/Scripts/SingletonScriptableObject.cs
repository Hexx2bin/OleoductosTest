using System;
using UnityEngine;

namespace Infinitus
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    T[] assets = Resources.LoadAll<T>("");
                    if (assets == null || assets.Length < 1)
                        throw new InvalidOperationException($"No instance of {typeName} found in the resources.");
                    else if (assets.Length > 1)
                        Debug.LogWarning($"Multiple instances of the singleton {typeName} found in the resources");
                    instance = assets[0];
                }
                return instance;
            }
        }
        private static string typeName => typeof(T).ToString();
    }
}