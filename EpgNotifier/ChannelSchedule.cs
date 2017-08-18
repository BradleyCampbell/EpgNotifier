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

        public override string ToString()
        {
            var sb = new StringBuilder($"Channel Number: {ChannelNumber}").AppendLine();
            foreach (var time in Schedule.Keys)
            {
                sb.AppendLine($"     {time.ToString("G")} - {Schedule[time].ToString()}");
            }
            return sb.ToString();
        }
    }
}
