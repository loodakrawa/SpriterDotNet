// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using SpriterDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class SpriterImporter : AssetPostprocessor
    {
        private static readonly string AutosaveExtension = ".autosave.scml";
        private static readonly string[] ScmlExtensions = new string[] { ".scml" };
        private static readonly string ObjectNameSprites = "Sprites";
        private static readonly string ObjectNameMetadata = "Metadata";

        public static bool UseNativeTags = true;

        public static event Action<SpriterEntity, GameObject> EntityImported = (e, p) => { };

        public static IContentLoader ContentLoader = new DefaultContentLoader();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            foreach (string asset in importedAssets)
            {
                if (!IsScml(asset)) continue;
                CreateSpriter(asset);
            }
        }

        private static bool IsScml(string path)
        {
            return ScmlExtensions.Any(path.EndsWith) && !path.EndsWith(AutosaveExtension);
        }

        private static void CreateSpriter(string path)
        {
            string data = File.ReadAllText(path);
            Spriter spriter = SpriterReader.Default.Read(data);
            string rootFolder = Path.GetDirectoryName(path);

            string name = Path.GetFileNameWithoutExtension(path);
            SpriterData spriterData = CreateSpriterData(spriter, rootFolder, name);

            foreach (SpriterEntity entity in spriter.Entities)
            {
                GameObject go = new GameObject(entity.Name);
                GameObject sprites = new GameObject(ObjectNameSprites);
                GameObject metadata = new GameObject(ObjectNameMetadata);

                SpriterDotNetBehaviour behaviour = go.AddComponent<SpriterDotNetBehaviour>();
                behaviour.UseNativeTags = UseNativeTags;
                if (HasSound(entity)) go.AddComponent<AudioSource>();

                sprites.SetParent(go);
                metadata.SetParent(go);

                ChildData cd = new ChildData();
                CreateSprites(entity, cd, spriter, sprites);
                CreateCollisionRectangles(entity, cd, spriter, metadata);
                CreatePoints(entity, cd, spriter, metadata);

                behaviour.EntityIndex = entity.Id;
                behaviour.enabled = true;
                behaviour.SpriterData = spriterData;
                behaviour.ChildData = cd;

                GameObject prefab = CreatePrefab(go, rootFolder);

                EntityImported(entity, prefab);
            }

            if (UseNativeTags) CreateTags(spriter);
        }

        private static SpriterData CreateSpriterData(Spriter spriter, string rootFolder, string name)
        {
            SpriterData data = ScriptableObject.CreateInstance<SpriterData>();
            data.Spriter = spriter;
            data.FileEntries = LoadAssets(spriter, rootFolder).ToArray();

            AssetDatabase.CreateAsset(data, rootFolder + "/" + name + ".asset");
            AssetDatabase.SaveAssets();

            return data;
        }

        private static GameObject CreatePrefab(GameObject go, string folder)
        {
            string prefabPath = folder + "/" + go.name + ".prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject prefab;
            if (existingPrefab != null) prefab = ReplacePrefab(go, existingPrefab, prefabPath);
            else prefab = PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.Default);

            GameObject.DestroyImmediate(go);

            return prefab;
        }

        private static GameObject ReplacePrefab(GameObject go, GameObject prefab, string path)
        {
            GameObject existing = GameObject.Instantiate(prefab);
            MoveChild(go, existing, ObjectNameSprites);
            MoveChild(go, existing, ObjectNameMetadata);

            SpriterDotNetBehaviour sdnbNew = go.GetComponent<SpriterDotNetBehaviour>();
            SpriterDotNetBehaviour sdnbExisting = existing.GetComponent<SpriterDotNetBehaviour>();
            sdnbExisting.ChildData = sdnbNew.ChildData;
            sdnbExisting.EntityIndex = sdnbNew.EntityIndex;
            sdnbExisting.SpriterData = sdnbNew.SpriterData;
            sdnbExisting.UseNativeTags = sdnbNew.UseNativeTags;

            GameObject createdPrefab = PrefabUtility.ReplacePrefab(existing, prefab, ReplacePrefabOptions.Default);
            GameObject.DestroyImmediate(existing);
            return createdPrefab;
        }

        private static void MoveChild(GameObject from, GameObject to, string name)
        {
            Transform toChild = to.transform.Find(name);
            GameObject.DestroyImmediate(toChild.gameObject);

            Transform fromChild = from.transform.Find(name);
            fromChild.SetParent(to.transform);
            fromChild.localPosition = Vector3.zero;
            fromChild.localRotation = Quaternion.identity;
            fromChild.localScale = Vector3.one;
        }

        private static void CreateSprites(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            int maxObjects = GetDrawablesCount(entity);

            cd.Sprites = new GameObject[maxObjects];
            cd.SpritePivots = new GameObject[maxObjects];
            cd.SpriteTransforms = new Transform[maxObjects];
            cd.SpritePivotTransforms = new Transform[maxObjects];

            for (int i = 0; i < maxObjects; ++i)
            {
                GameObject pivot = new GameObject("Pivot " + i);
                GameObject child = new GameObject("Sprite " + i);

                pivot.SetParent(parent);
                child.SetParent(pivot);

                cd.SpritePivots[i] = pivot;
                cd.Sprites[i] = child;
                cd.SpritePivotTransforms[i] = pivot.transform;
                cd.SpriteTransforms[i] = child.transform;

                child.transform.localPosition = Vector3.zero;

                child.AddComponent<SpriteRenderer>();
            }
        }

        private static void CreateCollisionRectangles(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            if (entity.ObjectInfos == null) return;
            var boxes = entity.ObjectInfos.Where(o => o.ObjectType == SpriterObjectType.Box).ToList();
            if (boxes.Count == 0) return;

            GameObject boxRoot = new GameObject("Boxes");
            boxRoot.SetParent(parent);

            cd.BoxPivots = new GameObject[boxes.Count];
            cd.Boxes = new GameObject[boxes.Count];
            cd.BoxTransforms = new Transform[boxes.Count];
            cd.BoxPivotTransforms = new Transform[boxes.Count];

            for (int i = 0; i < boxes.Count; ++i)
            {
                GameObject pivot = new GameObject("Pivot " + i);
                GameObject child = new GameObject("Box " + i);

                pivot.SetParent(boxRoot);
                child.SetParent(pivot);

                cd.BoxPivots[i] = pivot;
                cd.Boxes[i] = child;
                cd.BoxPivotTransforms[i] = pivot.transform;
                cd.BoxTransforms[i] = child.transform;

                child.AddComponent<BoxCollider2D>();
            }
        }

        private static void CreatePoints(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            GameObject pointRoot = new GameObject("Points");
            pointRoot.SetParent(parent);

            int count = GetPointsCount(entity);

            cd.Points = new GameObject[count];
            cd.PointTransforms = new Transform[count];

            for (int i = 0; i < count; ++i)
            {
                GameObject point = new GameObject("Point " + i);
                point.SetParent(pointRoot);
                cd.Points[i] = point;
                cd.PointTransforms[i] = point.transform;
            }
        }

        private static IEnumerable<SdnFileEntry> LoadAssets(Spriter spriter, string rootFolder)
        {
            for (int i = 0; i < spriter.Folders.Length; ++i)
            {
                SpriterFolder folder = spriter.Folders[i];

                for (int j = 0; j < folder.Files.Length; ++j)
                {
                    SpriterFile file = folder.Files[j];
                    string path = rootFolder;
                    path += "/";
                    path += file.Name;

                    SdnFileEntry entry = new SdnFileEntry
                    {
                        FolderId = folder.Id,
                        FileId = file.Id
                    };

                    if (file.Type == SpriterFileType.Sound) entry.Sound = ContentLoader.Load<AudioClip>(path);
                    else entry.Sprite = ContentLoader.Load<Sprite>(path);

                    yield return entry;
                }
            }
        }

        private static void CreateTags(Spriter spriter)
        {
            if (spriter.Tags == null) return;

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            foreach (SpriterElement tag in spriter.Tags) AddTag(tags, tag.Name);

            tagManager.ApplyModifiedProperties();
        }

        private static void AddTag(SerializedProperty tags, string value)
        {
            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty t = tags.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value)) return;
            }

            ++tags.arraySize;
            SerializedProperty newEntry = tags.GetArrayElementAtIndex(tags.arraySize - 1);
            newEntry.stringValue = value;
        }

        private static bool HasSound(SpriterEntity entity)
        {
            foreach (SpriterAnimation animation in entity.Animations)
            {
                if (animation.Soundlines != null && animation.Soundlines.Length > 0) return true;
                if (animation.Timelines == null) continue;
                foreach(SpriterTimeline timeline in animation.Timelines)
                {
                    if (timeline.ObjectType != SpriterObjectType.Entity || timeline.Keys == null) continue;
                    foreach(SpriterTimelineKey key in timeline.Keys)
                    {
                        if (key.ObjectInfo == null) continue;
                        bool hasSound = HasSound(entity.Spriter.Entities[key.ObjectInfo.EntityId]);
                        if (hasSound) return true;
                    }
                }
            }
            return false;
        }

        private static int GetDrawablesCount(SpriterEntity entity)
        {
			if(entity.Animations == null) return 0;
			
            int drawablesCount = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                int count = GetDrawablesCount(animation);
                drawablesCount = Math.Max(drawablesCount, count);
            }

            return drawablesCount;
        }

        private static int GetDrawablesCount(SpriterAnimation animation)
        {
			if(animation.MainlineKeys == null) return 0;
			
            int drawablesCount = 0;

            foreach (SpriterMainlineKey key in animation.MainlineKeys)
            {
                int countForKey = GetDrawablesCount(animation, key);
                drawablesCount = Math.Max(drawablesCount, countForKey);
            }

            return drawablesCount;
        }

        private static int GetDrawablesCount(SpriterAnimation animation, SpriterMainlineKey key)
        {
			if(key.ObjectRefs == null) return 0;
			
            int drawablesCount = 0;

            foreach (SpriterObjectRef obj in key.ObjectRefs)
            {
                SpriterTimeline timeline = animation.Timelines[obj.TimelineId];
                if (timeline.ObjectType == SpriterObjectType.Sprite) ++drawablesCount;
                else if (timeline.ObjectType == SpriterObjectType.Entity)
                {
                    Spriter spriter = animation.Entity.Spriter;
                    HashSet<SpriterAnimation> animations = new HashSet<SpriterAnimation>();
                    foreach (SpriterTimelineKey timelineKey in timeline.Keys)
                    {
                        SpriterObject spriterObject = timelineKey.ObjectInfo;
                        SpriterAnimation newAnim = spriter.Entities[spriterObject.EntityId].Animations[spriterObject.AnimationId];
                        if (!animations.Contains(newAnim)) animations.Add(newAnim);
                    }
                    IEnumerable<int> drawableCount = animations.Select<SpriterAnimation, int>(GetDrawablesCount);
                    drawablesCount += drawableCount.Max();
                }
            }

            return drawablesCount;
        }

        private static int GetPointsCount(SpriterEntity entity)
        {
			if(entity.Animations == null) return 0;
			
            int count = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                int countForAnim = animation.Timelines.Where(t => t.ObjectType == SpriterObjectType.Point).Count();
                count = Math.Max(count, countForAnim);
            }

            return count;
        }
    }

    internal static class SpriterImporterUtil
    {
        public static void SetParent(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
        }
    }
}

#endif