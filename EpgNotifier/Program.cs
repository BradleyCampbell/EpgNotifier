using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            List<XmlNode> desiredPrograms = GetDesiredPrograms(shows, doc);

            var channelDict = GetChannelListing(doc);

            var programIds = desiredPrograms.Select(p => p.Attributes["id"].Value);
            GetChannelSchedulesForPrograms(programIds, doc);
            

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

        private static List<XmlNode> GetChannelSchedulesForPrograms(IEnumerable<string> programIds, XmlDocument doc)
        {
            var channelSchedules = doc.SelectNodes("/MXF/With/ScheduleEntries");
            
            var desiredSchedules = new List<XmlNode>();
            for (int i = 0; i < channelSchedules.Count; i++)
            {
                var channelSchedule = channelSchedules[i];
                for (int i = 0; i < channelSchedule.ChildNodes.Count; i++)
                {
                    var scheduleItem = channelSchedule.ChildNodes[i];
                    if (programIds.Any(p => string.Equals(p, scheduleItem.Attributes["program"].Value, StringComparison.InvariantCultureIgnoreCase)))
                        desiredSchedules.Add(channelSchedule);

                }
            }
            return desiredSchedules;
        }

        private static Dictionary<string, string> GetChannelListing(XmlDocument doc)
        {
            var channels = doc.SelectNodes("/MXF/With/Lineups/Lineup/channels/Channel");
            var channelDictionary = new Dictionary<string, string>();
            for (int i = 0; i < channels.Count; i++)
            {
                channelDictionary.Add(channels[i].Attributes["service"].Value, channels[i].Attributes["number"].Value);
            }
            return channelDictionary;
        }

        private static List<XmlNode> GetDesiredPrograms(List<string> shows, XmlDocument doc)
        {
            var programs = doc.SelectNodes("/MXF/With/Programs/Program");
            var desiredPrograms = new List<XmlNode>();
            for (int i = 0; i < programs.Count; i++)
            {
                var program = programs[i];
                if (shows.Any(s => string.Equals(s, program.Attributes["title"].Value, StringComparison.InvariantCultureIgnoreCase)))
                    desiredPrograms.Add(program);
            }

            return desiredPrograms;
        }
    }
}