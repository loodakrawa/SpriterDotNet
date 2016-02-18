using System;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public static class SpriterUtil
    {
        public static float[] GetDimensions(this SpriterEntity entity)
        {
            FrameData fd = new FrameData();

            float width = 0;
            float height = 0;

            foreach (SpriterAnimation anim in entity.Animations)
            {
                float left = float.MaxValue;
                float right = float.MinValue;
                float bottom = float.MaxValue;
                float top = float.MinValue;

                foreach (SpriterMainlineKey key in anim.MainlineKeys)
                {
                    SpriterProcessor.UpdateFrameData(fd, anim, key.Time);
                    foreach (SpriterObject so in fd.SpriteData)
                    {
                        SpriterFile file = entity.Spriter.Folders[so.FolderId].Files[so.FileId];
                        float l = so.X - (file.Width * so.PivotX) / 2;
                        float r = l + file.Width;
                        float b = so.Y - (file.Height * (1 - so.PivotY)) / 2;
                        float t = b + file.Height;
                        left = Math.Min(left, l);
                        right = Math.Max(right, r);
                        bottom = Math.Min(bottom, b);
                        top = Math.Max(top, t);
                    }
                }

                width = Math.Max(width, right - left);
                height = Math.Max(height, top - bottom);
            }

            return new float[] { width, height };
        }
    }
}
