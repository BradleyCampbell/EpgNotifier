using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace EpgNotifier
{
    // Link to explanation of mxf xml schema
    // https://msdn.microsoft.com/en-us/library/windows/desktop/dd776338.aspx?f=255&MSPPError=-2147217396#mxf_file_structure__ucad

    public class EpgNotifier
    {
        static string _showListFileName;
        static string _guideFileName;

        public static void Main(string[] args)
        {
            if (!AreArgumentsValid(args))
                return;

            var shows = File.ReadAllLines(_showListFileName).ToList();

            XmlDocument doc = new XmlDocument();
            doc.Load(_guideFileName);

            var desiredPrograms = GetDesiredPrograms(shows, doc);

            var channelDict = GetChannelListing(doc);

            var programIds = desiredPrograms.Select(p => p.Id);
            var desiredChannelSchedules = GetChannelSchedulesWithPrograms(programIds, doc);

            var finalSchedules = new List<ChannelSchedule>();
            foreach (var channelSchedule in desiredChannelSchedules)
            {
                var serviceId = channelSchedule.Attributes["service"].Value;
                
                finalSchedules.Add(BuildChannelSchedule(serviceId, channelDict[serviceId], channelSchedule.ChildNodes, desiredPrograms));
            }

            var sb = new StringBuilder();
            foreach (var schedule in finalSchedules)
            {
                sb.AppendLine(schedule.ToString());
            }
            var body = sb.ToString();
        }

        public static ChannelSchedule BuildChannelSchedule(string serviceId, string channelNumber, XmlNodeList scheduleEntries, List<TvProgram> desiredShows)
        {
            var channelSchedule = new ChannelSchedule { ServiceId = serviceId, ChannelNumber = channelNumber, Schedule = new Dictionary<DateTime, TvProgram>() };
            //startTime="2017-08-17T02:12:00"
            var currentTime = DateTime.Parse(scheduleEntries[0].Attributes["startTime"].Value);
            for (int i = 0; i < scheduleEntries.Count; i++)
            {
                var programId = scheduleEntries[i].Attributes["program"].Value;
                var currentShow = desiredShows.FirstOrDefault(s => s.Id == programId);
                if (currentShow != null)
                {
                    channelSchedule.Schedule.Add(new DateTime(currentTime.Ticks, DateTimeKind.Local), currentShow);
                }

                currentTime = currentTime.AddSeconds(Convert.ToInt32(scheduleEntries[i].Attributes["duration"].Value));
            }

            return channelSchedule;
        }

        private static bool AreArgumentsValid(string[] args)
        {
            if(args.Length < 4)
                return false;

            var arg = Array.IndexOf(args, "-ShowList");
            if (arg != -1)
            {
                if ((arg + 1) == args.Length)
                {
                    return false;
                }

                _showListFileName = args[arg + 1];
                args[arg] = null;
                args[arg + 1] = null;
            }

            arg = Array.IndexOf(args, "-Guide");
            if (arg != -1)
            {
                if ((arg + 1) == args.Length)
                {
                    return false;
                }

                _guideFileName = args[arg + 1];
                args[arg] = null;
                args[arg + 1] = null;
            }

            return true;
        }

        private static List<TvProgram> GetDesiredPrograms(List<string> shows, XmlDocument doc)
        {
            var programs = doc.SelectNodes("/MXF/With/Programs/Program");
            var desiredPrograms = new List<XmlNode>();
            for (int i = 0; i < programs.Count; i++)
            {
                var program = programs[i];
                if (shows.Any(s => string.Equals(s, program.Attributes["title"].Value, StringComparison.InvariantCultureIgnoreCase)))
                    desiredPrograms.Add(program);
            }

            var tvPrograms = desiredPrograms.Select(dp => new TvProgram {
                Id = dp.Attributes["id"].Value,
                Title = dp.Attributes["title"].Value,
                Season = GetSeasonNumberFromDescription(dp.Attributes["description"].Value),
                Episode = GetEpisodeNumberFromDescription(dp.Attributes["description"].Value),
            });

            return tvPrograms.ToList();
        }

        public static int GetSeasonNumberFromDescription(string description)
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

        public static int GetEpisodeNumberFromDescription(string description)
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

        private static Dictionary<string, string> GetChannelListing(XmlDocument doc)
        {
            var channels = doc.SelectNodes("/MXF/With/Lineups/Lineup/channels/Channel");
            var channelDictionary = new Dictionary<string, string>();
            for (int i = 0; i < channels.Count; i++)
            {
                if(Convert.ToInt32(channels[i].Attributes["number"].Value) >= 500)
                    channelDictionary.Add(channels[i].Attributes["service"].Value, channels[i].Attributes["number"].Value);
            }
            return channelDictionary;
        }

        private static List<XmlNode> GetChannelSchedulesWithPrograms(IEnumerable<string> programIds, XmlDocument doc)
        {
            var channelSchedules = doc.SelectNodes("/MXF/With/ScheduleEntries");

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
            return desiredSchedules;
        }

        private static void EmailNotifications()
        {
            SmtpClient client = new SmtpClient
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.google.com"
            };

            MailMessage mail = new MailMessage("bradleycampbell@gmail.com", "bradleycampbell@gmail.com")
            {
                Subject = "Upcoming Shows To Record",
                Body = ""
            };

            client.Send(mail);
        }
    }
}