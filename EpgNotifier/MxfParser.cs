using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EpgNotifier
{
    public class MxfParser
    {
        private XmlDocument mxfDoc;

        public MxfParser(XmlDocument doc)
        {
            mxfDoc = doc;
        }

        public List<TvProgram> GetDesiredPrograms(List<string> shows)
        {
            var programs = mxfDoc.SelectNodes("/MXF/With/Programs/Program");
            var desiredPrograms = new List<XmlNode>();
            for (int i = 0; i < programs.Count; i++)
            {
                var program = programs[i];
                if (shows.Any(s => string.Equals(s, program.Attributes["title"].Value, StringComparison.InvariantCultureIgnoreCase)))
                    desiredPrograms.Add(program);
            }

            var tvPrograms = desiredPrograms.Select(dp => new TvProgram
            {
                Id = dp.Attributes["id"].Value,
                Title = dp.Attributes["title"].Value,
                EpisodeTitle = dp.Attributes["episodeTitle"].Value,
                ShortDescription = dp.Attributes["shortDescription"].Value,
                Description = dp.Attributes["description"].Value,
                Year = dp.Attributes["year"] != null ? Convert.ToInt32(dp.Attributes["year"].Value) : -1,
                OriginalAirDate = dp.Attributes["originalAirdate"] != null ? DateTime.Parse(dp.Attributes["originalAirdate"].Value) : DateTime.MinValue,
                SeasonNumber = dp.Attributes["seasonNumber"] != null ? Convert.ToInt32(dp.Attributes["seasonNumber"].Value) : -1,
                EpisodeNumber = dp.Attributes["episodeNumber"] != null ? Convert.ToInt32(dp.Attributes["episodeNumber"].Value) : -1,
            });

            return tvPrograms.ToList();
        }

        public int GetSeasonNumberFromDescription(string description)
        {
            if (description == null)
                return -1;

            var index = description.IndexOf("\n\nSeason ");
            if (index == -1)
                return -1;

            var substring = description.Substring(index);  // "Season N, Episode N"
            var seasonNumber = substring.Substring(9, substring.IndexOf(",") - 9).Trim();
            return Convert.ToInt32(seasonNumber);
        }

        public  int GetEpisodeNumberFromDescription(string description)
        {
            if (description == null)
                return -1;

            var index = description.IndexOf("\n\nSeason ");
            if (index == -1)
                return -1;

            var substring = description.Substring(index);  // "Season N, Episode N"
            var episodeNumber = substring.Substring(substring.IndexOf(",") + 10).Trim();
            return Convert.ToInt32(episodeNumber);
        }

        public Dictionary<string, string> GetChannelListing()
        {
            var channels = mxfDoc.SelectNodes("/MXF/With/Lineups/Lineup/channels/Channel");
            var channelList = new Dictionary<string, string>();
            for (int i = 0; i < channels.Count; i++)
            {
                if (Convert.ToInt32(channels[i].Attributes["number"].Value) >= 500)
                    channelList.Add(channels[i].Attributes["service"].Value, channels[i].Attributes["number"].Value);
            }
            return channelList;
        }

        public List<ChannelSchedule> GetSchedulesOfPrograms(IEnumerable<TvProgram> desiredPrograms)
        {
            var channelSchedules = mxfDoc.SelectNodes("/MXF/With/ScheduleEntries");

            var programIds = desiredPrograms.Select(p => p.Id);
            var desiredSchedules = new List<XmlNode>();
            for (int i = 0; i < channelSchedules.Count; i++)
            {
                var channelSchedule = channelSchedules[i];
                for (int t = 0; t < channelSchedule.ChildNodes.Count; t++)
                {
                    var scheduleItem = channelSchedule.ChildNodes[t];
                    if (programIds.Any(p => string.Equals(p, scheduleItem.Attributes["program"].Value, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        desiredSchedules.Add(channelSchedule);
                        break;
                    }
                }
            }

            var channelDict = GetChannelListing();

            var finalSchedules = new List<ChannelSchedule>();
            foreach (var channelSchedule in desiredSchedules)
            {
                var serviceId = channelSchedule.Attributes["service"].Value;

                finalSchedules.Add(BuildChannelSchedule(serviceId, channelDict[serviceId], channelSchedule.ChildNodes, desiredPrograms));
            }

            return finalSchedules;
        }

        public static ChannelSchedule BuildChannelSchedule(string serviceId, string channelNumber, XmlNodeList scheduleEntries, IEnumerable<TvProgram> desiredShows)
        {
            var channelSchedule = new ChannelSchedule { ServiceId = serviceId, ChannelNumber = channelNumber, Schedule = new Dictionary<DateTime, TvProgram>() };
            //startTime="2017-08-17T02:12:00"  UTC
            var currentTime = DateTime.Parse(scheduleEntries[0].Attributes["startTime"].Value);
            for (int i = 0; i < scheduleEntries.Count; i++)
            {
                var programId = scheduleEntries[i].Attributes["program"].Value;
                var currentShow = desiredShows.FirstOrDefault(s => s.Id == programId);
                if (currentShow != null)
                {
                    channelSchedule.Schedule.Add(new DateTime(currentTime.Ticks, DateTimeKind.Utc), currentShow);
                }

                currentTime = currentTime.AddSeconds(Convert.ToInt32(scheduleEntries[i].Attributes["duration"].Value));
            }

            return channelSchedule;
        }
    }
}
