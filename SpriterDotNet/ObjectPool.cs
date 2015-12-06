using System;
using System.Collections;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public static class ObjectPool
    {
        private static readonly IDictionary<string, Stack<object>> Pools = new Dictionary<string, Stack<object>>();
        private static readonly IDictionary<string, Stack<object>> ArrayPools = new Dictionary<string, Stack<object>>();
        private static readonly IDictionary<string, int> ArraySizes = new Dictionary<string, int>();

        public static T[] GetArray<T>(int capacity)
        {
            Type type = typeof(T);

            int size;
            ArraySizes.TryGetValue(type.FullName, out size);

            if (size == 0)
            {
                size = capacity;
                ArraySizes[type.FullName] = capacity;
            }

            var pool = GetPool(type, ArrayPools);

            if (capacity > size) pool.Clear();

            if (pool.Count > 0) return pool.Pop() as T[];

            return new T[capacity];
        }

        public static T GetObject<T>() where T : class, new()
        {
            var pool = GetPool<T>(Pools);
            if (pool.Count > 0) return pool.Pop() as T;
            return new T();
        }

        public static void ReturnObject<T>(T obj) where T : class
        {
            if (obj == null) return;
            Type type = typeof(T);

            if(obj is IList)
            {
                int a = 5;
            }

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

            var pool = GetPool(type, ArrayPools);
            pool.Push(obj);
        }

        public static void ReturnChildren<T>(ICollection<T> collection) where T : class
        {
            foreach (T t in collection) ReturnObject<T>(t);
            collection.Clear();
        }

        public static void ReturnChildren<K, T>(IDictionary<K, T> dict) where T : class
        {
            foreach (var entry in dict) ReturnObject<T>(entry.Value);
            dict.Clear();
        }

        private static Stack<object> GetPool<T>(IDictionary<string, Stack<object>> pools)
        {
            return GetPool(typeof(T), pools);
        }

        private static Stack<object> GetPool(Type type, IDictionary<string, Stack<object>> pools)
        {
            Stack<object> pool;

            pools.TryGetValue(type.FullName, out pool);
            if (pool == null)
            {
                pool = new Stack<object>();
                pools[type.FullName] = pool;
            }

            return pool;
        }
    }
}
