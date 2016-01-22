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
        /// The current character map. If set to null, the default is used.
        /// </summary>
        public SpriterCharacterMap CharacterMap { get { return charMaps.Count > 0 ? charMaps.Peek() : null; } }

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
        /// Contains all the frame metadata. Updated on every call to Step.
        /// </summary>
        public FrameMetadata Metadata { get; private set; }

        /// <summary>
        /// The provider of the animation data.
        /// </summary>
        public IAnimationDataProvider DataProvider { get; set; }

        private readonly Dictionary<string, SpriterAnimation> animations;
        private readonly Dictionary<int, Dictionary<int, TSprite>> sprites = new Dictionary<int, Dictionary<int, TSprite>>();
        private readonly Dictionary<TSprite, TSprite> swappedSprites = new Dictionary<TSprite, TSprite>();
        private readonly Dictionary<TSprite, KeyValuePair<int, int>> charMapValues = new Dictionary<TSprite, KeyValuePair<int, int>>();
        private readonly Stack<SpriterCharacterMap> charMaps = new Stack<SpriterCharacterMap>();
        private readonly Dictionary<int, Dictionary<int, TSound>> sounds = new Dictionary<int, Dictionary<int, TSound>>();
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
            Metadata = new FrameMetadata();
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
        /// Register the sprite for the given folderId and fileId.
        /// </summary>
        public void Register(int folderId, int fileId, TSprite obj)
        {
            AddToDict(folderId, fileId, obj, sprites);
        }

        /// <summary>
        /// Register the sound for the given folderId and fileId.
        /// </summary>
        public void Register(int folderId, int fileId, TSound obj)
        {
            AddToDict(folderId, fileId, obj, sounds);
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

        /// <summary>
        /// Gets the transform information for all object types and calls the relevant apply method for each one.
        /// </summary>
        protected virtual void Animate(float deltaTime)
        {
            FrameData frameData = DataProvider.GetFrameData(Time, deltaTime, factor, CurrentAnimation, NextAnimation);
            if (SpriterConfig.MetadataEnabled) Metadata = DataProvider.GetFrameMetadata(Time, deltaTime, factor, CurrentAnimation, NextAnimation);

            for (int i = 0; i < frameData.SpriteData.Count; ++i)
            {
                SpriterObject info = frameData.SpriteData[i];
                TSprite obj = GetSprite(info);
                if (obj == null) continue;
                obj = GetSwappedSprite(obj);
                ApplySpriteTransform(obj, info);
            }

            if (SpriterConfig.MetadataEnabled)
            {
                for (int i = 0; i < Metadata.Sounds.Count; ++i)
                {
                    SpriterSound info = Metadata.Sounds[i];
                    TSound sound = GetFromDict(info.FolderId, info.FileId, sounds);
                    PlaySound(sound, info);
                }

                var pointE = frameData.PointData.GetEnumerator();
                while (pointE.MoveNext())
                {
                    var e = pointE.Current;
                    ApplyPointTransform(e.Key, e.Value);
                }

                var boxE = frameData.BoxData.GetEnumerator();
                while (boxE.MoveNext())
                {
                    var e = boxE.Current;
                    ApplyBoxTransform(Entity.ObjectInfos[e.Key], e.Value);
                }

                for (int i = 0; i < Metadata.Events.Count; ++i)
                {
                    DispatchEvent(Metadata.Events[i]);
                }
            }
        }

        /// <summary>
        /// Gets the folderId and fileId for the given SpriterObject based on the current character map or default
        /// </summary>
        protected virtual TSprite GetSprite(SpriterObject obj)
        {
            TSprite sprite = GetFromDict(obj.FolderId, obj.FileId, sprites);

            if (!charMapValues.ContainsKey(sprite)) return sprite;

            KeyValuePair<int, int> mapping = charMapValues[sprite];
            return GetFromDict(mapping.Key, mapping.Value, sprites);
        }

        /// <summary>
        /// Remove a manually swapped sprite by name
        /// </summary>
        public virtual void UnswapSprite(TSprite original)
        {
            if (swappedSprites.ContainsKey(original)) swappedSprites.Remove(original);
        }

        /// <summary>
        /// Swap one sprite for another, pass the name of the spriter piece you'd like to target, and a Sprite instance to replace it with.
        /// </summary>
        public virtual void SwapSprite(TSprite original, TSprite replacement)
        {
            swappedSprites[original] = replacement;
        }

        public virtual void PushCharMap(SpriterCharacterMap charMap)
        {
            ApplyCharMap(charMap);
            charMaps.Push(charMap);
        }

        public virtual void PopCharMap()
        {
            if (charMaps.Count == 0) return;
            charMaps.Pop();
            ApplyCharMap(charMaps.Count > 0 ? charMaps.Peek() : null);
        }

        protected virtual void ApplyCharMap(SpriterCharacterMap charMap)
        {
            if(charMap == null)
            {
                charMapValues.Clear();
                return;
            }

            for (int i = 0; i < charMap.Maps.Length; ++i)
            {
                SpriterMapInstruction map = charMap.Maps[i];
                TSprite sprite = GetFromDict(map.FolderId, map.FileId, sprites);
                if (sprite == null) continue;

                charMapValues[sprite] = new KeyValuePair<int, int>(map.TargetFolderId, map.TargetFileId);
            }
        }

        /// <summary>
        /// Internal function to lookup swapped sprites.
        /// </summary>
        private TSprite GetSwappedSprite(TSprite original)
        {
            if (swappedSprites.ContainsKey(original)) return swappedSprites[original];
            return original;
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

        private static void AddToDict<T>(int folderId, int fileId, T obj, Dictionary<int, Dictionary<int, T>> dict)
        {
            Dictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);

            if (objectsByFiles == null)
            {
                objectsByFiles = new Dictionary<int, T>();
                dict[folderId] = objectsByFiles;
            }

            objectsByFiles[fileId] = obj;
        }

        private static T GetFromDict<T>(int folderId, int fileId, Dictionary<int, Dictionary<int, T>> dict)
        {
            Dictionary<int, T> objectsByFiles;
            dict.TryGetValue(folderId, out objectsByFiles);
            if (objectsByFiles == null) return default(T);

            T obj;
            objectsByFiles.TryGetValue(fileId, out obj);

            return obj;
        }
    }
}