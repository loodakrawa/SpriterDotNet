// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Helpers;
using System;

namespace SpriterDotNet
{
    public class SpriterProcessor
    {
        private readonly FrameData frameData;
        private readonly SpriterConfig config;
        private readonly SpriterObjectPool pool;

        public SpriterProcessor(SpriterConfig config, SpriterObjectPool pool)
        {
            this.config = config;
            this.pool = pool;
            frameData = new FrameData(pool);
        }

        public FrameData GetFrameData(SpriterAnimation first, SpriterAnimation second, float targetTime, float deltaTime, float factor)
        {
            frameData.Clear();

            if (first == second)
            {
                GetFrameData(first, targetTime, deltaTime);
                return frameData;
            }

            float targetTimeSecond = targetTime / first.Length * second.Length;

            SpriterMainlineKey firstKeyA;
            SpriterMainlineKey firstKeyB;
            GetMainlineKeys(first.MainlineKeys, targetTime, out firstKeyA, out firstKeyB);

            SpriterMainlineKey secondKeyA;
            SpriterMainlineKey secondKeyB;
            GetMainlineKeys(second.MainlineKeys, targetTimeSecond, out secondKeyA, out secondKeyB);

            if (!SpriterHelper.WillItBlend(firstKeyA, secondKeyA) || !SpriterHelper.WillItBlend(firstKeyB, secondKeyB))
            {
                GetFrameData(first, targetTime, deltaTime);
                return frameData;
            }

            float adjustedTimeFirst = SpriterHelper.AdjustTime(targetTime, firstKeyA, firstKeyB, first.Length);
            float adjustedTimeSecond = SpriterHelper.AdjustTime(targetTimeSecond, secondKeyA, secondKeyB, second.Length);

            SpriterSpatial[] boneInfosA = GetBoneInfos(firstKeyA, first, adjustedTimeFirst);
            SpriterSpatial[] boneInfosB = GetBoneInfos(secondKeyA, second, adjustedTimeSecond);
            SpriterSpatial[] boneInfos = null;
            if (boneInfosA != null && boneInfosB != null)
            {
                boneInfos = pool.GetArray<SpriterSpatial>(boneInfosA.Length);
                for (int i = 0; i < boneInfosA.Length; ++i)
                {
                    SpriterSpatial boneA = boneInfosA[i];
                    SpriterSpatial boneB = boneInfosB[i];
                    SpriterSpatial interpolated = Interpolate(boneA, boneB, factor, 1);
                    interpolated.Angle = MathHelper.CloserAngleLinear(boneA.Angle, boneB.Angle, factor);
                    boneInfos[i] = interpolated;
                }
            }

            SpriterMainlineKey baseKey = factor < 0.5f ? firstKeyA : firstKeyB;
            SpriterAnimation currentAnimation = factor < 0.5f ? first : second;

            for (int i = 0; i < baseKey.ObjectRefs.Length; ++i)
            {
                SpriterObjectRef objectRefFirst = baseKey.ObjectRefs[i];
                SpriterObject interpolatedFirst = GetObjectInfo(objectRefFirst, first, adjustedTimeFirst);

                SpriterObjectRef objectRefSecond = secondKeyA.ObjectRefs[i];
                SpriterObject interpolatedSecond = GetObjectInfo(objectRefSecond, second, adjustedTimeSecond);

                SpriterObject info = Interpolate(interpolatedFirst, interpolatedSecond, factor, 1);
                info.Angle = MathHelper.CloserAngleLinear(interpolatedFirst.Angle, interpolatedSecond.Angle, factor);
                info.PivotX = MathHelper.Linear(interpolatedFirst.PivotX, interpolatedSecond.PivotX, factor);
                info.PivotY = MathHelper.Linear(interpolatedFirst.PivotY, interpolatedSecond.PivotY, factor);

                if (boneInfos != null && objectRefFirst.ParentId >= 0) info.ApplyParentTransform(boneInfos[objectRefFirst.ParentId]);

                AddSpatialData(info, currentAnimation.Timelines[objectRefFirst.TimelineId], currentAnimation.Entity.Spriter, deltaTime);

                pool.ReturnObject(interpolatedFirst);
                pool.ReturnObject(interpolatedSecond);
            }

            pool.ReturnObject(boneInfosA);
            pool.ReturnObject(boneInfosB);
            pool.ReturnObject(boneInfos);

            if (config.MetadataEnabled) UpdateMetadata(currentAnimation, targetTime, deltaTime);

            return frameData;
        }

