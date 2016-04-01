
namespace SpriterDotNet
{
    public class SpriterConfig
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

        public SpriterConfig() 
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
