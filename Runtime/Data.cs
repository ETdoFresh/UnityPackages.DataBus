using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ETdoFresh.UnityPackages.DataBusSystem
{
    public class Data<T>
    {
        protected T _value;
        private UnityEvent<T, T> _onValueChanged = new();
        protected List<UnityAction<T, T>> _listeners = new();

        public T Value { get => _value; set => SetValue(value); }

        public void AddListener(UnityAction<T, T> onValueChanged)
        {
            _listeners.Add(onValueChanged);
            _onValueChanged.AddListener(onValueChanged);
            _onValueChanged.Invoke(default, _value);
        }

        public void RemoveListener(UnityAction<T, T> onValueChanged)
        {
            _listeners.Remove(onValueChanged);
            _onValueChanged.RemoveListener(onValueChanged);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
            _onValueChanged.RemoveAllListeners();
        }

        protected virtual void SetValue(T value)
        {
            var oldValue = _value;
            _value = value;
            _onValueChanged.Invoke(oldValue, _value);
        }
        
        public virtual async Task<T> GetValueAsync()
        {
            return _value;
        }
        
        public virtual async Task SetValueAsync(T value)
        {
            SetValue(value);
        }
    }
}