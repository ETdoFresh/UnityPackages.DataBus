using System;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace ETdoFresh.UnityPackages.DataBusSystem
{
    [Serializable]
    internal class DataBusEntry
    {
        public string name;

#if UNITY_EDITOR
        public UnityEditor.MonoScript script;
#endif

        public Type type;
        public Object unityObject;
        public object[] args;
    }
    
    internal class DataBusEntry<T> : DataBusEntry
    {
        public Data<T> data;
        public UnityAction<T, T> action;
    }
}