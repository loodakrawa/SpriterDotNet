using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public static class SpriterObjectPool
    {
        private static readonly IDictionary<Type, Stack<object>> Pools = new Dictionary<Type, Stack<object>>();
        private static readonly IDictionary<Type, Stack<object>> ArrayPools = new Dictionary<Type, Stack<object>>();
        private static readonly IDictionary<Type, int> ArraySizes = new Dictionary<Type, int>();

        public static T[] GetArray<T>(int capacity)
        {
            if (!SpriterConfig.PoolingEnabled) return new T[capacity];

            Type type = typeof(T);

            int size;
            ArraySizes.TryGetValue(type, out size);

            if (size == 0)
            {
                size = capacity;
                ArraySizes[type] = capacity;
            }

            var pool = GetPool(type, ArrayPools);

            if (capacity > size) pool.Clear();

            if (pool.Count > 0) return pool.Pop() as T[];

            return new T[capacity];
        }

        public static T GetObject<T>() where T : class, new()
        {
            if (SpriterConfig.PoolingEnabled)
            {
                var pool = GetPool<T>(Pools);
                if (pool.Count > 0) return pool.Pop() as T;
            }
            return new T();
        }

        public static void ReturnObject<T>(T obj) where T : class
        {
            if (!SpriterConfig.PoolingEnabled || obj == null) return;
            Type type = typeof(T);

            var pool = GetPool(type, Pools);
            pool.Push(obj);
        }

        public static void ReturnObject<T>(T[] obj) where T : class
        {
            if (obj == null) return;
            Type type = typeof(T);

            for (int i = 0; i < obj.Length; ++i)
            {
                ReturnObject(obj[i]);
                obj[i] = null;
            }

            if (SpriterConfig.PoolingEnabled)
            {
                var pool = GetPool(type, ArrayPools);
                pool.Push(obj);
            }
        }

        public static void ReturnChildren<T>(ICollection<T> collection) where T : class
        {
            if(SpriterConfig.PoolingEnabled) foreach (T t in collection) ReturnObject<T>(t);
            collection.Clear();
        }

        public static void ReturnChildren<K, T>(IDictionary<K, T> dict) where T : class
        {
            if (SpriterConfig.PoolingEnabled) foreach (var entry in dict) ReturnObject<T>(entry.Value);
            dict.Clear();
        }

        private static Stack<object> GetPool<T>(IDictionary<Type, Stack<object>> pools)
        {
            return GetPool(typeof(T), pools);
        }

        private static Stack<object> GetPool(Type type, IDictionary<Type, Stack<object>> pools)
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
