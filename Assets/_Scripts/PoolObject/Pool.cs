using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Scripts.PoolObject
{
    public class Pool<T> where T: MonoBehaviour
    {
        public bool AutoExpand { get; set; }
        private T _prefab { get; set; }
        private Transform _container { get; }

        public List<T> PoolList
        {
            get => _pool;
            set => _pool = value;
        }

        private List<T> _pool;

        public Pool(T prefab, int capacity, Transform container)
        {
            _prefab = prefab;
            _container = container;

            CreatePool(capacity);
        }

        private void CreatePool(int capacity)
        {
            _pool = new List<T>();
            for (int i = 0; i < capacity; i++)
            {
                CreateObject();
            }
        }

        private T CreateObject(bool isActiveByDefault = false)
        {
            var createdObject = Object.Instantiate(_prefab, _container);
            createdObject.gameObject.SetActive((isActiveByDefault));
            _pool.Add(createdObject);
            return createdObject;
        }

        public bool HasFreeElement(out T element)
        {
            foreach (var obj in _pool)
            {
                if (!obj.gameObject.activeInHierarchy)
                {
                    element = obj;
                    obj.gameObject.SetActive(true);
                    return true;
                }
            }
            element = null;
            return false;
        }

        public T GetFreeElement()
        {
            if (this.HasFreeElement(out var element))
            {
                return element;
            }

            if (AutoExpand)
            {
                return this.CreateObject(true);
            }

            throw new Exception($"There is no free element in pool of type {typeof(T)}");
        }
    }
}