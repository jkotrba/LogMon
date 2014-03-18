using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace LogMon
{
    public class LogRepository
    {
        private const string MinEntryIdParamName = "@minEntryId";
        private readonly string _connectionStringName;
        public LogRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }
        
        public string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings[_connectionStringName].ConnectionString; }
        }

        public IEnumerable<LogEntry> GetEntries(int minEntryId)
        {
            var entries = new List<LogEntry>();
            const string queryText = "SELECT * FROM dbo.Log WHERE [Id] > " + MinEntryIdParamName;
            using (var conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryText;
                    cmd.Parameters.AddWithValue(MinEntryIdParamName, minEntryId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            entries.Add(ReadEntry(reader));
                        }
                    }
                }
            }
            return entries;
        }

        public LogEntry GetLatestEntry()
        {
            LogEntry latestEntry = null;
            const string queryTxt = "SELECT TOP 1 * FROM dbo.Log ORDER BY [ID] DESC";
            using (var conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = queryTxt;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            latestEntry = ReadEntry(reader);
                        }
                    }
                }
            }
            return latestEntry;
        }

        private LogEntry ReadEntry(DbDataReader dataReader)
        {
            var entry = new LogEntry();
            
            int idOrd = dataReader.GetOrdinal("ID");
            entry.ID = dataReader.GetFieldValue<int>(idOrd);

            int applicationOrd = dataReader.GetOrdinal("Application");
            entry.Application = dataReader.IsDBNull(applicationOrd) ? null : dataReader.GetFieldValue<string>(applicationOrd);

            int machineOrd = dataReader.GetOrdinal("MachineName");
            entry.MachineName = dataReader.IsDBNull(machineOrd) ? null : dataReader.GetFieldValue<string>(machineOrd);

            int timestampOrd = dataReader.GetOrdinal("TimeStamp");
            entry.TimeStamp = dataReader.GetFieldValue<DateTime>(timestampOrd);

            int threadOrd = dataReader.GetOrdinal("Thread");
            entry.Thread = dataReader.IsDBNull(threadOrd) ? null : dataReader.GetFieldValue<string>(threadOrd);

            int levelOrd = dataReader.GetOrdinal("Level");
            entry.Level = dataReader.IsDBNull(levelOrd) ? null : dataReader.GetFieldValue<string>(levelOrd);

            int loggerOrd = dataReader.GetOrdinal("Logger");
            entry.Logger = dataReader.IsDBNull(loggerOrd) ? null : dataReader.GetFieldValue<string>(loggerOrd);

            int messageOrd = dataReader.GetOrdinal("Message");
            entry.Message = dataReader.IsDBNull(messageOrd) ? null : dataReader.GetFieldValue<string>(messageOrd);

            int exceptionOrd = dataReader.GetOrdinal("Exception");
            entry.Exception = dataReader.IsDBNull(exceptionOrd) ? null : dataReader.GetFieldValue<string>(exceptionOrd);

            int ntUserOrd = dataReader.GetOrdinal("NTUser");
            entry.NTUser = dataReader.IsDBNull(ntUserOrd) ? null : dataReader.GetFieldValue<string>(ntUserOrd);

            return entry;
        }
    }

    
}
