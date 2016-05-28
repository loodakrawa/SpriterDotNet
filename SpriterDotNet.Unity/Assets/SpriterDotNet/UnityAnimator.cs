// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet;
using System.Collections.Generic;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class UnityAnimator : Animator<Sprite, AudioClip>
    {
        public string SortingLayer { get; set; }
        public int SortingOrder { get; set; }

        private const float DefaultPPU = 100.0f;
        private const float DefaultPivot = 0.5f;

        private ChildData childData;
        private SpriteRenderer[] renderers;
        private AudioSource audioSource;
        private int index;
        private int boxIndex;
        private int pointIndex;

        public UnityAnimator(SpriterEntity entity, ChildData childData, AudioSource audioSource) : base(entity)
        {
            this.childData = childData;
            this.audioSource = audioSource;

            renderers = new SpriteRenderer[childData.Sprites.Length];
            for (int i = 0; i < childData.Sprites.Length; ++i)
            {
                renderers[i] = childData.Sprites[i].GetComponent<SpriteRenderer>();
            }
        }

        protected override void Animate(float deltaTime)
        {
            index = 0;
            boxIndex = 0;
            pointIndex = 0;

            base.Animate(deltaTime);

            while(index < childData.Sprites.Length)
            {
                renderers[index].sprite = null;
                childData.Sprites[index].SetActive(false);
                childData.SpritePivots[index].SetActive(false);
                ++index;
            }
            while (boxIndex < childData.Boxes.Length)
            {
                childData.Boxes[boxIndex].SetActive(false);
                childData.BoxPivots[boxIndex].SetActive(false);
                ++boxIndex;
            }
            while(pointIndex < childData.Points.Length)
            {
                childData.Points[pointIndex].SetActive(false);
                ++pointIndex;
            }
        }

        protected override void ApplySpriteTransform(Sprite sprite, SpriterObject info)
        {
            GameObject child = childData.Sprites[index];
            GameObject pivot = childData.SpritePivots[index];
            Transform childTransform = childData.SpriteTransforms[index];
            Transform pivotTransform = childData.SpritePivotTransforms[index];

            child.SetActive(true);
            pivot.SetActive(true);
            SpriteRenderer renderer = renderers[index];

            float ppu = sprite.pixelsPerUnit;

            renderer.sprite = sprite;
            Vector3 size = sprite.bounds.size;
            float spritePivotX = sprite.pivot.x / ppu / size.x;
            float spritePivotY = sprite.pivot.y / ppu / size.y;

            float deltaX = (spritePivotX - info.PivotX) * size.x * info.ScaleX;
            float deltaY = (spritePivotY - info.PivotY) * size.y * info.ScaleY;

            Color c = renderer.color;
            renderer.color = new Color(c.r, c.g, c.b, info.Alpha);
            pivotTransform.localEulerAngles = new Vector3(0, 0, info.Angle);
            pivotTransform.localPosition = new Vector3(info.X / ppu, info.Y / ppu, 0);
            childTransform.localPosition = new Vector3(deltaX, deltaY, childTransform.localPosition.z);
            childTransform.localScale = new Vector3(info.ScaleX, info.ScaleY, 1);

            renderer.sortingLayerName = SortingLayer;
            renderer.sortingOrder = SortingOrder * renderers.Length + index;

            ++index;
        }

        protected override void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
            GameObject child = childData.Boxes[boxIndex];
            GameObject pivot = childData.BoxPivots[boxIndex];
            Transform childTransform = childData.BoxTransforms[boxIndex];
            Transform pivotTransform = childData.BoxPivotTransforms[boxIndex];
            child.SetActive(true);
            pivot.SetActive(true);

            float w = objInfo.Width / DefaultPPU;
            float h = objInfo.Height / DefaultPPU;

            BoxCollider2D collider = child.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(w, h);

            child.name = objInfo.Name;

            float deltaX = (DefaultPivot - info.PivotX) * w * info.ScaleX;
            float deltaY = (DefaultPivot - info.PivotY) * h * info.ScaleY;

            pivotTransform.localEulerAngles = new Vector3(0, 0, info.Angle);
            pivotTransform.localPosition = new Vector3(info.X / DefaultPPU, info.Y / DefaultPPU, 0);
            childTransform.localPosition = new Vector3(deltaX, deltaY, childTransform.localPosition.z);
            childTransform.localScale = new Vector3(info.ScaleX, info.ScaleY, 1);
            ++boxIndex;
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            GameObject point = childData.Points[pointIndex];
            Transform pointTransform = childData.PointTransforms[pointIndex];
            point.name = name;
            point.SetActive(true);

            float x = info.X / DefaultPPU;
            float y = info.Y / DefaultPPU;

            pointTransform.localPosition = new Vector3(x, y, pointTransform.localPosition.z);

            ++pointIndex;
        }

        protected override void PlaySound(AudioClip sound, SpriterSound info)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif

            audioSource.panStereo = info.Panning;
            audioSource.PlayOneShot(sound, info.Volume);
        }

        /// <summary>
        /// In order to compile on console with AOT-Only, we must provide public definitions for some of the generic classes used internally.
        /// </summary>
        public Dictionary<int, Sprite> aot_SpritesByInt = new Dictionary<int, Sprite>();
        public Dictionary<int, IDictionary<int, Sprite>> aot_SpriteByIntNested = new Dictionary<int, IDictionary<int, Sprite>>();
        public Dictionary<int, AudioClip> aot_AudioByInt = new Dictionary<int, AudioClip>();
        public Dictionary<int, IDictionary<int, AudioClip>> aot_AudioByIntNested = new Dictionary<int, IDictionary<int, AudioClip>>();
    }
}
