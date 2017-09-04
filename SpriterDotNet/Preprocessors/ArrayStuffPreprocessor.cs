// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;
using System.Diagnostics;

namespace SpriterDotNet.Preprocessors
{
    public class ArrayStuffPreprocessor : ISpriterPreprocessor
    {
        public void Preprocess(Spriter spriter)
        {
            Debug.WriteLine("Processing Entity: " + spriter);
            Counts counts = GetCounts(spriter);

            // Inisialise struct arrays to hold all the data we're going to interpolate.
            Transform[] boneData = new Transform[counts.TransformCount];
            ObjectData[] objectData = new ObjectData[counts.ObjectDataCount];

            // Fill the data structures.
            FillTransforms(counts.TransformIndices, boneData);
            FillObjectData(counts.ObjectDataIndices, objectData);

            //

            Debug.WriteLine("Transform Count: " + counts.TransformCount);
            Debug.WriteLine("Object Data Count: " + counts.ObjectDataCount);
        }

        private static void FillTransforms(Dictionary<SpriterTimeline, int> timelineTransformIndices, Transform[] transforms)
        {
            foreach (var entry in timelineTransformIndices)
            {
                int index = entry.Value;
                SpriterTimeline timeline = entry.Key;

                bool isBone = timeline.ObjectType == SpriterObjectType.Bone;

                for (int i = 0; i < timeline.Keys.Length; ++i)
                {
                    SpriterTimelineKey key = timeline.Keys[i];
                    Debug.Assert(default(Transform).Equals(transforms[index + i]));
                    FillTransform(ref transforms[index + i], isBone ? key.BoneInfo : key.ObjectInfo);
                }
            }
        }

        private static void FillObjectData(Dictionary<SpriterTimeline, int> objectDataTransformIndices, ObjectData[] objectData)
        {
            foreach (var entry in objectDataTransformIndices)
            {
                int index = entry.Value;
                if (index < 0) continue;

                SpriterTimeline timeline = entry.Key;

                for (int i = 0; i < timeline.Keys.Length; ++i)
                {
                    SpriterTimelineKey key = timeline.Keys[i];
                    Debug.Assert(default(ObjectData).Equals(objectData[index + i]));
                    FillObjectData(ref objectData[index + i], key.ObjectInfo);
                }
            }
        }

        private static void FillTransform(ref Transform t, SpriterSpatial ss)
        {
            t.Alpha = ss.Alpha;
            t.ScaleX = ss.ScaleX;
            t.ScaleY = ss.ScaleY;
            t.Angle = ss.Angle;
            t.X = ss.X;
            t.Y = ss.Y;
        }

        private static void FillObjectData(ref ObjectData o, SpriterObject ss)
        {
            o.AnimationId = ss.AnimationId;
            o.EntityId = ss.EntityId;
            o.FileId = ss.FileId;
            o.FolderId = ss.FolderId;
            o.PivotX = ss.PivotX;
            o.PivotY = ss.PivotY;
        }

        private static Counts GetCounts(Spriter spriter)
        {
            Counts ret = new Counts()
            {
                TransformIndices = new Dictionary<SpriterTimeline, int>(),
                ObjectDataIndices = new Dictionary<SpriterTimeline, int>(),
            };

            foreach (var entity in spriter.Entities)
            {
                foreach (var animation in entity.Animations)
                {
                    foreach (var timeline in animation.Timelines)
                    {
                        bool isBone = timeline.ObjectType == SpriterObjectType.Bone;

                        int length = timeline.Keys.Length;

                        // All objects in timelines have transforms.
                        ret.TransformIndices[timeline] = ret.TransformCount;
                        ret.TransformCount += length;

                        // Everything except bones has additional object data.
                        if (!isBone)
                        {
                            ret.ObjectDataIndices[timeline] = ret.ObjectDataCount;
                            ret.ObjectDataCount += length;
                        }
                        else
                        {
                            // If it's for a bone, the timeline doesn't have object data.
                            ret.ObjectDataIndices[timeline] = -1;
                        }
                    }
                }
            }

            return ret;
        }

        private static int GetTimelineCount(Spriter spriter)
        {
            int count = 0;

            foreach (var entity in spriter.Entities)
            {
                foreach (var animation in entity.Animations) count += animation.Timelines.Length;
            }

            return count;
        }

        private struct Counts
        {
            public int TransformCount;
            public int ObjectDataCount;
            public Dictionary<SpriterTimeline, int> TransformIndices;
            public Dictionary<SpriterTimeline, int> ObjectDataIndices;
        }
    }
}
