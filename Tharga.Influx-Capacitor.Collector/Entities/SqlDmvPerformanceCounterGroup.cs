using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class SqlDmvPerformanceCounterGroup : IPerformanceCounterGroup
    {
        private readonly SqlDmvPerformanceCounterProvider _provider;
        private readonly ICounterGroup _counterGroup;
        private List<SqlDmvPerformanceCounterInfo> _counters;

        public SqlDmvPerformanceCounterGroup(SqlDmvPerformanceCounterProvider provider, ICounterGroup counterGroup)
        {
            _provider = provider;
            _counterGroup = counterGroup;
        }

        public string Name
        {
            get { return _counterGroup.Name; }
        }

        public int SecondsInterval
        {
            get { return _counterGroup.SecondsInterval; }
        }

        public int RefreshInstanceInterval
        {
            get { return _counterGroup.RefreshInstanceInterval; }
        }

        public IEnumerable<ITag> Tags
        {
            get { return _counterGroup.Tags; }
        }

        public CollectorEngineType CollectorEngineType
        {
            get { return _counterGroup.CollectorEngineType; }
        }

        public IEnumerable<IPerformanceCounterInfo> GetCounters()
        {
            if (_counters == null)
            {
                lock (this)
                {
                    if (_counters == null)
                    {
                        _provider.EnsureFreshness(this.SecondsInterval);

                        var counters = new List<SqlDmvPerformanceCounterInfo>();

                        foreach (var counter in this._counterGroup.Counters)
                        {
                            counters.Add(new SqlDmvPerformanceCounterInfo(this, counter));
                        }

                        _counters = counters;
                    }
                }
            }

            return _counters;
        }

        public IEnumerable<IPerformanceCounterInfo> GetFreshCounters()
        {
            return _counters;
        }

        public void RemoveCounter(IPerformanceCounterInfo performanceCounterInfo)
        {
            // not supported
        }

        internal float? GetValue(string objectName, string counterName, string instanceName)
        {
            return this._provider.GetValue(this.SecondsInterval, objectName, counterName, instanceName);
        }
    }
}