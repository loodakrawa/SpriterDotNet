// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;
using UnityEngine;
using SpriterDotNet;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

namespace SpriterDotNetUnity
{
    public class SpriterRuntimeImporter
    {
        private struct PathToEntity
        {
            public string path;
            public string entity;

            public PathToEntity(string path, string entity)
            {
                this.path = path;
                this.entity = entity;
            }
        }

        private struct SpriterEntityData
        {
            public SpriterEntity entity;
            public SpriterData data;

            public SpriterEntityData(SpriterEntity entity, SpriterData data)
            {
                this.entity = entity;
                this.data = data;
            }
        }

        private static readonly Dictionary<PathToEntity, SpriterEntityData> SpriterEntityDatas = new Dictionary<PathToEntity, SpriterEntityData>();

        private static readonly string ObjectNameSprites = "Sprites";
        private static readonly string ObjectNameMetadata = "Metadata";

        private static SpriterEntity FetchOrCacheSpriterEntityDataFromFile(string path, string entityName, SpriterDotNetBehaviour spriterDotNetBehaviour)
        {
            if (SpriterEntityDatas.TryGetValue(new PathToEntity(path, entityName), out SpriterEntityData cachedEntityData))
            {
                spriterDotNetBehaviour.SpriterData = cachedEntityData.data;
                return cachedEntityData.entity;
            }

            string data = File.ReadAllText(path);
            Spriter spriter = SpriterReader.Default.Read(data);
            string rootFolder = Path.GetDirectoryName(path);

            SpriterEntity requestedEntity = null;
            foreach (SpriterEntity entity in spriter.Entities)
            {
                bool isRequestedEntity = entity.Name == entityName;
                SpriterData spriterData = CreateSpriterData(spriter, rootFolder, spriterDotNetBehaviour, isRequestedEntity);
                SpriterEntityData entityData = new SpriterEntityData(entity, spriterData);
                SpriterEntityDatas[new PathToEntity(path, entity.Name)] = entityData;
                if (isRequestedEntity)
                {
                    requestedEntity = entity;
                    spriterDotNetBehaviour.SpriterData = spriterData;
                }
            }

            return requestedEntity;
        }

        public static SpriterDotNetBehaviour CreateSpriter(string path, string entityName, Transform parent = null)
        {
            GameObject go = new GameObject(entityName);
            SpriterDotNetBehaviour behaviour = go.AddComponent<SpriterDotNetBehaviour>();
            SpriterEntity entity = FetchOrCacheSpriterEntityDataFromFile(path, entityName, behaviour);
            if (entity == null)
            {
                UnityEngine.Object.Destroy(go);
                return null;
            }

            GameObject sprites = new GameObject(ObjectNameSprites);
            GameObject metadata = new GameObject(ObjectNameMetadata);

            behaviour.UseNativeTags = false;
            if (SpriterImporterUtil.HasSound(entity)) go.AddComponent<AudioSource>();

            sprites.SetParent(go);
            metadata.SetParent(go);

            ChildData cd = new ChildData();
            SpriterImporterUtil.CreateSprites(entity, cd, sprites);
            SpriterImporterUtil.CreateCollisionRectangles(entity, cd, metadata);
            SpriterImporterUtil.CreatePoints(entity, cd, metadata);
            cd.Verify();

            behaviour.EntityIndex = entity.Id;
            behaviour.enabled = true;
            behaviour.ChildData = cd;

            go.transform.parent = parent;
            return behaviour;
        }

        private static SpriterData CreateSpriterData(Spriter spriter, string rootFolder, SpriterDotNetBehaviour spriterDotNetBehaviour, bool andAssignAudioClips)
        {
            SpriterData data = ScriptableObject.CreateInstance<SpriterData>();
            data.Spriter = spriter;
            data.FileEntries = LoadAssets(spriter, rootFolder, spriterDotNetBehaviour, andAssignAudioClips).ToArray();

            return data;
        }

        private static IEnumerator GetAudioClip(SdnFileEntry entry, string path, SpriterDotNetBehaviour spriterDotNetBehaviour, bool andAssign)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    entry.Sound = DownloadHandlerAudioClip.GetContent(www);
                    if (andAssign)
                    {
                        spriterDotNetBehaviour.Animator.SoundProvider.Set(entry.FolderId, entry.FileId, entry.Sound);
                    }
                }
            }
        }

        private static IEnumerable<SdnFileEntry> LoadAssets(Spriter spriter, string rootFolder, SpriterDotNetBehaviour spriterDotNetBehaviour, bool andAssignAudioClips)
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

                    if (file.Type == SpriterFileType.Sound)
                    {
                        spriterDotNetBehaviour.StartCoroutine(GetAudioClip(entry, path, spriterDotNetBehaviour, andAssignAudioClips));
                    }
                    else
                    {
                        entry.Sprite = LoadNewSprite(path);
                    }

                    yield return entry;
                }
            }
        }

        private static Sprite LoadNewSprite(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            byte[] FileData = File.ReadAllBytes(FilePath);
            Texture2D SpriteTexture = new Texture2D(2, 2);

            if (!SpriteTexture.LoadImage(FileData))
            {

                return null;
            }

            return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0));
        }
    }
}
