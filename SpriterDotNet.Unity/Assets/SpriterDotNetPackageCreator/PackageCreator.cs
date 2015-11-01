// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace SpriterDotNetExamples
{
    public class PackageCreator
    {
        [MenuItem("SpriterDotNet/Create Packages")]
        private static void CreatePackages()
        {
            var allAssets = AssetDatabase.GetAllAssetPaths();
            var sdnCore = allAssets.Where(a => a.StartsWith("Assets/SpriterDotNet/")).ToArray();
            var sdnExamples = allAssets.Where(a => a.StartsWith("Assets/SpriterDotNetExamples/")).Union(sdnCore).ToArray();

            string path = Application.dataPath;
            path = Path.GetFullPath(Path.Combine(path, "../"));

            string corePackageName = path + "SpriterDotNet.Unity.unitypackage";
            string examplePackageName = path + "SpriterDotNet.Unity.Examples.unitypackage";

            AssetDatabase.ExportPackage(sdnCore, corePackageName);
            AssetDatabase.ExportPackage(sdnExamples, examplePackageName);
        }
    }
}

#endif
