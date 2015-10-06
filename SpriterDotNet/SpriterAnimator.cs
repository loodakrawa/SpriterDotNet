// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriterDotNet
{
    public abstract class SpriterAnimator<T>
    {
        private readonly IDictionary<string, SpriterAnimation> animations;
        private readonly IDictionary<int, IDictionary<int, T>> objects = new Dictionary<int, IDictionary<int, T>>();

        public SpriterEntity Entity { get; private set; }
        public SpriterAnimation CurrentAnimation { get; private set; }
        public SpriterAnimation NextAnimation { get; private set; }

        public string Name { get; private set; }
        public float Speed { get; set; }
        public float Length { get; set; }
        public float Time { get; set; }

        private float totalTransitionTime;
        private float transitionTime;
        private float factor;

        public float Progress
        {
            get { return Time / Length; }
            set { Time = value * Length; }
        }

        public SpriterAnimator(SpriterEntity entity)
        {
            Entity = entity;
            animations = entity.Animations.ToDictionary(a => a.Name, a => a);
            Speed = 1.0f;
            Play(animations.Keys.First());
        }

        public IEnumerable<string> GetAnimations()
        {
            return animations.Keys;
        }

        public void Register(int folderId, int fileId, T obj)
        {
            IDictionary<int, T> objectsByFiles;
            objects.TryGetValue(folderId, out objectsByFiles);

            if (objectsByFiles == null)
            {
                objectsByFiles = new Dictionary<int, T>();
                objects[folderId] = objectsByFiles;
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

        public virtual void Transition(string name, float transitionTime)
        {
            totalTransitionTime = transitionTime;
            this.transitionTime = 0;
            NextAnimation = animations[name];
        }

        public virtual void Blend(string first, string second, float factor)
        {
            Play(first);
            NextAnimation = animations[second];
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

        protected virtual void Animate()
        {
            SpriterObjectInfo[] drawData;
            if (NextAnimation == null) drawData = SpriterProcessor.GetDrawData(CurrentAnimation, Time);
            else drawData = SpriterProcessor.GetDrawData(CurrentAnimation, NextAnimation, Time, factor);

            foreach (SpriterObjectInfo info in drawData)
            {
                IDictionary<int, T> objectsByFiles;
                objects.TryGetValue(info.FolderId, out objectsByFiles);
                if (objectsByFiles == null) continue;

                T obj;
                objectsByFiles.TryGetValue(info.FileId, out obj);
                if (EqualityComparer<T>.Default.Equals(obj, default(T))) continue;

                ApplyTransform(obj, info);
            }
        }

        protected virtual void ApplyTransform(T obj, SpriterObjectInfo info)
        {
        }

        protected virtual void AnimationFinished(string name)
        {
        }
    }
}