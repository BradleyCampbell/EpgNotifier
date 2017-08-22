using System;
using System.Collections.Generic;
using System.Text;

namespace EpgNotifier
{
    public class ChannelSchedule
    {
        public string ServiceId { get; set; }
        public string ChannelNumber { get; set; }
        public Dictionary<DateTime, TvProgram> Schedule {get; set;}

        public string ToStringFirstOccurence()
        {
            var sb = new StringBuilder($"Channel Number: {ChannelNumber}").AppendLine();
            var usedShows = new List<String>();
            foreach (var time in Schedule.Keys)
            {
                if (usedShows.Contains(Schedule[time].Title)) continue;
                sb.AppendLine($"     {time.ToLocalTime().ToString("G")} - {Schedule[time].ToString()}");
                usedShows.Add(Schedule[time].Title);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"Channel Number: {ChannelNumber}").AppendLine();
            foreach (var time in Schedule.Keys)
            {
                sb.AppendLine($"     {time.ToLocalTime().ToString("G")} - {Schedule[time].ToString()}");
            }
            return sb.ToString();
        }
    }
}
