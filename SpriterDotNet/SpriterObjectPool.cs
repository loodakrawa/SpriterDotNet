using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public static class SpriterObjectPool
    {
        private static readonly Dictionary<Type, Stack<object>> Pools = new Dictionary<Type, Stack<object>>();
        private static readonly Dictionary<Type, Stack<object>> ArrayPools = new Dictionary<Type, Stack<object>>();
        private static readonly Dictionary<Type, int> ArraySizes = new Dictionary<Type, int>();

        public static T[] GetArray<T>(int capacity)
        {
            if (!SpriterConfig.PoolingEnabled) return new T[capacity];

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

        public static void ReturnStructDict<K, T>(Dictionary<K, T> obj) where T : struct
        {
            if (!SpriterConfig.PoolingEnabled || obj == null) return;
            obj.Clear();

            Type type = obj.GetType();

            var pool = GetPool(type, Pools);
            pool.Push(obj);
        }

        public static void ReturnChildren<T>(List<T> list) where T : class
        {
            if (SpriterConfig.PoolingEnabled)
            {
                for (int i=0; i<list.Count; ++i) ReturnObject<T>(list[i]);
            }
            list.Clear();
        }

        public static void ReturnChildren<K, T>(Dictionary<K, T> dict) where T : class
        {
            if (SpriterConfig.PoolingEnabled)
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

        private static Stack<object> GetPool<T>(Dictionary<Type, Stack<object>> pools)
        {
            return GetPool(typeof(T), pools);
        }

        private static Stack<object> GetPool(Type type, Dictionary<Type, Stack<object>> pools)
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
