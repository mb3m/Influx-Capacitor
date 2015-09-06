﻿using System;
using System.Threading.Tasks;
using Tharga.InfluxCapacitor.Collector;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.InfluxCapacitor.Console.Commands.Service;
using Tharga.Toolkit.Console.Command.Base;

namespace Tharga.InfluxCapacitor.Console.Commands.Config
{
    internal class ConfigAutoCommand : ConfigCommandBase
    {
        private readonly IConfigBusiness _configBusiness;
        private readonly ICounterBusiness _counterBusiness;

        public ConfigAutoCommand(IInfluxDbAgentLoader influxDbAgentLoader, IConfigBusiness configBusiness, ICounterBusiness counterBusiness)
            : base("Auto", "Automatically run full setup.", influxDbAgentLoader, configBusiness)
        {
            _configBusiness = configBusiness;
            _counterBusiness = counterBusiness;
        }

        public override async Task<bool> InvokeAsync(string paramList)
        {
            var status = true;
            try
            {
                var index = 0;

                var defaultUrl = _configBusiness.OpenDatabaseConfig().Url;
                var influxDbVersion = _configBusiness.OpenDatabaseConfig().InfluxDbVersion;

                var response = await GetServerUrlAsync(paramList, index++, defaultUrl, influxDbVersion);
                if (response == null || string.IsNullOrEmpty(response.Item1))
                {
                    status = false;
                    return false;
                }

                var config = _configBusiness.OpenDatabaseConfig();
                var logonInfo = await GetUsernameAsync(paramList, index++, config, "config_auto");
                if (logonInfo == null)
                {
                    status = false;
                    return false;
                }

                _configBusiness.InitiateApplicationConfig();

                if (!InitiateDefaultCounters())
                {
                    status = false;
                    return false;
                }

                if (!await StartService())
                {
                    status = false;
                    return false;
                }

                return true;
            }
            finally
            {
                if (!status)
                {
                    OutputLine("Setup did not go as it should. Correct the issues manually and type exit when you are done.", ConsoleColor.Blue, OutputLevel.Information);
                    OutputLine("Use the site https://github.com/poxet/Influx-Capacitor for support.", ConsoleColor.Blue, OutputLevel.Information);
                }
            }
        }

        private bool InitiateDefaultCounters()
        {
            var initaiteBusiness = new DataInitiator(_configBusiness, _counterBusiness);
            var messages = initaiteBusiness.CreateAll();
            var someIssue = false;
            foreach (var message in messages)
            {
                OutputLine(message.Item1, message.Item2);

                if (message.Item2 == OutputLevel.Error)
                {
                    someIssue = true;
                }
            }

            return !someIssue;
        }

        private async Task<bool> StartService()
        {
            OutputInformation("Trying to start the service...");

            var result = await ServiceCommands.GetServiceStatusAsync();
            if (result != null)
            {
                await ServiceCommands.RestartServiceAsync();
            }
            else
            {
                OutputError("The service {0} cannot be found on this machine.", Constants.ServiceName);
                return false;
            }

            return true;
        }
    }
}