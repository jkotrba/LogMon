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
        //is http://websvcs1.mtnam.org/symcon.service.alps/symconservice.svc
        private const string ConnectionStringName = "AppLogs";
        private const string LastEntryReadSettingsKey = "LastReadEntryId";
        private const string SymConCastException = "Unable to cast object of type";
        private const string AlertRecipientsSettingsKey = "AlertRecipients";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private int? _lastReadEntryId;
        public void Run()
        {
            _logger.Debug("Monitor Run");
            try
            {
                //_logger.Debug();
                var entries = FindEntries(LastReadEntryId);
                if (entries.Any())
                {
                    SendAlert(entries);
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("Run failure", e);
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
                    e => !string.IsNullOrEmpty(e.Exception) && e.Exception.Substring(0, 30) == SymConCastException);
            }
            return null;
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
               ConfigurationManager.AppSettings.Set(LastEntryReadSettingsKey, value.ToString(CultureInfo.CurrentUICulture));
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
