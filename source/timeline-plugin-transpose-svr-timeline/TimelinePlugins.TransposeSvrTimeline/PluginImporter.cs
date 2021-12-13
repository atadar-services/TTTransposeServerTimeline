using System;
using Finkit.ManicTime.Plugins.Activities;
using Finkit.ManicTime.Plugins.Groups;
using Finkit.ManicTime.Shared.Helpers;
using Finkit.ManicTime.Shared.Logging;
using System.Data.SQLite;
using System.Linq;
using System.Collections.Generic;

namespace TimelinePlugins.TransposeSvrTimeline
{
    public static class PluginImporter
    {
        public static Activity[] GetData(PluginTimeline timeline, Func<Group> createGroup,
            Func<Activity> createActivity, DateTime fromLocalTime, DateTime toLocalTime)
        {
            /*
                TO INSTALL:  Follow regular plugin install directions but create symbolic link directory with 
                             this command so you don't have to copy the build output to the db folder.
                C:\Users\jon\AppData\Local\Finkit\ManicTime\Plugins\Packages>mklink /D TimelinePlugins.TransposeSvrTimeline C:\tmp\GitHub\Atadar\TTTransposeServerTimeline\installable-plugin\Debug\Plugins\Packages\TimelinePlugins.TransposeSvrTimeline


                get activities from your source for time range fromLocalTime-toLocalTime
                then create Activity objects and return the collection.

                Here we create only one activity spanning the whole day.
            */



            try
            {
                //ApplicationLog.WriteInfo("current program's processor architecture = " + System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
                //ApplicationLog.WriteInfo("current program's .NET version = " + Environment.Version.ToString());

                // Based on the above, download and copy-paste the following files into the same folder as the output dll:
                // https://system.data.sqlite.org/downloads/1.0.115.5/sqlite-netFx35-static-binary-bundle-Win32-2008-1.0.115.5.zip

                /*
                string cs = "Data Source=:memory:";
                string stm = "SELECT SQLITE_VERSION()";

                using var con = new SQLiteConnection(cs);
                con.Open();

                using var cmd = new SQLiteCommand(stm, con);
                string version = cmd.ExecuteScalar().ToString();

                ApplicationLog.WriteInfo($"SQLite version: {version}");
                */

                List<Activity> activities = new List<Activity>();


                string serverTimeline = string.Empty;
                if (timeline.DisplayName == "SvrApps")
                {
                    serverTimeline = "ManicTime/Applications";
                }
                else if (timeline.DisplayName == "SvrDocs")
                {
                    serverTimeline = "ManicTime/Documents";
                }


                string dateFormat = "yyyy-dd-MM HH:mm:ss";
                string fromUTC = fromLocalTime.ToUniversalTime().ToString(dateFormat);
                string toUTC = toLocalTime.ToUniversalTime().ToString(dateFormat);

                // Logs see C:\Users\jon\AppData\Local\Finkit\ManicTime\Logs
                ApplicationLog.WriteInfo(
                    "timeline="+timeline.DisplayName+
                    ", retrieving data from server timeline "+serverTimeline+
                    ", fromUTC="+fromUTC+
                    ", toUTC="+toUTC
                );
                //ApplicationLog.WriteInfo("current dir = " + Directory.GetCurrentDirectory());

                // Must create a symbolic link because SqliteConnection won't open db in a different place
                // C:\Program Files (x86)\ManicTime>mklink ManicTimeServerReports.db C:\ProgramData\ManicTime\Server\Data\ManicTimeReports.db
                string cs = @"URI=file:C:\ProgramData\ManicTime\Server\Data\ManicTimeReports.db";

                using var con = new SQLiteConnection(cs);
                con.Open();

                string stm =
                    "SELECT activity.ActivityId, activity.Name, StartUtcTime, EndUtcTime, grp.GroupId, grp.Name" +
                    " FROM Ar_User usr" +
                    " INNER JOIN Ar_Timeline timeline ON usr.UserId = timeline.OwnerId" +
                    " INNER JOIN Ar_Activity activity ON timeline.ReportId = activity.ReportId" +
                    " INNER JOIN Ar_Group grp ON activity.GroupId = grp.GroupId AND timeline.ReportId = grp.ReportId" +
                    " WHERE usr.Username = 'jon@rds.gearscrm.com'" +
                    " AND timeline.SchemaName = '" + serverTimeline + "'" +
                    " AND activity.StartUtcTime >= '" + fromUTC + "'" +
                    " AND activity.EndUtcTime <= '" + toUTC + "'";

                //ApplicationLog.WriteInfo("sql = " + stm);
                using var cmd = new SQLiteCommand(stm, con);
                using SQLiteDataReader reader = cmd.ExecuteReader();

                string activityId;
                string activityName;
                DateTimeOffset startUtcTime;
                DateTimeOffset endUtcTime;
                string groupId;
                string groupName;

                while (reader.Read())
                {
                    activityId = reader.GetInt32(0).ToString();
                    activityName = reader.GetString(1);
                    startUtcTime = DateTime.Parse(reader.GetString(2));
                    endUtcTime = DateTime.Parse(reader.GetString(3));
                    groupId = reader.GetInt32(4).ToString();
                    groupName = reader.GetString(5);
                    /*
                    ApplicationLog.WriteInfo(
                        "Retrieved group ("+groupName+")," +
                        " activity ("+activityName+")," +
                        " startUtcTime ("+startUtcTime.ToString()+")," +
                        " endUtcTime ("+endUtcTime.ToString()+")"
                    );
                    */
                    activities.Add(CreateActivity(
                        timeline.DisplayName + "-" + activityId,
                        startUtcTime.Add(DateTime.Now.Subtract(DateTime.UtcNow)),
                        endUtcTime.Add(DateTime.Now.Subtract(DateTime.UtcNow)).AddMilliseconds(-1),
                        activityName,
                        CreateGroup(
                            timeline.DisplayName + "-" + groupId,
                            groupName,
                                hashStringToColor(groupName),
                                createGroup
                        ),
                        createActivity
                    ));

                }
                //ApplicationLog.WriteInfo("activities.Count="+activities.Count.ToString());
                return activities.ToArray();

/*
                string groupDisplayName = serverTimeline;
                string displayColor = hashStringToColor(groupDisplayName);

                var groupOne = CreateGroup("1-group", groupDisplayName, displayColor, createGroup);

                return new Activity[]
                {
                    CreateActivity("1-activity", fromLocalTime, toLocalTime, "Sample activity", groupOne, createActivity)
                };
*/

            }
            catch (Exception ex)
            {
                ApplicationLog.WriteError(ex);
                throw;
            }
        }

        private static Group CreateGroup(string id, string displayName, string color, Func<Group> createGroup)
        {
            var group = createGroup();
            group.Color = color.ToRgb();
            group.SourceId = id;
            group.DisplayName = displayName;
            return group;
        }

        private static Activity CreateActivity(string id, DateTimeOffset startTime, DateTimeOffset endTime,
            string displayName, Group group, Func<Activity> createActivity)
        {
            var activity = createActivity();
            activity.StartTime = startTime;
            activity.EndTime = endTime;
            activity.SourceId = id;
            activity.Group = group;
            activity.DisplayName = displayName;
            return activity;
        }

        private static string hashStringToColor(string displayName)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(displayName));
            var color = System.Drawing.Color.FromArgb(hash[0], hash[1], hash[2]);
            return System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(color.ToArgb()));
        }
    }
}
