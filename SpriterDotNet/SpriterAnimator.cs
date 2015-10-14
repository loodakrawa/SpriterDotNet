// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriterDotNet
{
    public abstract class SpriterAnimator<TSprite, TSound>
    {
        public event Action<string> AnimationFinished = s => { };
        public event Action<string> EventTriggered = s => { };

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

        protected FrameMetadata LastMetadata;

        private readonly IDictionary<string, SpriterAnimation> animations;
        private readonly IDictionary<int, IDictionary<int, TSprite>> sprites = new Dictionary<int, IDictionary<int, TSprite>>();
        private readonly IDictionary<int, IDictionary<int, TSound>> sounds = new Dictionary<int, IDictionary<int, TSound>>();
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
            LastMetadata = new FrameMetadata();
        }

        public IEnumerable<string> GetAnimations()
        {
            return animations.Keys;
        }

        public void Register(int folderId, int fileId, TSprite obj)
        {
            AddToDict(folderId, fileId, obj, sprites);
        }

        public void Register(int folderId, int fileId, TSound obj)
        {
            AddToDict(folderId, fileId, obj, sounds);
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

            Animate(elapsed);
        }

        public ICollection<string> GetObjectNames()
        {
            return LastMetadata.ObjectVars.Keys;
        }

        public ICollection<string> GetObjectVarNames(string name)
        {
            return LastMetadata.ObjectVars[name].Keys;
        }

        public ICollection<string> GetVarNames()
        {
            return LastMetadata.AnimationVars.Keys;
        }

        public SpriterVarValue GetVarValue(string name)
        {
            return LastMetadata.AnimationVars[name];
        }

        public SpriterVarValue GetObjectVarValue(string objectName, string varName)
        {
            return LastMetadata.ObjectVars[objectName][varName];
        }

        public ICollection<string> GetEventNames(string name)
        {
            return CurrentAnimation.Eventlines.Select(e => e.Name).ToList();
        }

        protected virtual void Animate(float deltaTime)
        {
            FrameData frameData;
            FrameMetadata metaData;
            if (NextAnimation == null)
            {
                frameData = SpriterProcessor.GetFrameData(CurrentAnimation, Time);
                metaData = SpriterProcessor.GetFrameMetadata(CurrentAnimation, Time, deltaTime);
            }
            else
            {
                frameData = SpriterProcessor.GetFrameData(CurrentAnimation, NextAnimation, Time, factor);
                metaData = SpriterProcessor.GetFrameMetadata(CurrentAnimation, NextAnimation, Time, deltaTime, factor);
            }

            foreach (SpriterObject info in frameData.SpriteData)
            {
                TSprite obj = GetFromDict(info.FolderId, info.FileId, sprites);
                ApplySpriteTransform(obj, info);
            }

            foreach (SpriterSound info in metaData.Sounds)
            {
                TSound sound = GetFromDict(info.FolderId, info.FileId, sounds);
                PlaySound(sound, info);
            }

            foreach (SpriterObject info in frameData.PointData) ApplyPointTransform(info);
            foreach (var entry in frameData.BoxData) ApplyBoxTransform(Entity.ObjectInfos[entry.Key], entry.Value);
            foreach (string eventName in metaData.Events) DispatchEvent(eventName);

            LastMetadata = metaData;
        }

        protected virtual void ApplySpriteTransform(TSprite sprite, SpriterObject info)
        {
        }

        protected virtual void PlaySound(TSound sound, SpriterSound info)
        {
        }

        protected virtual void ApplyPointTransform(SpriterObject info)
        {
        }

        protected virtual void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
        }

        protected virtual void DispatchEvent(string eventName)
        {
            EventTriggered(eventName);
        }

        private static void AddToDict<T>(int folderId, int fileId, T obj, IDictionary<int, IDictionary<int, T>> dict)
        {
            IDictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);

            if (objectsByFiles == null)
            {
                objectsByFiles = new Dictionary<int, T>();
                dict[folderId] = objectsByFiles;
            }

            objectsByFiles[fileId] = obj;
        }

        private static T GetFromDict<T>(int folderId, int fileId, IDictionary<int, IDictionary<int, T>> dict)
        {
            IDictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);
            if (objectsByFiles == null) return default(T);

            T obj;
            objectsByFiles.TryGetValue(fileId, out obj);

            return obj;
        }
    }
}