        public FrameData GetFrameData(SpriterAnimation animation, float targetTime, float deltaTime, SpriterSpatial parentInfo = null)
        {
            frameData.Clear();

            SpriterMainlineKey keyA;
            SpriterMainlineKey keyB;
            GetMainlineKeys(animation.MainlineKeys, targetTime, out keyA, out keyB);

            float adjustedTime = SpriterHelper.AdjustTime(targetTime, keyA, keyB, animation.Length);

            SpriterSpatial[] boneInfos = GetBoneInfos(keyA, animation, adjustedTime, parentInfo);

            if (keyA.ObjectRefs == null)
            {
                pool.ReturnObject(boneInfos);
                return frameData;
            }

            for (int i = 0; i < keyA.ObjectRefs.Length; ++i)
            {
                SpriterObjectRef objectRef = keyA.ObjectRefs[i];
                SpriterObject interpolated = GetObjectInfo(objectRef, animation, adjustedTime);
                if (boneInfos != null && objectRef.ParentId >= 0) interpolated.ApplyParentTransform(boneInfos[objectRef.ParentId]);
                else if (parentInfo != null) interpolated.ApplyParentTransform(parentInfo);

                AddSpatialData(interpolated, animation.Timelines[objectRef.TimelineId], animation.Entity.Spriter, deltaTime);
            }

            pool.ReturnObject(boneInfos);

            if (config.MetadataEnabled) UpdateMetadata(animation, targetTime, deltaTime);

            return frameData;
        }

        public void UpdateMetadata(SpriterAnimation animation, float targetTime, float deltaTime, SpriterSpatial parentInfo = null)
        {
            if (config.VarsEnabled || config.TagsEnabled) AddVariableAndTagData(animation, targetTime);
            if (config.EventsEnabled) AddEventData(animation, targetTime, deltaTime);
            if (config.SoundsEnabled) AddSoundData(animation, targetTime, deltaTime);
        }

        private void AddVariableAndTagData(SpriterAnimation animation, float targetTime)
        {
            if (animation.Meta == null) return;

            if (config.VarsEnabled && animation.Meta.Varlines != null && animation.Meta.Varlines.Length > 0)
            {
                for (int i = 0; i < animation.Meta.Varlines.Length; ++i)
                {
                    SpriterVarline varline = animation.Meta.Varlines[i];
                    SpriterVarDef variable = animation.Entity.Variables[varline.Def];
                    frameData.AnimationVars[variable.Name] = GetVariableValue(animation, variable, varline, targetTime);
                }
            }

            SpriterElement[] tags = animation.Entity.Spriter.Tags;
            SpriterTagline tagline = animation.Meta.Tagline;
            if (config.TagsEnabled && tagline != null && tagline.Keys != null && tagline.Keys.Length > 0)
            {
                SpriterTaglineKey key = tagline.Keys.GetLastKey(targetTime);
                if (key != null && key.Tags != null)
                {
                    for (int i = 0; i < key.Tags.Length; ++i)
                    {
                        SpriterTag tag = key.Tags[i];
                        frameData.AnimationTags.Add(tags[tag.TagId].Name);
                    }
                }
            }

            for (int i = 0; i < animation.Timelines.Length; ++i)
            {
                SpriterTimeline timeline = animation.Timelines[i];
                SpriterMeta meta = timeline.Meta;
                if (meta == null) continue;

                SpriterObjectInfo objInfo = GetObjectInfo(animation, timeline.Name);

                if (config.VarsEnabled && meta.Varlines != null && meta.Varlines.Length > 0)
                {
                    for (int j = 0; j < timeline.Meta.Varlines.Length; ++j)
                    {
                        SpriterVarline varline = timeline.Meta.Varlines[j];
                        SpriterVarDef variable = objInfo.Variables[varline.Def];
                        frameData.AddObjectVar(objInfo.Name, variable.Name, GetVariableValue(animation, variable, varline, targetTime));
                    }
                }

                if (config.TagsEnabled && meta.Tagline != null && meta.Tagline.Keys != null && meta.Tagline.Keys.Length > 0)
                {
                    SpriterTaglineKey key = tagline.Keys.GetLastKey(targetTime);
                    if (key != null && key.Tags != null)
                    {
                        for (int j = 0; j < key.Tags.Length; ++j)
                        {
                            SpriterTag tag = key.Tags[j];
                            frameData.AddObjectTag(objInfo.Name, tags[tag.TagId].Name);
                        }
                    }
                }
            }
        }

