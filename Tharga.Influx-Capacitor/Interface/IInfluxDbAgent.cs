using System;
using System.Threading.Tasks;
using InfluxDB.Net.Contracts;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Tharga.InfluxCapacitor.Interface
{
    public interface IKafkaAgent : IDisposable
    {
        Task<IAgentSendResponse> SendAsync(Point[] points);
    }

    public interface IInfluxDbAgent
    {
        Task<bool> CanConnect();
        Task<Pong> PingAsync();
        Task<string> VersionAsync();
        Task<InfluxDbApiResponse> WriteAsync(Point[] points);
        Task CreateDatabaseAsync(string databaseName);
        Tuple<IFormatter, InfluxVersion> GetAgentInfo();
        IFormatter GetFormatter();
        string Description { get; }
    }
}