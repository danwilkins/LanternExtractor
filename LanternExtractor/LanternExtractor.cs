using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanternExtractor.EQ;
using LanternExtractor.Infrastructure.Logger;

namespace LanternExtractor
{
    static class LanternExtractor
    {
        private static Settings _settings;
        private static ILogger _logger;
        private static bool _useThreading = false;

        private static void Main(string[] args)
        {
            _logger = new TextFileLogger("log.txt");
            _settings = new Settings("settings.txt", _logger);
            _settings.Initialize();
            _logger.SetVerbosity((LogVerbosity) _settings.LoggerVerbosity);

            string archiveName;

            DateTime start = DateTime.Now;

            if (args.Length != 1)
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine($"Usage: {appName} <filename/shortname/all>");
                return;
            }
            
            archiveName = args[0];

            List<string> eqFiles = EqFileHelper.GetValidEqFilePaths(_settings.EverQuestDirectory, archiveName);

            if (eqFiles.Count == 0)
            {
                Console.WriteLine("No valid EQ files found for: '" + archiveName + "' at path: " +
                                  _settings.EverQuestDirectory);
                return;
            }

            if (_useThreading)
            {
                List<Task> tasks = new List<Task>();

                foreach (var file in eqFiles)
                {
                    string fileName = file;
                    Task task = Task.Factory.StartNew(() =>
                    {
                        ArchiveExtractor.Extract(fileName, "Exports/", _logger, _settings);
                    });
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                foreach (var file in eqFiles)
                {
                    ArchiveExtractor.Extract(file, "Exports/", _logger, _settings);
                }
            }

            Console.WriteLine($"Extraction complete ({(DateTime.Now - start).TotalSeconds})s");
        }
    }
}