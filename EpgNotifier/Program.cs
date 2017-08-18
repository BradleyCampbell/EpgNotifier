using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace EpgNotifier
{
    public class EpgNotifier
    {
        static string _showListFileName;
        static string _guideFileName;

        public static void Main(string[] args)
        {
            var arg = Array.IndexOf(args, "-ShowList");
            if (arg != -1)
            {
                if ((arg + 1) == args.Length)
                {
                    return;
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
                    return;
                }

                _guideFileName = args[arg + 1];
                args[arg] = null;
                args[arg + 1] = null;
            }

            var shows = File.ReadAllLines(_showListFileName).ToList();

            XmlDocument doc = new XmlDocument();
            doc.Load(_guideFileName);

            var programs = doc.SelectNodes("/MXF/With/Programs/Program");
            var desiredPrograms = new List<XmlNode>();
            for (int i = 0; i < programs.Count; i++)
            {
                var program = programs[i];
                if (shows.Any(s => string.Equals(s, program.Attributes["title"].Value, StringComparison.InvariantCultureIgnoreCase)))
                    desiredPrograms.Add(program);
            }



        }
    }
}