        private SpriterObjectInfo GetObjectInfo(SpriterAnimation animation, string name)
        {
            SpriterObjectInfo objInfo = null;
            for (int i = 0; i < animation.Entity.ObjectInfos.Length; ++i)
            {
                SpriterObjectInfo info = animation.Entity.ObjectInfos[i];
                if (info.Name == name)
                {
                    objInfo = info;
                    break;
                }
            }

            return objInfo;
        }

        private SpriterVarValue GetVariableValue(SpriterAnimation animation, SpriterVarDef varDef, SpriterVarline varline, float targetTime)
        {
            SpriterVarlineKey[] keys = varline.Keys;
            if (keys == null) return varDef.VariableValue;

            SpriterVarlineKey keyA = keys.GetLastKey(targetTime) ?? keys[keys.Length - 1];

            if (keyA == null) return varDef.VariableValue;

            SpriterVarlineKey keyB = varline.Keys.GetNextKey(keyA, animation.Looping);

            if (keyB == null) return keyA.VariableValue;

            float adjustedTime = keyA.Time == keyB.Time ? targetTime : SpriterHelper.AdjustTime(targetTime, keyA, keyB, animation.Length);
            float factor = SpriterHelper.GetFactor(keyA, keyB, animation.Length, adjustedTime);

            SpriterVarValue varVal = pool.GetObject<SpriterVarValue>();
            varVal.Interpolate(keyA.VariableValue, keyB.VariableValue, factor);
            return varVal;
        }

        private void AddEventData(SpriterAnimation animation, float targetTime, float deltaTime)
        {
            if (animation.Eventlines == null) return;

            float previousTime = targetTime - deltaTime;
            for (int i = 0; i < animation.Eventlines.Length; ++i)
            {
                SpriterEventline eventline = animation.Eventlines[i];
                for (int j = 0; j < eventline.Keys.Length; ++j)
                {
                    SpriterKey key = eventline.Keys[j];
                    if (IsTriggered(key, targetTime, previousTime, animation.Length)) frameData.Events.Add(eventline.Name);
                }
            }
        }

        private void AddSoundData(SpriterAnimation animation, float targetTime, float deltaTime)
        {
            if (animation.Soundlines == null) return;

            float previousTime = targetTime - deltaTime;
            for (int i = 0; i < animation.Soundlines.Length; ++i)
            {
                SpriterSoundline soundline = animation.Soundlines[i];
                for (int j = 0; j < soundline.Keys.Length; ++j)
                {
                    SpriterSoundlineKey key = soundline.Keys[j];
                    SpriterSound sound = key.SoundObject;
                    if (sound.Trigger && IsTriggered(key, targetTime, previousTime, animation.Length)) frameData.Sounds.Add(sound);
                }
            }
        }

        private bool IsTriggered(SpriterKey key, float targetTime, float previousTime, float animationLength)
        {
            float timeA = Math.Min(previousTime, targetTime);
            float timeB = Math.Max(previousTime, targetTime);
            if (timeA > timeB)
            {
                if (timeA < key.Time) timeB += animationLength;
                else timeA -= animationLength;
            }
            return timeA <= key.Time && timeB >= key.Time;
        }

