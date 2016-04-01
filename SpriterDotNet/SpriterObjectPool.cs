using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public class SpriterObjectPool
    {
        private readonly SpriterConfig config;

        private readonly Dictionary<Type, Stack<object>> Pools = new Dictionary<Type, Stack<object>>();
        private readonly Dictionary<Type, Stack<object>> ArrayPools = new Dictionary<Type, Stack<object>>();
        private readonly Dictionary<Type, int> ArraySizes = new Dictionary<Type, int>();

        public SpriterObjectPool(SpriterConfig config)
        {
            this.config = config;
        }

        public void Clear()
        {
            Pools.Clear();
            ArrayPools.Clear();
            ArraySizes.Clear();
        }

        public T[] GetArray<T>(int capacity)
        {
            if (!config.PoolingEnabled) return new T[capacity];

            Type type = typeof(T);

            int size;
            ArraySizes.TryGetValue(type, out size);

            if (size == 0)
            {
                ArraySizes[type] = capacity;
            }

            var pool = GetPool(type, ArrayPools);

            if (capacity > size) pool.Clear();

            if (pool.Count > 0) return pool.Pop() as T[];

            return new T[capacity];
        }

        public T GetObject<T>() where T : class, new()
        {
            if (config.PoolingEnabled)
            {
                var pool = GetPool<T>(Pools);
                if (pool.Count > 0) return pool.Pop() as T;
            }
            return new T();
        }

        public void ReturnObject<T>(T obj) where T : class
        {
            if (!config.PoolingEnabled || obj == null) return;
            Type type = typeof(T);

            var pool = GetPool(type, Pools);
            pool.Push(obj);
        }

        public void ReturnObject<T>(T[] obj) where T : class
        {
            if (obj == null) return;
            Type type = typeof(T);

            for (int i = 0; i < obj.Length; ++i)
            {
                ReturnObject(obj[i]);
                obj[i] = null;
            }

            if (config.PoolingEnabled)
            {
                var pool = GetPool(type, ArrayPools);
                pool.Push(obj);
            }
        }

        public void ReturnObject<K, T>(Dictionary<K, T> obj)
        {
            if (!config.PoolingEnabled || obj == null) return;
            obj.Clear();

            Type type = obj.GetType();

            var pool = GetPool(type, Pools);
            pool.Push(obj);
        }

        public void ReturnChildren<T>(List<T> list) where T : class
        {
            if (config.PoolingEnabled)
            {
                for (int i=0; i<list.Count; ++i) ReturnObject<T>(list[i]);
            }
            list.Clear();
        }

        public void ReturnChildren<K, T>(Dictionary<K, T> dict) where T : class
        {
            if (config.PoolingEnabled)
            {
                var enumerator = dict.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    var e = enumerator.Current;
                    ReturnObject<T>(e.Value);
                }
            }
            dict.Clear();
        }

        private Stack<object> GetPool<T>(Dictionary<Type, Stack<object>> pools)
        {
            return GetPool(typeof(T), pools);
        }

        private Stack<object> GetPool(Type type, Dictionary<Type, Stack<object>> pools)
        {
            Stack<object> pool;

            pools.TryGetValue(type, out pool);
            if (pool == null)
            {
                pool = new Stack<object>();
                pools[type] = pool;
            }

            return pool;
        }
    }
}
