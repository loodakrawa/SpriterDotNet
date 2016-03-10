// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet.AssetProvider
{
    public class DefaultAssetProvider<T> : IAssetProvider<T> where T : class
    {
        public SpriterCharacterMap CharacterMap { get { return charMaps.Count > 0 ? charMaps.Peek() : null; } }

        private readonly Dictionary<int, Dictionary<int, T>> assets;
        private readonly Dictionary<T, T> swappedAssets = new Dictionary<T, T>();

        private readonly Dictionary<T, KeyValuePair<int, int>> charMapValues = new Dictionary<T, KeyValuePair<int, int>>();
        private readonly Stack<SpriterCharacterMap> charMaps = new Stack<SpriterCharacterMap>();

        public DefaultAssetProvider() : this(new Dictionary<int, Dictionary<int, T>>())
        {
        }

        public DefaultAssetProvider(Dictionary<int, Dictionary<int, T>> assets)
        {
            this.assets = assets;
        }

        public T Get(int folderId, int fileId)
        {
            T asset = GetFromDict(folderId, fileId, assets);
            if (asset == null) return asset;

            if (charMapValues.ContainsKey(asset))
            {
                KeyValuePair<int, int> mapping = charMapValues[asset];
                return Get(mapping.Key, mapping.Value);
            }

            return swappedAssets.ContainsKey(asset) ? swappedAssets[asset] : asset;
        }

        public void Set(int folderId, int fileId, T asset)
        {
            AddToDict(folderId, fileId, asset, assets);
        }

        public virtual void Swap(T original, T replacement)
        {
            swappedAssets[original] = replacement;
        }

        public virtual void Unswap(T original)
        {
            if (swappedAssets.ContainsKey(original)) swappedAssets.Remove(original);
        }

        public virtual void PushCharMap(SpriterCharacterMap charMap)
        {
            ApplyCharMap(charMap);
            charMaps.Push(charMap);
        }

        public virtual void PopCharMap()
        {
            if (charMaps.Count == 0) return;
            charMaps.Pop();
            ApplyCharMap(charMaps.Count > 0 ? charMaps.Peek() : null);
        }

        protected virtual void ApplyCharMap(SpriterCharacterMap charMap)
        {
            if (charMap == null)
            {
                charMapValues.Clear();
                return;
            }

            for (int i = 0; i < charMap.Maps.Length; ++i)
            {
                SpriterMapInstruction map = charMap.Maps[i];
                T sprite = GetFromDict(map.FolderId, map.FileId, assets);
                if (sprite == null) continue;

                charMapValues[sprite] = new KeyValuePair<int, int>(map.TargetFolderId, map.TargetFileId);
            }
        }

        private static void AddToDict<T>(int folderId, int fileId, T obj, Dictionary<int, Dictionary<int, T>> dict)
        {
            Dictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);

            if (objectsByFiles == null)
            {
                objectsByFiles = new Dictionary<int, T>();
                dict[folderId] = objectsByFiles;
            }

            objectsByFiles[fileId] = obj;
        }

        private static T GetFromDict<T>(int folderId, int fileId, Dictionary<int, Dictionary<int, T>> dict)
        {
            Dictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);
            if (objectsByFiles == null) return default(T);

            T obj;
            objectsByFiles.TryGetValue(fileId, out obj);

            return obj;
        }
    }
}
