using System;
using UniRx.Toolkit;
using UnityEngine;

namespace TestApp.ObjectPool
{
    public class GameObjectDirectPool<TKey, TPoolable> : ObjectPool<TPoolable> where TPoolable : Component
    {
        protected Transform _parent;
        protected TKey _key;
        protected Func<TKey, TPoolable> _prefabGetter;

        public GameObjectDirectPool() { }

        public GameObjectDirectPool(TKey key, Func<TKey, TPoolable> prefabGetter, Transform parent) : this() =>
            Init(key, prefabGetter, parent);

        public void Init(TKey key, Func<TKey, TPoolable> prefabGetter, Transform parent)
        {
            _key = key;
            _prefabGetter = prefabGetter;
            _parent = parent;
        }

        protected override TPoolable CreateInstance()
        {
            var instance = _prefabGetter != null ? _prefabGetter?.Invoke(_key) : default;
            if (_parent)
                instance.transform.SetParent(_parent);

            return instance;
        }
    }
}