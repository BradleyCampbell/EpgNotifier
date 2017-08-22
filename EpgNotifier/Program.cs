using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        private static bool _episodeSummary;
        private static bool _showSummary;
        private static bool _schedule;
        private static bool _scheduleFirstOnly;

        public static void Main(string[] args)
        {
            if (!AreArgumentsValid(args))
                return;

            var shows = File.ReadAllLines(_showListFileName).ToList();

            XmlDocument doc = new XmlDocument();
            doc.Load(_guideFileName);

            var mxfParser = new MxfParser(doc);

            var tvPrograms = mxfParser.GetDesiredPrograms(shows);
            var applicableSchedules = mxfParser.GetSchedulesOfPrograms(tvPrograms);

            var sb = new StringBuilder();

            if (_showSummary)
            {
                AddShowSummary(tvPrograms, sb);
            }

            if (_episodeSummary)
            {
                AddEpisodeSummary(tvPrograms, sb);
            }

            if (_scheduleFirstOnly)
            {
                AddScheduleFistOnly(applicableSchedules, sb);
            }

            if (_schedule)
            {
                AddSchedule(applicableSchedules, sb);
            }

            Console.WriteLine(sb.ToString());

            EmailNotifications(sb.ToString());
        }

        private static void AddScheduleFistOnly(List<ChannelSchedule> applicableSchedules, StringBuilder sb)
        {
            sb.AppendLine().AppendLine("Schedule (First occurrence only):");
            foreach (var schedule in applicableSchedules)
            {
                sb.AppendLine(schedule.ToStringFirstOccurence());
            }
        }

        private static void AddSchedule(List<ChannelSchedule> applicableSchedules, StringBuilder sb)
        {
            sb.AppendLine().AppendLine("Schedule:");
            foreach (var schedule in applicableSchedules)
            {
                sb.AppendLine(schedule.ToString());
            }
        }

        private static void AddEpisodeSummary(List<TvProgram> tvPrograms, StringBuilder sb)
        {
            sb.AppendLine("Episode Summary:");
            foreach (var show in tvPrograms.Distinct().OrderBy(t => t.Title).ThenBy(t => t.SeasonNumber).ThenBy(t => t.EpisodeNumber))
            {
                sb.AppendLine(show.ToString());
            }
        }

        private static void AddShowSummary(List<TvProgram> tvPrograms, StringBuilder sb)
        {
            sb.AppendLine("Show Summary:");
            foreach (var show in (new HashSet<string>(tvPrograms.Select(t => t.Title))).OrderBy(t => t))
            {
                sb.AppendLine(show.ToString());
            }
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

            arg = Array.IndexOf(args, "-ShowSummary");
            if (arg != -1)
            {
                _showSummary = true;
                args[arg] = null;
            }

            arg = Array.IndexOf(args, "-EpisodeSummary");
            if (arg != -1)
            {
                _episodeSummary = true;
                args[arg] = null;
            }

            arg = Array.IndexOf(args, "-Schedule");
            if (arg != -1)
            {
                _schedule = true;
                args[arg] = null;
            }

            arg = Array.IndexOf(args, "-ScheduleFirstOnly");
            if (arg != -1)
            {
                _scheduleFirstOnly = true;
                args[arg] = null;
            }

            return true;
        }

        private static void EmailNotifications(string body)
        {
            SmtpClient client = new SmtpClient
            {
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                Credentials = new NetworkCredential("username", "password"),
                EnableSsl = true
            };

            MailMessage mail = new MailMessage("bradleycampbell@gmail.com", "bradleycampbell@gmail.com")
            {
                Subject = "Upcoming Shows To Record",
                Body = body
            };

            client.Send(mail);
        }
    }
}