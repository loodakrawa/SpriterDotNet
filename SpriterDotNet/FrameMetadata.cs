// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
    public class FrameMetadata
    {
        public IDictionary<string, SpriterVarValue> AnimationVars { get; private set; }
        public IDictionary<string, IDictionary<string, SpriterVarValue>> ObjectVars { get; private set; }
        public IList<string> AnimationTags { get; private set; }
        public IDictionary<string, IList<string>> ObjectTags { get; private set; }
        public IList<string> Events { get; private set; }
        public IList<SpriterSound> Sounds { get; private set; }

        public FrameMetadata()
        {
            AnimationVars = new Dictionary<string, SpriterVarValue>();
            ObjectVars = new Dictionary<string, IDictionary<string, SpriterVarValue>>();
            AnimationTags = new List<string>();
            ObjectTags = new Dictionary<string, IList<string>>();
            Events = new List<string>();
            Sounds = new List<SpriterSound>();
        }

        public void AddObjectVar(string objectName, string varName, SpriterVarValue value)
        {
            IDictionary<string, SpriterVarValue> values;
            if (!ObjectVars.TryGetValue(objectName, out values))
            {
                values = new Dictionary<string, SpriterVarValue>();
                ObjectVars[objectName] = values;
            }
            values[varName] = value;
        }

        public void AddObjectTag(string objectName, string tag)
        {
            IList<string> tags;
            if(!ObjectTags.TryGetValue(objectName, out tags))
            {
                tags = new List<string>();
                ObjectTags[objectName] = tags;
            }
            tags.Add(tag);
        }
    }
}
