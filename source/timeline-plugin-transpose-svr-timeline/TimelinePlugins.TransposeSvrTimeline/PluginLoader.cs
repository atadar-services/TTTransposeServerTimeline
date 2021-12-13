using Finkit.ManicTime.Client.ActivityTimelines;
using Finkit.ManicTime.Shared.ObjectBuilding;
using Finkit.ManicTime.Plugins.Activities;
using Finkit.ManicTime.Plugins.Groups;
using Finkit.ManicTime.Plugins.Timelines;
using Finkit.ManicTime.Shared;
using Finkit.ManicTime.Shared.Ioc;
using Finkit.ManicTime.Shared.Plugins;
using Finkit.ManicTime.Shared.Plugins.ServiceProviders.Manager;
using TimelinePlugins.TransposeSvrTimeline.Wizard;

namespace TimelinePlugins.TransposeSvrTimeline
{
    //plugin definition
    [Plugin]
    public class PluginLoader
    {
        public const string SourceTypeName = "ManicTime/TransposeSvrTimelinePluginSource";
        public const string TimelineTypeName = "ManicTime/TransposeSvrTimelinePluginTimeline";
        private const string DefaultDisplayName = "Timeline plugin TransposeSvrTimeline";

        public PluginLoader(PluginContext pluginContext,
            ITimelineTypeRegistry timelineTypeRegistry, ISourceTypeRegistry sourceTypeRegistry,
            IObjectBuilder objectBuilder, IServiceLocator serviceLocator)
        {
            var timelineType = new TimelineType(pluginContext.Spec, TimelineTypeName, () => DefaultDisplayName)
                .WithAllowMultipleInstances(true);

            timelineTypeRegistry.RegisterTimeline(timelineType);

            timelineTypeRegistry.RegisterTimelineFactory(TimelineTypeName,
                () => new PluginTimeline(timelineType, TimelineTypeName, TimelineTypeNames.GenericGroup,
                    t => t.TimelineType.GetDefaultDisplayName()));
            
            timelineTypeRegistry
                .RegisterTimelineEntityFactory(TimelineTypeName, () => new SingleGroupActivity(null));

            timelineTypeRegistry.RegisterTimelineEntityFactory(TimelineTypeName, () => new Group(null));
            
            //register function for loading day view data
            sourceTypeRegistry.RegisterDayViewLoader(SourceTypeName, objectBuilder.Build<PluginDayViewLoader>());

            //register user inferface view
            sourceTypeRegistry.RegisterAddTimelineViewModelFactory(SourceTypeName, serviceLocator.GetInstance<AddPluginTimelineViewModel>);

            sourceTypeRegistry.RegisterSourceType(
                new SourceType(SourceTypeName, () => DefaultDisplayName, 6)
                .WithIcon32Path("pack://application:,,,/TimelinePlugins.TransposeSvrTimeline;component/Images/SourceImage.png")
                .WithGetDescription(() =>"This is ManicTime timeline plugin TransposeSvrTimeline"));
        }
    }
}
