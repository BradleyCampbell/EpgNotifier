using System;
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
            sb.AppendLine("Summary:");
            foreach(var show in tvPrograms.Distinct().OrderBy(t => t.Title).ThenBy(t => t.SeasonNumber).ThenBy(t => t.EpisodeNumber))
            {
                sb.AppendLine(show.ToString());
            }


            sb.AppendLine().AppendLine("Schedule:");
            foreach (var schedule in applicableSchedules)
            {
                sb.AppendLine(schedule.ToString());
            }

            Console.WriteLine(sb.ToString());

            EmailNotifications(sb.ToString());
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