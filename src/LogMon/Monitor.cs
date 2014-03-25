using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using NLog;

namespace LogMon
{
    public class Monitor
    {
        private const string ConnectionStringName = "AppLogs";
        private const string LastEntryReadSettingsKey = "LastReadEntryId";
        //private const string SymConCastException = "Unable to cast object of type";
        private const string SymConTimeoutException = "Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time";
        private const string AlertRecipientsSettingsKey = "AlertRecipients";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private int? _lastReadEntryId;
        public void Run()
        {
            _logger.Info("Checking for entries");
            try
            {
                var entries = FindEntries(LastReadEntryId).ToList();
                _logger.Info("{0} entries found", entries.Count);

                if (entries.Any())
                {
                    _logger.Info("Sending alerts");
                    SendAlert(entries);
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException(e.Message, e);
            }
            
        }

        protected IEnumerable<LogEntry> FindEntries(int lastReadEntryId)
        {
            var currentEntries = new LogRepository(ConnectionStringName).GetEntries(lastReadEntryId).OrderByDescending(e => e.ID);
            if (currentEntries.Any())
            {
                // Update the LastReadEntryId
                int latestEntryId = currentEntries.First().ID;
                if (latestEntryId > LastReadEntryId)
                {
                    LastReadEntryId = latestEntryId;
                }

                return currentEntries.Where(
                    e => !string.IsNullOrEmpty(e.Exception) && e.Exception.Substring(0, SymConTimeoutException.Length) == SymConTimeoutException);
            }
            return Enumerable.Empty<LogEntry>();
        }

        protected void SendAlert(IEnumerable<LogEntry> entriesFound)
        {
            var entries = entriesFound.ToArray();
            string entryException = entries.First().Exception;
            int occurrences = entries.Length;

            var alert = new AlertEmail();
            alert.Send(entryException, occurrences, AlertRecipients);
        }

        protected int LastReadEntryId
        {
            get
            {
                if (!_lastReadEntryId.HasValue)
                {
                    int settingsId;
                    if (!int.TryParse(ConfigurationManager.AppSettings.Get(LastEntryReadSettingsKey), out settingsId))
                    {
                        settingsId = GetLatestLogEntryId();
                    }
                    _lastReadEntryId = settingsId;
                }
                return _lastReadEntryId.Value;
            }
            set
            {
                _lastReadEntryId = value;

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove(LastEntryReadSettingsKey);
                config.AppSettings.Settings.Add(LastEntryReadSettingsKey, _lastReadEntryId.Value.ToString(CultureInfo.CurrentCulture));
                config.Save();
                
            }
        }

        protected string[] AlertRecipients
        {
            get { return ConfigurationManager.AppSettings.GetValues(AlertRecipientsSettingsKey); }
        }

        private static int GetLatestLogEntryId()
        {
            var latestEntry = new LogRepository(ConnectionStringName).GetLatestEntry();
            return latestEntry.ID;
        }
    }
}
