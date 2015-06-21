﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InfluxDB.Net.Collector.Console.Business;
using InfluxDB.Net.Collector.Console.Entities;
using InfluxDB.Net.Models;

namespace InfluxDB.Net.Collector.Console
{
    static class Program
    {
        private static InfluxDb _client;
        private static Config _config;

        static void Main(string[] args)
        {
            var configBusiness = new ConfigBusiness();
            _config = configBusiness.LoadFiles(args);

            _client = new InfluxDb(_config.Database.Url, _config.Database.Username, _config.Database.Password);

            var pong = _client.PingAsync().Result;
            System.Console.WriteLine("Ping: {0} ({1} ms)", pong.Status, pong.ResponseTime);

            var version = _client.VersionAsync().Result;
            System.Console.WriteLine("Version: {0}", version);

            var counterBusiness = new CounterBusiness();
            var counterGroups = counterBusiness.GetPerformanceCounterGroups(_config).ToArray();

            RegisterCounterValues("Processor", counterGroups.First());

            System.Console.WriteLine("Press enter to exit...");
            System.Console.ReadKey();
        }

        private static InfluxDbApiResponse RegisterCounterValues(string name, IEnumerable<PerformanceCounter> processorCounters)
        {
            var columnNames = new List<string>();
            var datas = new List<object>();

            foreach (var processorCounter in processorCounters)
            {
                var data = processorCounter.NextValue();

                columnNames.Add(processorCounter.InstanceName);
                datas.Add(data);

                System.Console.WriteLine("{0} {1}: {2}", processorCounter.CounterName, processorCounter.InstanceName, data);
            }

            var serie = new Serie.Builder(name)
                .Columns(columnNames.Select(x => name + x).ToArray())
                .Values(datas.ToArray()) //TODO: This is provided as one value, hot as a list as it should
                .Build();
            var result = _client.WriteAsync(_config.Database.Name, TimeUnit.Milliseconds, serie);
            return result.Result;
        }
    }
}