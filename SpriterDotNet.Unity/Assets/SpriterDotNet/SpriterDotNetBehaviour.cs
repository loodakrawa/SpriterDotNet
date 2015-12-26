// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet;
using System;
using UnityEngine;

namespace SpriterDotNetUnity
{
    [Serializable]
    public class ChildData
    {
        public GameObject[] SpritePivots;
        public GameObject[] Sprites;
        public GameObject[] BoxPivots;
        public GameObject[] Boxes;
        public GameObject[] Points;
    }

    [ExecuteInEditMode]
    public class SpriterDotNetBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public string SortingLayer;

        [HideInInspector]
        public ChildData ChildData;

        [HideInInspector]
        public int EntityIndex;

        [HideInInspector]
        public SpriterData SpriterData;

        [HideInInspector]
        public bool UseNativeTags;

        public UnitySpriterAnimator Animator { get; private set; }

        private string defaultTag;

        public void Start()
        {
            SpriterEntity entity = SpriterData.Spriter.Entities[EntityIndex];
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();

            Animator = new UnitySpriterAnimator(entity, ChildData, audioSource);
            RegisterSpritesAndSounds();

            if (UseNativeTags) defaultTag = gameObject.tag;

            Animator.Step(0);
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif

            if (Animator == null) return;

            Animator.SortingLayer = SortingLayer;
            Animator.Step(Time.deltaTime * 1000.0f);

            if (UseNativeTags)
            {
                var tags = Animator.Metadata.AnimationTags;
                if (tags != null && tags.Count > 0) gameObject.tag = tags[0];
                else gameObject.tag = defaultTag;
            }
        }

        private void RegisterSpritesAndSounds()
        {
            foreach (SdnFileEntry entry in SpriterData.FileEntries)
            {
                if (entry.Sprite != null) Animator.Register(entry.FolderId, entry.FileId, entry.Sprite);
                else Animator.Register(entry.FolderId, entry.FileId, entry.Sound);
            }
        }
    }
}
