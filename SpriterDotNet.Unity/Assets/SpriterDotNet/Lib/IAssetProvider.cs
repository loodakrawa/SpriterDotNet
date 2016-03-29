// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
    public interface IAssetProvider<T>
    {
        /// <summary>
        /// The current character map. If set to null, the default is used.
        /// </summary>
        SpriterCharacterMap CharacterMap { get; }

        /// <summary>
        /// Gets the asset associated for the given folderId and fileId.
        /// </summary>
        T Get(int folderId, int fileId);

        /// <summary>
        /// Gets the mapped ids for the given folderId and fileId.
        /// </summary>
        KeyValuePair<int, int> GetMapping(int folderId, int fileId);

        // <summary>
        /// Associates the asset with the given folderId and fileId.
        /// </summary>
        void Set(int folderId, int fileId, T asset);

        /// <summary>
        /// Swaps one asset with another
        /// </summary>
        void Swap(T original, T replacement);

        /// <summary>
        /// Removes a asset swap
        /// </summary>
        void Unswap(T original);

        /// <summary>
        /// Applies the provided character map
        /// </summary>
        void PushCharMap(SpriterCharacterMap charMap);

        /// <summary>
        /// Removes the top character map
        /// </summary>
        void PopCharMap();
    }
}
