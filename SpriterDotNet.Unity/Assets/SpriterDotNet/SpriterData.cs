// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet;
using SpriterDotNet.Preprocessors;
using System;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class SpriterData : ScriptableObject
    {
        public Spriter Spriter;

        public SdnFileEntry[] FileEntries;

        public void OnEnable()
        {
            if(Spriter != null) new SpriterInitPreprocessor().Preprocess(Spriter);
        }
    }

    [Serializable]
    public class SdnFileEntry
    {
        public int FolderId;
        public int FileId;
        public Sprite Sprite;
        public AudioClip Sound;
    }
}
