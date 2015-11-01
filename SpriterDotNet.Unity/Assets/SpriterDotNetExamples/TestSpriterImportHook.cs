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
        }

        private static void SpriterImporter_EntityImported(SpriterEntity entity, GameObject go)
        {
            Debug.Log("Imported Entity: " + entity.Name);
        }
    }
}

#endif