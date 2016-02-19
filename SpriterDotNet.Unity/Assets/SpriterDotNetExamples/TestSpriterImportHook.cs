// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using SpriterDotNet;
using SpriterDotNetUnity;
using UnityEditor;
using UnityEngine;

namespace SpriterDotNetExamples
{
    [InitializeOnLoad]
    class TestSpriterImportHook
    {
        static TestSpriterImportHook()
        {
            SpriterImporter.EntityImported += SpriterImporter_EntityImported;
            SpriterImporter.ContentLoader = new CustomContentLoader();
        }

        private static void SpriterImporter_EntityImported(SpriterEntity entity, GameObject go)
        {
            Debug.Log("Imported Entity: " + entity.Name);
        }
    }

    class CustomContentLoader : DefaultContentLoader
    {
        public override T Load<T>(string path)
        {
            Debug.Log("CustomContentLoader.Load " + path);
            return base.Load<T>(path);
        }
    }

}

#endif