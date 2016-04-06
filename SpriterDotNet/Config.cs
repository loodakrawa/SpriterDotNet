// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    public class Config
    {
        /// <summary>
        /// Enables ALL metadata calculations.
        /// </summary>
        public bool MetadataEnabled { get; set; }

        public bool VarsEnabled { get; set; }
        public bool TagsEnabled { get; set; } 
        public bool EventsEnabled { get; set; } 
        public bool SoundsEnabled { get; set; }

        /// <summary>
        /// Enables object pooling
        /// </summary>
        public bool PoolingEnabled { get; set; }

        public Config() 
        {
            MetadataEnabled = true;
            VarsEnabled = true;
            TagsEnabled = true;
            EventsEnabled = true;
            SoundsEnabled = true;
            PoolingEnabled = true;
        }
    }
}
