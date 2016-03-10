// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.AnimationDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriterDotNet
{
    public abstract class SpriterAnimator<TSprite, TSound>
    {
        /// <summary>
        /// Occurs when the animation finishes playing or loops.
        /// </summary>
        public event Action<string> AnimationFinished = s => { };

        /// <summary>
        /// Occurs when an animation events gets triggered.
        /// </summary>
        public event Action<string> EventTriggered = s => { };

        /// <summary>
        /// The animated Entity.
        /// </summary>
        public SpriterEntity Entity { get; private set; }

        /// <summary>
        /// The current animation.
        /// </summary>
        public SpriterAnimation CurrentAnimation { get; private set; }

        /// <summary>
        /// The animation transitioned to or blended with the current animation.
        /// </summary>
        public SpriterAnimation NextAnimation { get; private set; }

        /// <summary>
        /// The name of the current animation.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Playback speed. Defaults to 1.0f. Negative values reverse the animation.<para />
        /// For example:<para />
        /// 0.5f corresponds to 50% of the default speed<para />
        /// 2.0f corresponds to 200% of the default speed<para />
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// The legth of the current animation in milliseconds.
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// The current time in milliseconds.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Allow external class to check if an animation exists for a given name.
        /// </summary>
        public bool HasAnimation(string name) { return animations.ContainsKey(name); }

        /// <summary>
        /// The current progress. Ranges from 0.0f - 1.0f.
        /// </summary>
        public float Progress
        {
            get { return Time / Length; }
            set { Time = value * Length; }
        }

        /// <summary>
        /// The provider of the animation data.
        /// </summary>
        public IAnimationDataProvider DataProvider { get; set; }

        public IAssetProvider<TSprite> SpriteProvider { get; set; }
        public IAssetProvider<TSound> SoundProvider { get; set; }

        public FrameData FrameData { get; set; }

        private readonly Dictionary<string, SpriterAnimation> animations;

        private float totalTransitionTime;
        private float transitionTime;
        private float factor;

        /// <summary>
        /// Sole constructor. Creates a new instance which animates the given entity.
        /// </summary>
        protected SpriterAnimator(SpriterEntity entity)
        {
            Entity = entity;
            animations = entity.Animations.ToDictionary(a => a.Name, a => a);
            Speed = 1.0f;
            DataProvider = new DefaultAnimationDataProvider();
        }

        /// <summary>
        /// Returns a list of all the animations for the entity
        /// </summary>
        public IEnumerable<string> GetAnimations()
        {
            return animations.Keys;
        }

       

        /// <summary>
        /// Plays the animation with the given name. Playback starts from the beginning.
        /// </summary>
        public virtual void Play(string name)
        {
            SpriterAnimation animation = animations[name];
            Play(animation);
        }

        /// <summary>
        /// Plays the given animation. Playback starts from the beginning.
        /// </summary>
        public virtual void Play(SpriterAnimation animation)
        {
            Progress = 0;

            CurrentAnimation = animation;
            Name = animation.Name;

            NextAnimation = null;
            Length = CurrentAnimation.Length;
        }

        /// <summary>
        /// Transitions to given animation doing a progressive blend in the given time.
        /// <remarks>Animation blending works only for animations with identical hierarchies.</remarks>
        /// </summary>
        public virtual void Transition(string name, float totalTransitionTime)
        {
            this.totalTransitionTime = totalTransitionTime;
            transitionTime = 0.0f;
            factor = 0.0f;
            NextAnimation = animations[name];
        }

        /// <summary>
        /// Blends two animations with the given weight factor. Factor ranges from 0.0f - 1.0f.<para />
        /// Animation blending works only for animations with identical hierarchies.<para />
        /// For example:<para />
        /// factor == 0.0f corresponds to 100% of the first animation and 0% of the second<para />
        /// factor == 0.25f corresponds to 75% of the first animation and 25% of the second<para />
        /// factor == 0.5f corresponds to 50% of each animation<para />
        /// </summary>
        public virtual void Blend(string first, string second, float factor)
        {
            Play(first);
            NextAnimation = animations[second];
            totalTransitionTime = 0;
            this.factor = factor;
        }

        /// <summary>
        /// Advances the animation for the deltaTime increment.
        /// </summary>
        public virtual void Step(float deltaTime)
        {
            if (CurrentAnimation == null) Play(animations.Keys.First());

            float initialTime = Time;
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
                if(Time != initialTime) AnimationFinished(Name);
            }
            else if (Time >= Length)
            {
                if (CurrentAnimation.Looping) Time -= Length;
                else Time = Length;
                if (Time != initialTime) AnimationFinished(Name);
            }

            Animate(elapsed);

        }
        /// <summary>
        /// Gets the transform information for all object types and calls the relevant apply method for each one.
        /// </summary>
        protected virtual void Animate(float deltaTime)
        {
            FrameData = DataProvider.GetFrameData(Time, deltaTime, factor, CurrentAnimation, NextAnimation);

            for (int i = 0; i < FrameData.SpriteData.Count; ++i)
            {
                SpriterObject info = FrameData.SpriteData[i];
                TSprite sprite = SpriteProvider.Get(info.FolderId, info.FileId);
                if(sprite != null) ApplySpriteTransform(sprite, info);
            }

            if (SpriterConfig.MetadataEnabled)
            {
                for (int i = 0; i < FrameData.Sounds.Count; ++i)
                {
                    SpriterSound info = FrameData.Sounds[i];
                    TSound sound = SoundProvider.Get(info.FolderId, info.FileId);
                    if(sound != null) PlaySound(sound, info);
                }

                var pointE = FrameData.PointData.GetEnumerator();
                while (pointE.MoveNext())
                {
                    var e = pointE.Current;
                    ApplyPointTransform(e.Key, e.Value);
                }

                var boxE = FrameData.BoxData.GetEnumerator();
                while (boxE.MoveNext())
                {
                    var e = boxE.Current;
                    ApplyBoxTransform(Entity.ObjectInfos[e.Key], e.Value);
                }

                for (int i = 0; i < FrameData.Events.Count; ++i)
                {
                    DispatchEvent(FrameData.Events[i]);
                }
            }
        }

       

        /// <summary>
        /// Applies the transform to the concrete sprite isntance.
        /// </summary>
        protected virtual void ApplySpriteTransform(TSprite sprite, SpriterObject info)
        {
        }

        /// <summary>
        /// Plays the concrete sound isntance.
        /// </summary>
        protected virtual void PlaySound(TSound sound, SpriterSound info)
        {
        }

        /// <summary>
        /// Applies the transforms for the point with the given name.
        /// </summary>
        protected virtual void ApplyPointTransform(string name, SpriterObject info)
        {
        }

        /// <summary>
        /// Applies the transform for the given box.
        /// </summary>
        protected virtual void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
        }

        /// <summary>
        /// Dispatches event when triggered in animation.
        /// </summary>
        protected virtual void DispatchEvent(string eventName)
        {
            EventTriggered(eventName);
        }

      

        
    }
}