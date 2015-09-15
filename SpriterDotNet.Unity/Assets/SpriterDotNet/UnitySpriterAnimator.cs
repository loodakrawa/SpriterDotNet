// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using SpriterDotNet;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class UnitySpriterAnimator : SpriterAnimator<Sprite>
    {
        private GameObject[] pivots;
        private GameObject[] children;
        private SpriteRenderer[] renderers;
        private int index;

        public UnitySpriterAnimator(SpriterEntity entity, GameObject[] pivots, GameObject[] children) : base(entity)
        {
            this.pivots = pivots;
            this.children = children;
            
            renderers = new SpriteRenderer[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                renderers[i] = children[i].GetComponent<SpriteRenderer>();
            }
        }

        protected override void ApplyTransform(Sprite sprite, SpriterObjectInfo info)
        {
            GameObject child = children[index];
            GameObject pivot = pivots[index];
            SpriteRenderer renderer = renderers[index];

            float ppu = sprite.pixelsPerUnit;

            renderer.sprite = sprite;
            Vector3 size = sprite.bounds.size;
            float deltaX = (0.5f - info.PivotX) * size.x * info.ScaleX;
            float deltaY = (0.5f - info.PivotY) * size.y * info.ScaleY;

            pivot.transform.localEulerAngles = new Vector3(0, 0, info.Angle);
            pivot.transform.localPosition = new Vector3(info.X / ppu, info.Y / ppu, 0);
            child.transform.localPosition = new Vector3(deltaX, deltaY, child.transform.localPosition.z);
            child.transform.localScale = new Vector3(info.ScaleX, info.ScaleY, 1);

            ++index;
        }

        protected override void Animate()
        {
            foreach (SpriteRenderer renderer in renderers) renderer.sprite = null;
            index = 0;

            base.Animate();
        }
    }
}
