using SpriterDotNet.Helpers;
using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public class SpriterObjectPool
    {
        private readonly SpriterConfig config;

        private readonly Dictionary<Type, Stack<object>> Pools = new Dictionary<Type, Stack<object>>();
        private readonly Dictionary<Type, Dictionary<int, Stack<object>>> ArrayPools = new Dictionary<Type, Dictionary<int, Stack<object>>>();

        public SpriterObjectPool(SpriterConfig config)
        {
            this.config = config;
        }

        public void Clear()
        {
            Pools.Clear();
            ArrayPools.Clear();
        }

        public T[] GetArray<T>(int capacity)
        {
            if (!config.PoolingEnabled) return new T[capacity];

            var poolsDict = ArrayPools.GetOrCreate(typeof(T));
            var stack = poolsDict.GetOrCreate(capacity);

            if (stack.Count > 0) return stack.Pop() as T[];

            return new T[capacity];
        }

        public T GetObject<T>() where T : class, new()
        {
            if (config.PoolingEnabled)
            {
                var pool = Pools.GetOrCreate(typeof(T));
                if (pool.Count > 0) return pool.Pop() as T;
            }
            return new T();
        }

        public void ReturnObject<T>(T obj) where T : class
        {
            if (!config.PoolingEnabled || obj == null) return;
            var pool = Pools.GetOrCreate(typeof(T));
            pool.Push(obj);
        }

        public void ReturnObject<T>(T[] obj) where T : class
        {
            if (!config.PoolingEnabled || obj == null) return;

            for (int i = 0; i < obj.Length; ++i)
            {
                ReturnObject(obj[i]);
                obj[i] = null;
            }

            var poolsDict = ArrayPools.GetOrCreate(typeof(T));
            var stack = poolsDict.GetOrCreate(obj.Length);
            stack.Push(obj);
        }

        public void ReturnObject<K, T>(Dictionary<K, T> obj)
        {
            if (!config.PoolingEnabled || obj == null) return;
            obj.Clear();

            var pool = Pools.GetOrCreate(obj.GetType());
            pool.Push(obj);
        }

        public void ReturnChildren<T>(List<T> list) where T : class
        {
            if (config.PoolingEnabled)
            {
                for (int i = 0; i < list.Count; ++i) ReturnObject<T>(list[i]);
            }
            list.Clear();
        }

        public void ReturnChildren<K, T>(Dictionary<K, T> dict) where T : class
        {
            if (config.PoolingEnabled)
            {
                var enumerator = dict.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var e = enumerator.Current;
                    ReturnObject<T>(e.Value);
                }
            }
            dict.Clear();
        }
    }
}