        private void AddSpatialData(SpriterObject info, SpriterTimeline timeline, Spriter spriter, float deltaTime)
        {
            switch (timeline.ObjectType)
            {
                case SpriterObjectType.Sprite:
                    frameData.SpriteData.Add(info);
                    break;
                case SpriterObjectType.Entity:
                    SpriterAnimation newAnim = spriter.Entities[info.EntityId].Animations[info.AnimationId];
                    float newTargetTime = info.T * newAnim.Length;
                    GetFrameData(newAnim, newTargetTime, deltaTime, info);
                    break;
                case SpriterObjectType.Point:
                    info.PivotX = 0.0f;
                    info.PivotY = 1.0f;
                    frameData.PointData[timeline.Name] = info;
                    break;
                case SpriterObjectType.Box:
                    frameData.BoxData[timeline.ObjectId] = info;
                    break;
            }
        }

        private SpriterSpatial[] GetBoneInfos(SpriterMainlineKey key, SpriterAnimation animation, float targetTime, SpriterSpatial parentInfo = null)
        {
            if (key.BoneRefs == null) return null;
            SpriterSpatial[] ret = pool.GetArray<SpriterSpatial>(key.BoneRefs.Length);

            for (int i = 0; i < key.BoneRefs.Length; ++i)
            {
                SpriterRef boneRef = key.BoneRefs[i];
                SpriterSpatial interpolated = GetBoneInfo(boneRef, animation, targetTime);

                if (boneRef.ParentId >= 0) interpolated.ApplyParentTransform(ret[boneRef.ParentId]);
                else if (parentInfo != null) interpolated.ApplyParentTransform(parentInfo);
                ret[i] = interpolated;
            }

            return ret;
        }

        private void GetMainlineKeys(SpriterMainlineKey[] keys, float targetTime, out SpriterMainlineKey keyA, out SpriterMainlineKey keyB)
        {
            keyA = keys.GetLastKey(targetTime);
            keyA = keyA ?? keys[keys.Length - 1];
            int nextKey = keyA.Id + 1;
            if (nextKey >= keys.Length) nextKey = 0;
            keyB = keys[nextKey];
        }

        private SpriterSpatial GetBoneInfo(SpriterRef spriterRef, SpriterAnimation animation, float targetTime)
        {
            SpriterTimelineKey[] keys = animation.Timelines[spriterRef.TimelineId].Keys;
            SpriterTimelineKey keyA = keys[spriterRef.KeyId];
            SpriterTimelineKey keyB = keys.GetNextKey(keyA, animation.Looping);

            if (keyB == null) return Copy(keyA.BoneInfo);

            float factor = SpriterHelper.GetFactor(keyA, keyB, animation.Length, targetTime);
            return Interpolate(keyA.BoneInfo, keyB.BoneInfo, factor, keyA.Spin);
        }

        private SpriterObject GetObjectInfo(SpriterRef spriterRef, SpriterAnimation animation, float targetTime)
        {
            SpriterTimelineKey[] keys = animation.Timelines[spriterRef.TimelineId].Keys;
            SpriterTimelineKey keyA = keys[spriterRef.KeyId];
            SpriterTimelineKey keyB = keys.GetNextKey(keyA, animation.Looping);

            if (keyB == null) return Copy(keyA.ObjectInfo);

            float factor = SpriterHelper.GetFactor(keyA, keyB, animation.Length, targetTime);
            return Interpolate(keyA.ObjectInfo, keyB.ObjectInfo, factor, keyA.Spin);
        }

        private SpriterSpatial Interpolate(SpriterSpatial a, SpriterSpatial b, float f, int spin)
        {
            SpriterSpatial ss = pool.GetObject<SpriterSpatial>();
            ss.Interpolate(a, b, f, spin);
            return ss;
        }

        private SpriterObject Interpolate(SpriterObject a, SpriterObject b, float f, int spin)
        {
            SpriterObject so = pool.GetObject<SpriterObject>();
            so.Interpolate(a, b, f, spin);
            return so;
        }

        private SpriterSpatial Copy(SpriterSpatial info)
        {
            SpriterSpatial copy = pool.GetObject<SpriterSpatial>();
            copy.FillFrom(info);
            return copy;
        }

        private SpriterObject Copy(SpriterObject obj)
        {
            SpriterObject copy = pool.GetObject<SpriterObject>();
            copy.FillFrom(obj);
            return copy;
        }
    }
}