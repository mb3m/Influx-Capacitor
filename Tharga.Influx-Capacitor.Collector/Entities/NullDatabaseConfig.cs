using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Entities
{
    public class NullDatabaseConfig : IDatabaseConfig
    {
        public bool IsEnabled { get; }
        public string Url { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Name { get; private set; }

        public NullDatabaseConfig(bool enabled)
        {
            IsEnabled = enabled;
        }

        public IDataSender GetDataSender(IInfluxDbAgentLoader influxDbAgentLoader, int maxQueueSize)
        {
            return new NullDataSender(maxQueueSize);
        }
    }
}