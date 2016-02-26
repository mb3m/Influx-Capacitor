using System.Collections.Generic;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    class SqlDmvPerformanceCounterInfo : IPerformanceCounterInfo
    {
        private readonly SqlDmvPerformanceCounterGroup _group;
        private readonly ICounter _counter;
        private readonly string _name;
        private string _instanceName;

        public SqlDmvPerformanceCounterInfo(SqlDmvPerformanceCounterGroup group, ICounter counter)
        {
            _group = @group;
            _counter = counter;

            _name = counter.Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool HasPerformanceCounter
        {
            get { return this._group.GetValue(this.CategoryName, this.CounterName, this.InstanceName) != null; }
        }

        public string CategoryName
        {
            get { return _counter.CategoryName; }
        }

        public string CounterName
        {
            get { return _counter.CounterName; }
        }

        public string InstanceName
        {
            get { return _instanceName ?? _counter.InstanceName; }
            set { _instanceName = value; }
        }

        public string FieldName
        {
            get { return _counter.FieldName; }
        }

        public string MachineName
        {
            get { return _counter.MachineName; }
        }

        public string Alias
        {
            get { return _counter.Alias; }
        }

        public IEnumerable<ITag> Tags
        {
            get { return _counter.Tags; }
        }

        public float? Max
        {
            get { return _counter.Max; }
        }

        public float NextValue()
        {
            return this._group.GetValue(this.CategoryName, this.CounterName, this.InstanceName) ?? 0;
        }
    }
}