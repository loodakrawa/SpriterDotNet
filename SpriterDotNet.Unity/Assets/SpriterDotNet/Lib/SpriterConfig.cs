
namespace SpriterDotNet
{
    public static class SpriterConfig
    {
        /// <summary>
        /// Enables ALL metadata calculations.
        /// </summary>
        public static bool MetadataEnabled { get; set; }

        public static bool VarsEnabled { get; set; }
        public static bool TagsEnabled { get; set; } 
        public static bool EventsEnabled { get; set; } 
        public static bool SoundsEnabled { get; set; }

        /// <summary>
        /// Enables object pooling
        /// </summary>
        public static bool PoolingEnabled { get; set; }

        static SpriterConfig() 
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
