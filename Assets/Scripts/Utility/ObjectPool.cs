using UnityEngine;
using System.Collections.Generic;

namespace FootballTraining.Utility
{
    /// <summary>
    /// Generic typed object pool for Unity MonoBehaviour components.
    /// Reduces instantiation overhead for frequently reused objects (gap indicators, etc.).
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new();
        private readonly HashSet<T> _active   = new();
        private readonly int _maxSize;

        public int ActiveCount  => _active.Count;
        public int PooledCount  => _available.Count;

        public ObjectPool(T prefab, Transform parent, int initialSize = 8, int maxSize = 32)
        {
            _prefab  = prefab;
            _parent  = parent;
            _maxSize = maxSize;
            for (int i = 0; i < initialSize; i++)
                _available.Enqueue(CreateInstance());
        }

        public T Get()
        {
            T obj = _available.Count > 0 ? _available.Dequeue() : CreateInstance();
            obj.gameObject.SetActive(true);
            _active.Add(obj);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null || !_active.Contains(obj)) return;
            obj.gameObject.SetActive(false);
            _active.Remove(obj);
            if (_available.Count < _maxSize)
                _available.Enqueue(obj);
            else
                Object.Destroy(obj.gameObject);
        }

        public void ReturnAll()
        {
            foreach (var obj in new List<T>(_active))
                Return(obj);
        }

        private T CreateInstance()
        {
            var go = Object.Instantiate(_prefab.gameObject, _parent);
            go.SetActive(false);
            return go.GetComponent<T>();
        }
    }
}
