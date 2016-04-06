// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Helpers;
using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public class ObjectPool
    {
        protected Config Config { get; set; }
        protected Dictionary<Type, Stack<object>> Pools { get; set; }
        protected Dictionary<Type, Dictionary<int, Stack<object>>> ArrayPools { get; set; }

        public ObjectPool(Config config)
        {
            Config = config;
            Pools = new Dictionary<Type, Stack<object>>();
            ArrayPools = new Dictionary<Type, Dictionary<int, Stack<object>>>();
        }

        public void Clear()
        {
            Pools.Clear();
            ArrayPools.Clear();
        }

        public virtual T[] GetArray<T>(int capacity)
        {
            if (!Config.PoolingEnabled) return new T[capacity];

            var poolsDict = ArrayPools.GetOrCreate(typeof(T));
            var stack = poolsDict.GetOrCreate(capacity);

            if (stack.Count > 0) return stack.Pop() as T[];

            return new T[capacity];
        }

        public virtual T GetObject<T>() where T : class, new()
        {
            if (Config.PoolingEnabled)
            {
                var pool = Pools.GetOrCreate(typeof(T));
                if (pool.Count > 0) return pool.Pop() as T;
            }
            return new T();
        }

        public virtual void ReturnObject<T>(T obj) where T : class
        {
            if (!Config.PoolingEnabled || obj == null) return;
            var pool = Pools.GetOrCreate(typeof(T));
            pool.Push(obj);
        }

        public virtual void ReturnObject<T>(T[] obj) where T : class
        {
            if (!Config.PoolingEnabled || obj == null) return;

            for (int i = 0; i < obj.Length; ++i)
            {
                ReturnObject(obj[i]);
                obj[i] = null;
            }

            var poolsDict = ArrayPools.GetOrCreate(typeof(T));
            var stack = poolsDict.GetOrCreate(obj.Length);
            stack.Push(obj);
        }

        public virtual void ReturnObject<K, T>(Dictionary<K, T> obj)
        {
            if (!Config.PoolingEnabled || obj == null) return;
            obj.Clear();

            var pool = Pools.GetOrCreate(obj.GetType());
            pool.Push(obj);
        }

        public virtual void ReturnChildren<T>(List<T> list) where T : class
        {
            if (Config.PoolingEnabled)
            {
                for (int i = 0; i < list.Count; ++i) ReturnObject<T>(list[i]);
            }
            list.Clear();
        }

        public virtual void ReturnChildren<K, T>(Dictionary<K, T> dict) where T : class
        {
            if (Config.PoolingEnabled)
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
