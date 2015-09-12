#if UNITY_EDITOR

using SpriterDotNet;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpriterDotNetUnity
{

    public class SpriterImporter : AssetPostprocessor
    {
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
            return path.EndsWith(".scml");
        }

        private static void CreateSpriter(string path)
        {
            string data = File.ReadAllText(path);
            Spriter spriter = Spriter.Parse(data);
            string rootFolder = Path.GetDirectoryName(path);
            SpriterEntity entity = spriter.Entities[0];

            GameObject go = new GameObject();
            go.name = entity.Name;
            SpriterDotNetBehaviour sdnBehaviour = go.AddComponent<SpriterDotNetBehaviour>();
            sdnBehaviour.spriterData = data;
            sdnBehaviour.enabled = true;

            LoadSprites(sdnBehaviour, spriter, rootFolder);
            CreateChildren(sdnBehaviour, spriter, go);

            string prefabPath = rootFolder + "/" + spriter.Entities[0].Name + ".prefab";
            PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.ConnectToPrefab);

            GameObject.DestroyImmediate(go);
        }

        private static void CreateChildren(SpriterDotNetBehaviour sdnBehaviour, Spriter spriter, GameObject parent)
        {
            SpriterEntity entity = spriter.Entities[0];
            int maxObjects = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                foreach (SpriterMainLineKey mainKey in animation.MainlineKeys)
                {
                    maxObjects = Math.Max(maxObjects, mainKey.ObjectRefs.Length);
                }
            }

            sdnBehaviour.children = new GameObject[maxObjects];
            sdnBehaviour.pivots = new GameObject[maxObjects];

            for (int i = 0; i < maxObjects; ++i)
            {
                GameObject pivot = new GameObject();
                GameObject child = new GameObject();

                sdnBehaviour.pivots[i] = pivot;
                sdnBehaviour.children[i] = child;

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
            sdnBehaviour.folders = new SdnFolder[spriter.Folders.Length];

            for (int i = 0; i < spriter.Folders.Length; ++i)
            {
                SpriterFolder folder = spriter.Folders[i];
                SdnFolder sdnFolder = new SdnFolder();
                sdnFolder.Files = new Sprite[folder.Files.Length];
                sdnBehaviour.folders[i] = sdnFolder;

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
