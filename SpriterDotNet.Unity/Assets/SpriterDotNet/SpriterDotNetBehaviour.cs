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

        public Transform[] SpritePivotTransforms;
        public Transform[] SpriteTransforms;
        public Transform[] BoxPivotTransforms;
        public Transform[] BoxTransforms;
        public Transform[] PointTransforms;

        public void Verify()
        {
            if (SpritePivots == null) SpritePivots = new GameObject[0];
            if (Sprites == null) Sprites = new GameObject[0];
            if (BoxPivots == null) BoxPivots = new GameObject[0];
            if (Boxes == null) Boxes = new GameObject[0];
            if (Points == null) Points = new GameObject[0];
            if (SpritePivotTransforms == null) SpritePivotTransforms = new Transform[0];
            if (SpriteTransforms == null) SpriteTransforms = new Transform[0];
            if (BoxPivotTransforms == null) BoxPivotTransforms = new Transform[0];
            if (BoxTransforms == null) BoxTransforms = new Transform[0];
            if (PointTransforms == null) PointTransforms = new Transform[0];
        }
    }

    [ExecuteInEditMode]
    public class SpriterDotNetBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public string SortingLayer = "Default";

        [HideInInspector]
        public int SortingOrder;

        [SerializeField]
        private float Ppu = 100;

        [HideInInspector]
        public ChildData ChildData;

        [HideInInspector]
        public int EntityIndex;

        [HideInInspector]
        public SpriterData SpriterData;

        [HideInInspector]
        public bool UseNativeTags;

        public UnityAnimator Animator { get; private set; }

        private string defaultTag;

        private void Start()
        {
            SpriterEntity entity = SpriterData.Spriter.Entities[EntityIndex];
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();

            Animator = new UnityAnimator(entity, ChildData, audioSource, Ppu);
            RegisterSpritesAndSounds();

            if (UseNativeTags) defaultTag = gameObject.tag;

            Animator.Update(0);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif

            if (Animator == null) return;

            Animator.SortingLayer = SortingLayer;
            Animator.SortingOrder = SortingOrder;
            Animator.Update(Time.deltaTime * 1000.0f);

            if (UseNativeTags)
            {
                var tags = Animator.FrameData.AnimationTags;
                if (tags != null && tags.Count > 0) gameObject.tag = tags[0];
                else gameObject.tag = defaultTag;
            }
        }

        private void RegisterSpritesAndSounds()
        {
            foreach (SdnFileEntry entry in SpriterData.FileEntries)
            {
                if (entry.Sprite != null) Animator.SpriteProvider.Set(entry.FolderId, entry.FileId, entry.Sprite);
                else Animator.SoundProvider.Set(entry.FolderId, entry.FileId, entry.Sound);
            }
        }
    }
}
