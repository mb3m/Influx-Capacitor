// -----------------------------------------------------------------------
// <copyright file="SqlDmvPerformanceCounterProviderTests.cs" company="MB3M">
// Copyright (c) MB3M. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Business
{
    [TestFixture]
    public class SqlDmvPerformanceCounterProviderTests
    {
        [Test]
        [Explicit("This test requires that SQL Server Express is available on this machine")]

        public void Should_Collect_Local_SqlExpress_Counters()
        {
            var provider = new SqlDmvPerformanceCounterProvider();
            var group = provider.GetGroup(new CounterGroup("Test", 10, 10, new List<ICounter>() { new Counter("MSSQL$SQLEXPRESS:Locks", "Lock Requests/sec", "Application", null, null, null, null, null) }, null, CollectorEngineType.Exact, null, null, false));

            var counters = group.GetCounters().ToList();

            Assert.That(counters.Count, Is.EqualTo(1));
            var counter = counters.First();
            Assert.That(counter.InstanceName, Is.EqualTo("Application"));
            Assert.That(counter.NextValue(), Is.EqualTo(0));
        }
    }
}