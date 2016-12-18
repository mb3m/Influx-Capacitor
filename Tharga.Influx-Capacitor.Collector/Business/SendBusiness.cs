using System;
using System.Collections.Generic;
using InfluxDB.Net.Models;
using Tharga.InfluxCapacitor.Agents;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class SendBusiness : ISendBusiness
    {
        private readonly List<IQueue> _queues = new List<IQueue>();

        //public event EventHandler<SendCompleteEventArgs> SendBusinessEvent;
        //private static MyLogger _logger = new MyLogger();

        //public IEnumerable<Tuple<string, int>> GetQueueInfo()
        //{
        //    return _dataSenders.Select(x => new Tuple<string, int>(x.TargetDatabase, x.QueueCount));
        //}

        //private readonly Timer _timer;
        //private readonly List<IDataSender> _dataSenders;
        //private readonly bool _metadata;

        //public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader)
        //{
        //    var config = configBusiness.LoadFiles();

        //    var flushMilliSecondsInterval = 1000 * config.Application.FlushSecondsInterval;
        //    if (flushMilliSecondsInterval > 0)
        //    {
        //        _timer = new Timer(flushMilliSecondsInterval);
        //        _timer.Elapsed += Elapsed;
        //    }

        //    _dataSenders = config.Databases.Where(x => x.IsEnabled).Select(x => x.GetDataSender(influxDbAgentLoader, config.Application.MaxQueueSize)).ToList();
        //    foreach (var dataSender in _dataSenders)
        //    {
        //        dataSender.SendCompleteEvent += OnSendBusinessEvent;
        //    }

        //    _metadata = config.Application.Metadata;
        //}

        //private void Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        var metaPoints = new List<Point>();

        //        foreach (var dataSender in _dataSenders)
        //        {
        //            var previousQueueCount = dataSender.QueueCount;
        //            _logger.Debug(string.Format("Starting to send {0} points to server {1} database {2}.", previousQueueCount, dataSender.TargetServer, dataSender.TargetDatabase));
        //            var success = dataSender.Send();
        //            var postQueueCount = dataSender.QueueCount;
        //            _logger.Debug(string.Format("Done sending {0} points to server {1} database {2}. Now {3} items in queue.", previousQueueCount, dataSender.TargetServer, dataSender.TargetDatabase, postQueueCount));

        //            if (_metadata)
        //            {
        //                metaPoints.Add(MetaDataBusiness.GetQueueCountPoints("Send", dataSender.TargetServer, dataSender.TargetDatabase, previousQueueCount, postQueueCount - previousQueueCount + _dataSenders.Count, success));
        //            }
        //        }

        //        if (_metadata)
        //        {
        //            foreach (var dataSender in _dataSenders)
        //            {
        //                dataSender.Enqueue(metaPoints.ToArray());
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        _logger.Error(exception);
        //        throw;
        //    }
        //}

        //public void Enqueue(Point[] points)
        //{
        //    if (!points.Any())
        //    {
        //        return;
        //    }

        //    if (_timer == null)
        //    {
        //        _logger.Error(string.Format("The engine that sends data to the database is not enabled. Probably becuse the FlushSecondsInterval has not been configured. Point count: {0}.", points.Length));
        //        return;
        //    }

        //    if (!_timer.Enabled)
        //    {
        //        _timer.Start();
        //    }

        //    //Prepare metadata information about the queue and add that to the points to be sent.
        //    if (_metadata)
        //    {
        //        var metaPoints = _dataSenders.Select(x => MetaDataBusiness.GetQueueCountPoints("Enqueue", x.TargetServer, x.TargetDatabase, x.QueueCount, points.Length + _dataSenders.Count, new SendResponse(null, null))).ToArray();
        //        points = points.Union(metaPoints).ToArray();
        //    }

        //    foreach (var dataSender in _dataSenders)
        //    {
        //        _logger.Debug(string.Format("Enqueueing {0} points to server {1} database {2}.", points.Length, dataSender.TargetServer, dataSender.TargetDatabase));
        //        dataSender.Enqueue(points);
        //    }
        //}

        public event EventHandler<SendCompleteEventArgs> SendBusinessEvent;

        public SendBusiness(IConfigBusiness configBusiness, IInfluxDbAgentLoader influxDbAgentLoader, IQueueEvents queueEvents)
        {
            var config = configBusiness.LoadFiles();
            foreach (var databaseConfig in config.Databases)
            {
                ISenderAgent senderAgent = null;
                try
                {
                    if (databaseConfig.IsEnabled)
                    {
                        senderAgent = databaseConfig.GetSenderAgent();
                    }
                }
                catch (Exception exception)
                {
                    queueEvents.ExceptionEvent(exception);
                }

                if (senderAgent != null)
                {
                    var queueSettings = new QueueSettings(config.Application.FlushSecondsInterval, false, config.Application.MaxQueueSize);
                    _queues.Add(new Queue(senderAgent, queueEvents, queueSettings));
                }
            }
        }

        public void Enqueue(Point[] points)
        {
            foreach (var queue in _queues)
            {
                queue.Enqueue(points);
            }
        }

        public IEnumerable<Tuple<string, int>> GetQueueInfo()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnSendBusinessEvent(object sender, SendCompleteEventArgs e)
        {
            var handler = SendBusinessEvent;
            if (handler != null) handler.Invoke(this, e);
        }
    }
}