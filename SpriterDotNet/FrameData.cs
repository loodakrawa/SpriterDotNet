// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
    public class FrameData
    {
        public List<SpriterObject> SpriteData { get; private set; }
        public Dictionary<string, SpriterObject> PointData { get; private set; }
        public Dictionary<int, SpriterObject> BoxData { get; private set; }

        public Dictionary<string, SpriterVarValue> AnimationVars { get; private set; }
        public Dictionary<string, Dictionary<string, SpriterVarValue>> ObjectVars { get; private set; }
        public List<string> AnimationTags { get; private set; }
        public Dictionary<string, List<string>> ObjectTags { get; private set; }
        public List<string> Events { get; private set; }
        public List<SpriterSound> Sounds { get; private set; }

        private readonly ObjectPool pool;

        public FrameData(ObjectPool pool)
        {
            this.pool = pool;

            SpriteData = new List<SpriterObject>();
            PointData = new Dictionary<string, SpriterObject>();
            BoxData = new Dictionary<int, SpriterObject>();

            AnimationVars = new Dictionary<string, SpriterVarValue>();
            ObjectVars = new Dictionary<string, Dictionary<string, SpriterVarValue>>();
            AnimationTags = new List<string>();
            ObjectTags = new Dictionary<string, List<string>>();
            Events = new List<string>();
            Sounds = new List<SpriterSound>();
        }

        public void Clear()
        {
            pool.ReturnChildren(SpriteData);
            pool.ReturnChildren(PointData);
            pool.ReturnChildren(BoxData);

            var varE = ObjectVars.GetEnumerator();
            while (varE.MoveNext())
            {
                pool.ReturnChildren(varE.Current.Value);
                pool.ReturnObject(varE.Current.Value);
            }
            ObjectVars.Clear();

            var tagE = ObjectTags.GetEnumerator();
            while (tagE.MoveNext())
            {
                var list = tagE.Current.Value;
                list.Clear();
                pool.ReturnObject(list);
            }
            ObjectTags.Clear();

            Sounds.Clear();
            AnimationVars.Clear();
            AnimationTags.Clear();
            Events.Clear();
        }

        public void AddObjectVar(string objectName, string varName, SpriterVarValue value)
        {
            Dictionary<string, SpriterVarValue> values;
            if (!ObjectVars.TryGetValue(objectName, out values))
            {
                values = pool.GetObject<Dictionary<string, SpriterVarValue>>();
                ObjectVars[objectName] = values;
            }
            values[varName] = value;
        }

        public void AddObjectTag(string objectName, string tag)
        {
            List<string> tags;
            if (!ObjectTags.TryGetValue(objectName, out tags))
            {
                tags = pool.GetObject<List<string>>();
                ObjectTags[objectName] = tags;
            }
            tags.Add(tag);
        }
    }
}
