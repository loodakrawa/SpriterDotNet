// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class DefaultContentLoader : IContentLoader
    {
        public virtual T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset;
            asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.Log("Missing Asset: " + path);

            return asset;
        }
    }
}

#endif