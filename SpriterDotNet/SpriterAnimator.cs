// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriterDotNet
{
    public abstract class SpriterAnimator<T>
    {
        public event Action<string> AnimationFinished = s => { };
        public event Action<string> EventTriggered = s => { };
        public event Action<string, SpriterVarValue> VariableChanged = (n, v) => { };

        public Spriter Spriter { get; private set; }
        public SpriterEntity Entity { get; private set; }
        public SpriterAnimation CurrentAnimation { get; private set; }
        public SpriterAnimation NextAnimation { get; private set; }

        public string Name { get; private set; }
        public float Speed { get; set; }
        public float Length { get; set; }
        public float Time { get; set; }
        public float Progress
        {
            get { return Time / Length; }
            set { Time = value * Length; }
        }

        protected FrameData LastFrameData;

        private readonly IDictionary<string, SpriterAnimation> animations;
        private readonly IDictionary<int, IDictionary<int, T>> sprites = new Dictionary<int, IDictionary<int, T>>();
        private float totalTransitionTime;
        private float transitionTime;
        private float factor;

        public SpriterAnimator(SpriterEntity entity, Spriter spriter)
        {
            Entity = entity;
            Spriter = spriter;
            animations = entity.Animations.ToDictionary(a => a.Name, a => a);
            Speed = 1.0f;
            Play(animations.Keys.First());
            LastFrameData = new FrameData();
        }

        public IEnumerable<string> GetAnimations()
        {
            return animations.Keys;
        }

        public void Register(int folderId, int fileId, T obj)
        {
            IDictionary<int, T> objectsByFiles;
            sprites.TryGetValue(folderId, out objectsByFiles);

            if (objectsByFiles == null)
            {
                objectsByFiles = new Dictionary<int, T>();
                sprites[folderId] = objectsByFiles;
            }

            objectsByFiles[fileId] = obj;
        }

        public virtual void Play(string name)
        {
            SpriterAnimation animation = animations[name];
            Play(animation);
        }

        public virtual void Play(SpriterAnimation animation)
        {
            Progress = 0;

            CurrentAnimation = animation;
            Name = animation.Name;

            NextAnimation = null;
            Length = CurrentAnimation.Length;
        }

        public virtual void Transition(string name, float totalTransitionTime)
        {
            this.totalTransitionTime = totalTransitionTime;
            transitionTime = 0;
            NextAnimation = animations[name];
        }

        public virtual void Blend(string first, string second, float factor)
        {
            Play(first);
            NextAnimation = animations[second];
            totalTransitionTime = 0;
            this.factor = factor;
        }

        public virtual void Step(float deltaTime)
        {
            float elapsed = deltaTime * Speed;

            if (NextAnimation != null && totalTransitionTime != 0.0f)
            {
                elapsed += elapsed * factor * CurrentAnimation.Length / NextAnimation.Length;

                transitionTime += Math.Abs(elapsed);
                factor = transitionTime / totalTransitionTime;
                if (transitionTime >= totalTransitionTime)
                {
                    float progress = Progress;
                    Play(NextAnimation.Name);
                    Progress = progress;
                    NextAnimation = null;
                }
            }

            Time += elapsed;

            if (Time < 0.0f)
            {
                if (CurrentAnimation.Looping) Time += Length;
                else Time = 0.0f;
                AnimationFinished(Name);
            }
            else if (Time >= Length)
            {
                if (CurrentAnimation.Looping) Time -= Length;
                else Time = Length;
                AnimationFinished(Name);
            }

            Animate();
        }

        public ICollection<string> GetObjectNames()
        {
            return LastFrameData.ObjectVars.Keys;
        }

        public ICollection<string> GetObjectVarNames(string name)
        {
            return LastFrameData.ObjectVars[name].Keys;
        }

        public ICollection<string> GetVarNames()
        {
            return LastFrameData.AnimationVars.Keys;
        }

        public SpriterVarValue GetVarValue(string name)
        {
            return LastFrameData.AnimationVars[name];
        }

        public SpriterVarValue GetObjectVarValue(string objectName, string varName)
        {
            return LastFrameData.ObjectVars[objectName][varName];
        }

        protected virtual void Animate()
        {
            FrameData frameData;
            if (NextAnimation == null) frameData = SpriterProcessor.GetFrameData(CurrentAnimation, Time);
            else frameData = SpriterProcessor.GetFrameData(CurrentAnimation, NextAnimation, Time, factor);

            foreach (SpriterObject info in frameData.SpriteData)
            {
                IDictionary<int, T> objectsByFiles;
                sprites.TryGetValue(info.FolderId, out objectsByFiles);
                if (objectsByFiles == null) continue;

                T obj;
                objectsByFiles.TryGetValue(info.FileId, out obj);
                if (EqualityComparer<T>.Default.Equals(obj, default(T))) continue;

                ApplySpriteTransform(obj, info);
            }

            foreach (SpriterObject info in frameData.PointData) ApplyPointTransform(info);
            foreach (var entry in frameData.BoxData) ApplyBoxTransform(Entity.ObjectInfos[entry.Key], entry.Value);
            foreach (var entry in frameData.AnimationVars) ApplyVariableValue(entry.Key, entry.Value);

            LastFrameData = frameData;
        }

        protected virtual void ApplySpriteTransform(T obj, SpriterObject info)
        {
        }

        protected virtual void ApplyPointTransform(SpriterObject info)
        {
        }

        protected virtual void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
        }

        protected virtual void ApplyVariableValue(string name, SpriterVarValue value)
        {
            SpriterVarValue lastValue;
            LastFrameData.AnimationVars.TryGetValue(name, out lastValue);
            bool changed = lastValue.StringValue != value.StringValue
                            || lastValue.FloatValue != value.FloatValue
                            || lastValue.IntValue != value.IntValue;

            if (changed) VariableChanged(name, value);
        }
    }
}