using System;
using Finkit.ManicTime.Plugins.Timelines;

namespace TimelinePlugins.TransposeSvrTimeline
{
    public class PluginTimeline : Timeline
    {
        public PluginTimeline(TimelineType timelineType, string typeName, string genericTypeName,
            Func<Timeline, string> getDefaultDisplayName)
            : base(timelineType, typeName, genericTypeName, getDefaultDisplayName)
        {

        }
    }
}
