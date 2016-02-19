// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

namespace SpriterDotNetUnity
{
    public interface IContentLoader
    {
        T Load<T>(string path) where T : UnityEngine.Object;
    }
}

#endif