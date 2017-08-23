using System;
using System.Diagnostics;

namespace EpgNotifier
{
    [DebuggerDisplay("{Title} - {Season}x{Episode}")]
    public class TvProgram
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string EpisodeTitle { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public DateTime OriginalAirDate { get; set; }


        public override string ToString()
        {
            var returnMe = Title;
            if (SeasonNumber != -1)
            {
                returnMe += $" - {SeasonNumber}";
                if (EpisodeNumber != -1)
                    returnMe += $"x{EpisodeNumber}";
            }
            return returnMe;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TvProgram p = (TvProgram)obj;
            return (Title == p.Title) && (EpisodeNumber == p.EpisodeNumber) && (SeasonNumber == p.SeasonNumber);
        }
    }
}
