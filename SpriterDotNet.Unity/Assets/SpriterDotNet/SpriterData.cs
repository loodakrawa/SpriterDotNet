using SpriterDotNet;
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
            if(Spriter != null) SpriterParser.Init(Spriter);
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
