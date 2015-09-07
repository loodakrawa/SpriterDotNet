/**
The MIT License (MIT)

Copyright (c) 2015 Luka "loodakrawa" Sverko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

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

        public string Name { get; private set; }
        public float Speed { get; set; }
        public float Length { get; set; }
        public float Time { get; set; }

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
            Progress = 0;

            Name = name;
            CurrentAnimation = animations[name];
            Length = CurrentAnimation.Length;
        }

        public virtual void Step(float deltaTime)
        {
            float elapsed = deltaTime * Speed;

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
            SpriterObjectInfo[] drawData = SpriterProcessor.GetDrawData(CurrentAnimation, Time);

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