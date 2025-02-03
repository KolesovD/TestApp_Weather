using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TestApp.ObjectPool
{
    /// <summary>Мультиобъектный пул</summary>
    /// <typeparam name="TKey">Ключ словаря, определяющий тип префаба</typeparam>
    public class PoolObjectDirectList<TKey, TPoolable> where TPoolable : Component
    {
        private readonly Dictionary<TKey, GameObjectDirectPool<TKey, TPoolable>> _pools;
        private readonly Func<TKey, TPoolable> _prefabGetter;
        private readonly Transform _parent;

        public PoolObjectDirectList(Func<TKey, TPoolable> prefabGetter)
        {
            _pools = new Dictionary<TKey, GameObjectDirectPool<TKey, TPoolable>>();

            _parent = null;
            _prefabGetter = prefabGetter;
        }

        public PoolObjectDirectList(Transform parent, Func<TKey, TPoolable> prefabGetter)
        {
            _pools = new Dictionary<TKey, GameObjectDirectPool<TKey, TPoolable>>();

            _parent = parent;
            _prefabGetter = prefabGetter;
        }

        /// <summary>Создать объект из пула если возможно, или создать новый</summary>
        public TPoolable Rent(TKey key)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = _pools[key] = new GameObjectDirectPool<TKey, TPoolable>();
                pool.Init(key, _prefabGetter, _parent);
            }

            var result = pool?.Rent();
            if (result && _parent)
                result.transform.SetParent(_parent);
            return result;
        }

        /// <summary>Создать объект из пула если возможно, или создать новый, и определить родителя</summary>
        public TPoolable Rent(TKey key, Transform needParent)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = _pools[key] = new GameObjectDirectPool<TKey, TPoolable>();
                pool.Init(key, _prefabGetter, _parent);
            }

            var result = pool?.Rent();
            if (result && needParent)
                result.transform.SetParent(needParent);

            return result;
        }

        public async UniTask PreloadAsync(TKey key, int preloadCount, int maxPerFrame, CancellationToken cancellationToken)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = _pools[key] = new GameObjectDirectPool<TKey, TPoolable>();
                pool.Init(key, _prefabGetter, _parent);
            }

            await pool.PreloadAsync(preloadCount, maxPerFrame).ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask PreloadAsync(TKey[] keys, int preloadCount, int maxPerFrame, CancellationToken cancellationToken)
        {
            foreach (var key in keys)
            {
                if (!_pools.TryGetValue(key, out var pool))
                {
                    pool = _pools[key] = new GameObjectDirectPool<TKey, TPoolable>();
                    pool.Init(key, _prefabGetter, _parent);
                }

                await pool.PreloadAsync(preloadCount, maxPerFrame).ToUniTask(cancellationToken: cancellationToken);
            }
        }

        /// <summary>Вернуть объект в пул</summary>
        public void Return(TKey key, TPoolable item)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Return(item);
        }
    }
}