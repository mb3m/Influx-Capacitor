using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public sealed class SqlDmvPerformanceCounterProvider : IPerformanceCounterProvider
    {
        private readonly string _connectionString = "server=.\\sqlexpress;integrated security=sspi";
        private SqlPerfCounterStore _store;
        private DateTime _rowsAge;

        public void Setup(ICounterProviderConfig config)
        {
        }

        public IPerformanceCounterGroup GetGroup(ICounterGroup @group)
        {
            return new SqlDmvPerformanceCounterGroup(this, group);
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetCounterNames(string categoryName, string machineName)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetInstances(string category, string counterName, string machineName)
        {
            return Enumerable.Empty<string>();
        }

        internal float? GetValue(int secondsInterval, string objectName, string counterName, string instanceName)
        {
            EnsureFreshness(secondsInterval);
            return _store.Get(objectName, counterName, instanceName);
        }

        internal void EnsureFreshness(int secondsInterval)
        {
            var elapsed = DateTime.Now - _rowsAge;
            if (elapsed.TotalSeconds > secondsInterval)
            {
                Collect();
            }
        }

        private void Collect()
        {
            var newStore = new SqlPerfCounterStore();

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                using (var cmd = sqlConnection.CreateCommand())
                {
                    cmd.CommandText = "select * from sys.dm_os_performance_counters order by object_name, counter_name, instance_name";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            newStore.Add(reader.GetString(0).TrimEnd(), reader.GetString(1).TrimEnd(), reader.GetString(2).TrimEnd(), reader.GetInt64(3));
                        }
                    }
                }
            }

            _store = newStore;
            _rowsAge = DateTime.Now;
        }

        private class SqlPerfCounterStore
        {
            private readonly Dictionary<string, Dictionary<string, Dictionary<string, long>>> _store = new Dictionary<string, Dictionary<string, Dictionary<string, long>>>(StringComparer.Ordinal);

            private string _currentObjectName;

            private string _currentCounterName;

            private Dictionary<string, Dictionary<string, long>> _currentObjectStore;

            private Dictionary<string, long> _currentCounterStore;

            public long? Get(string objectName, string counterName, string instanceName)
            {
                Dictionary<string, Dictionary<string, long>> categoryStore;
                if (!_store.TryGetValue(objectName, out categoryStore))
                {
                    return null;
                }

                Dictionary<string, long> counterStore;
                if (!categoryStore.TryGetValue(counterName, out counterStore))
                {
                    return null;
                }

                long value;
                if (!counterStore.TryGetValue(instanceName, out value))
                {
                    return null;
                }

                return value;
            }

            public void Add(string objectName, string counterName, string instanceName, long value)
            {
                if (_currentObjectStore == null || !string.Equals(_currentObjectName, objectName, StringComparison.Ordinal))
                {
                    _currentObjectStore = new Dictionary<string, Dictionary<string, long>>(StringComparer.Ordinal);
                    _store[objectName] = _currentObjectStore;
                    _currentObjectName = objectName;
                    _currentCounterName = null;
                }

                if (_currentObjectName == null || !string.Equals(_currentCounterName, counterName, StringComparison.Ordinal))
                {
                    _currentCounterStore = new Dictionary<string, long>(StringComparer.Ordinal);
                    _currentObjectStore[counterName] = _currentCounterStore;
                    _currentCounterName = counterName;
                }

                _currentCounterStore[instanceName] = value;
            }
        }
    }
}
