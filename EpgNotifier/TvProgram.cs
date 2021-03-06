﻿using System;
using System.Collections.Generic;
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

        public override int GetHashCode()
        {
            var hashCode = -594151479;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + SeasonNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + EpisodeNumber.GetHashCode();
            return hashCode;
        }
    }
}
