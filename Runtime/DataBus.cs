using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ETdoFresh.UnityPackages.DataBusSystem
{
    public class DataBus : MonoBehaviourLazyLoadedSingleton<DataBus>
    {
        [SerializeField] private List<DataBusEntry> dataBusEntries = new();

        private static readonly object[] EmptyObject = Array.Empty<object>();
        private static readonly Dictionary<Type, Dictionary<object, List<DataBusEntry>>> _dataEntryDictionary = new();
        private static readonly List<object[]> _argReferenceList = new();

        public static void AddListener<T, TData>(UnityAction<T, T> action, object[] args = null) where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            AddListener(typeof(TData), action, args);
        }

        public static void RemoveListener<T, TData>(UnityAction<T, T> action, object[] args = null)
            where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            RemoveListener(typeof(TData), action, args);
        }

        public static T GetValue<T, TData>(object[] args = null) where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            return GetValue<T>(typeof(TData), args);
        }

        public static async Task<T> GetValueAsync<T, TData>(object[] args = null) where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            return await GetValueAsync<T>(typeof(TData), args);
        }

        public static void SetValue<T, TData>(T value, object[] args = null) where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            SetValue(typeof(TData), value, args);
        }

        public static async Task SetValueAsync<T, TData>(T value, object[] args = null) where TData : Data<T>
        {
            args = ResolveArgsReference(args);
            await SetValueAsync(typeof(TData), value, args);
        }

        private static void AddListener<T>(Type dataType, UnityAction<T, T> action, object[] args)
        {
            DataBusEntry<T> dataBusEntry;
            Data<T> dataT;
            if (!_dataEntryDictionary.ContainsKey(dataType) || !_dataEntryDictionary[dataType].ContainsKey(args) ||
                _dataEntryDictionary[dataType][args] == null || _dataEntryDictionary[dataType][args].Count == 0)
            {
                dataT = Activator.CreateInstance(dataType, args) as Data<T>;

                var actionUnityObject = GetGameObject(action);
                var methodName = $"{action.Method.DeclaringType?.Name}.{action.Method.Name}";
                var messageName = actionUnityObject
                    ? $"{dataType.Name} >> {actionUnityObject.name} {methodName}"
                    : $"{dataType.Name} >> {methodName}";
                dataBusEntry = new DataBusEntry<T>
                {
                    name = messageName,
                    type = dataType,
                    unityObject = actionUnityObject,
                    data = dataT,
                    action = action,
                    args = args,
                };

                if (!_dataEntryDictionary.ContainsKey(dataType))
                    _dataEntryDictionary.Add(dataType, new Dictionary<object, List<DataBusEntry>>());

                if (!_dataEntryDictionary[dataType].ContainsKey(args))
                    _dataEntryDictionary[dataType].Add(args, new List<DataBusEntry>());

                _dataEntryDictionary[dataType][args].Add(dataBusEntry);
            }
            else
            {
                dataBusEntry = _dataEntryDictionary[dataType][args][0] as DataBusEntry<T>;
                dataT = dataBusEntry.data;
            }

            dataT.AddListener(action);

#if UNITY_EDITOR
            Instance.dataBusEntries.Add(dataBusEntry);
            Instance.dataBusEntries.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
            dataBusEntry.script = GetMonoScript(action);
#endif
        }

        private static void RemoveListener<T>(Type dataType, UnityAction<T, T> action, object[] args)
        {
            if (!_dataEntryDictionary.ContainsKey(dataType)) return;
            if (!_dataEntryDictionary[dataType].ContainsKey(args)) return;
            if (_dataEntryDictionary[dataType][args] == null) return;
            if (_dataEntryDictionary[dataType][args].Count == 0) return;

            var dataBusEntry = _dataEntryDictionary[dataType][args]
                .OfType<DataBusEntry<T>>()
                .FirstOrDefault(x => x.action == action);
            if (dataBusEntry == null) return;

            dataBusEntry.data.RemoveListener(action);
            _dataEntryDictionary[dataType][args].Remove(dataBusEntry);
            if (_dataEntryDictionary[dataType][args].Count == 0)
                _dataEntryDictionary[dataType].Remove(args);

#if UNITY_EDITOR
            if (Instance) Instance.dataBusEntries.Remove(dataBusEntry);
#endif
        }
        
        private static T GetValue<T>(Type dataType, object[] args)
        {
            if (!_dataEntryDictionary.ContainsKey(dataType)) return default;
            if (!_dataEntryDictionary[dataType].ContainsKey(args)) return default;
            if (_dataEntryDictionary[dataType][args] == null) return default;
            if (_dataEntryDictionary[dataType][args].Count == 0) return default;

            var dataBusEntry = _dataEntryDictionary[dataType][args][0] as DataBusEntry<T>;
            return dataBusEntry.data.Value;
        }
        
        private static async Task<T> GetValueAsync<T>(Type dataType, object[] args)
        {
            if (!_dataEntryDictionary.ContainsKey(dataType)) return default;
            if (!_dataEntryDictionary[dataType].ContainsKey(args)) return default;
            if (_dataEntryDictionary[dataType][args] == null) return default;
            if (_dataEntryDictionary[dataType][args].Count == 0) return default;

            var dataBusEntry = _dataEntryDictionary[dataType][args][0] as DataBusEntry<T>;
            return await dataBusEntry.data.GetValueAsync();
        }
        
        private static void SetValue<T>(Type dataType, T value, object[] args)
        {
            if (!_dataEntryDictionary.ContainsKey(dataType)) return;
            if (!_dataEntryDictionary[dataType].ContainsKey(args)) return;
            if (_dataEntryDictionary[dataType][args] == null) return;
            if (_dataEntryDictionary[dataType][args].Count == 0) return;

            var dataBusEntry = _dataEntryDictionary[dataType][args][0] as DataBusEntry<T>;
            dataBusEntry.data.Value = value;
        }
        
        private static async Task SetValueAsync<T>(Type dataType, T value, object[] args)
        {
            if (!_dataEntryDictionary.ContainsKey(dataType)) return;
            if (!_dataEntryDictionary[dataType].ContainsKey(args)) return;
            if (_dataEntryDictionary[dataType][args] == null) return;
            if (_dataEntryDictionary[dataType][args].Count == 0) return;

            var dataBusEntry = _dataEntryDictionary[dataType][args][0] as DataBusEntry<T>;
            await dataBusEntry.data.SetValueAsync(value);
        }

        // ------------------------------------ HELPER FUNCTIONS ------------------------------------ //
        private static object[] ResolveArgsReference(object[] args)
        {
            if (args == null) return EmptyObject;
            if (args.Length == 0) return EmptyObject;
            
            foreach (var argReference in _argReferenceList)
            {
                if (args.Length != argReference.Length) continue;

                var isEqual = true;
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals(argReference[i])) continue;
                    isEqual = false;
                    break;
                }

                if (isEqual) return argReference;
            }
            
            _argReferenceList.Add(args);
            return args;
        }
        
        private static GameObject GetGameObject<T>(UnityAction<T, T> action)
        {
            return action.Target switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };
        }

#if UNITY_EDITOR
        private static UnityEditor.MonoScript GetMonoScript<T>(UnityAction<T, T> action)
        {
            return action.Target switch
            {
                GameObject gameObject => UnityEditor.MonoScript.FromMonoBehaviour(
                    gameObject.GetComponent(action.Method.DeclaringType) as MonoBehaviour),
                Component component => UnityEditor.MonoScript.FromMonoBehaviour(component as MonoBehaviour),
                _ => null
            };
        }
#endif
    }
}