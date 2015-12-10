
namespace SpriterDotNet
{
    public static class SpriterConfig
    {
        /// <summary>
        /// Disables ALL metadata calculations.
        /// </summary>
        public static bool MetadataEnabled { get; set; } = true;

        public static bool VarsEnabled { get; set; } = true;
        public static bool TagsEnabled { get; set; } = true;
        public static bool EventsEnabled { get; set; } = true;
        public static bool SoundsEnabled { get; set; } = true;
    }
}
