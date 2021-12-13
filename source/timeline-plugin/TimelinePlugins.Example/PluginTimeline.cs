using System;
using Finkit.ManicTime.Plugins.Timelines;

namespace TimelinePlugins.Example
{
    public class PluginTimeline : Timeline
    {
        public PluginTimeline(TimelineType timelineType, string typeName, string genericTypeName,
            Func<Timeline, string> getDefaultDisplayName)
            : base(timelineType, typeName, genericTypeName, getDefaultDisplayName)
        {
            // Create symbolic directory link so that we don't have to copy the build output to the ManicTime db directory
            // as per the instructions here: https://github.com/manictime/manictime-client-plugin-example
            //
            // C:\Users\jon\AppData\Local\Finkit\ManicTime\Plugins\Packages>mklink /D TimelinePlugins.ForSvrApps C:\tmp\GitHub\Atadar\TTTransposeServerTimeline\installable-plugin\Debug\Plugins\Packages\TimelinePlugins.ForSvrApps



        }
    }
}
