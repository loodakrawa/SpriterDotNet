// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

#if UNITY_EDITOR

using SpriterDotNet;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class SpriterImporter : AssetPostprocessor
    {
        private static readonly string[] ScmlExtensions = new string[] { ".scml" };
        private static readonly float DeltaZ = -0.001f;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            foreach (string asset in importedAssets)
            {
                if (!IsScml(asset)) continue;
                CreateSpriter(asset);
            }

            foreach (string asset in deletedAssets)
            {
                if (!IsScml(asset)) continue;
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                string asset = movedAssets[i];
                if (!IsScml(asset)) continue;
            }
        }

        private static bool IsScml(string path)
        {
            return ScmlExtensions.Any(path.EndsWith);
        }

        private static void CreateSpriter(string path)
        {
            string data = File.ReadAllText(path);
            Spriter spriter = SpriterParser.Parse(data);
            string rootFolder = Path.GetDirectoryName(path);

            foreach (SpriterEntity entity in spriter.Entities)
            {
                GameObject go = new GameObject();
                go.name = entity.Name;
                SpriterDotNetBehaviour sdnBehaviour = go.AddComponent<SpriterDotNetBehaviour>();
                sdnBehaviour.Entity = entity;
                sdnBehaviour.enabled = true;

                LoadSprites(sdnBehaviour, spriter, rootFolder);
                CreateChildren(entity, sdnBehaviour, spriter, go);

                string prefabPath = rootFolder + "/" + entity.Name + ".prefab";
                PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.ConnectToPrefab);

                GameObject.DestroyImmediate(go);
            }
        }

        private static void CreateChildren(SpriterEntity entity, SpriterDotNetBehaviour sdnBehaviour, Spriter spriter, GameObject parent)
        {
            int maxObjects = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                foreach (SpriterMainLineKey mainKey in animation.MainlineKeys)
                {
                    maxObjects = Math.Max(maxObjects, mainKey.ObjectRefs.Length);
                }
            }

            sdnBehaviour.Children = new GameObject[maxObjects];
            sdnBehaviour.Pivots = new GameObject[maxObjects];

            for (int i = 0; i < maxObjects; ++i)
            {
                GameObject pivot = new GameObject();
                GameObject child = new GameObject();

                sdnBehaviour.Pivots[i] = pivot;
                sdnBehaviour.Children[i] = child;

                pivot.transform.SetParent(parent.transform);
                child.transform.SetParent(pivot.transform);
                child.transform.localPosition = new Vector3(0, 0, DeltaZ * i);

                pivot.name = "pivot " + i;
                child.name = "child " + i;

                child.AddComponent<SpriteRenderer>();
            }
        }

        private static void LoadSprites(SpriterDotNetBehaviour sdnBehaviour, Spriter spriter, string rootFolder)
        {
            sdnBehaviour.Folders = new SdnFolder[spriter.Folders.Length];

            for (int i = 0; i < spriter.Folders.Length; ++i)
            {
                SpriterFolder folder = spriter.Folders[i];
                SdnFolder sdnFolder = new SdnFolder();
                sdnFolder.Files = new Sprite[folder.Files.Length];
                sdnBehaviour.Folders[i] = sdnFolder;

                for (int j = 0; j < folder.Files.Length; ++j)
                {
                    SpriterFile file = folder.Files[j];
                    string spritePath = rootFolder;
                    spritePath += "/";
                    spritePath += file.Name;

                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite == null)
                    {
                        Debug.LogWarning("Unable to load sprite: " + spritePath);
                        continue;
                    }

                    sdnFolder.Files[j] = sprite;
                }
            }
        }
    }
}
#endif
