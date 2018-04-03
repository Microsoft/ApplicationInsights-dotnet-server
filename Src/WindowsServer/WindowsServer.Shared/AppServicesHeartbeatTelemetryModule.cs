﻿namespace Microsoft.ApplicationInsights.WindowsServer
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using Microsoft.ApplicationInsights.WindowsServer.Implementation;

    /// <summary>
    /// Provides default values for the heartbeat feature of Application Insights that
    /// are specific to Azure App Services (Web Apps, Functions, etc...).
    /// </summary>
    public sealed class AppServicesHeartbeatTelemetryModule : ITelemetryModule, IDisposable
    {
        /// <summary>
        /// Environment variables and the Application Insights heartbeat field names that accompany them.
        /// </summary>
        internal readonly KeyValuePair<string, string>[] WebHeartbeatPropertyNameEnvVarMap = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("appSrv_SiteName", "WEBSITE_SITE_NAME"),
            new KeyValuePair<string, string>("appSrv_SiteName", "WEBSITE_SLOT_NAME"),
            new KeyValuePair<string, string>("appSrv_wsStamp", "WEBSITE_HOME_STAMPNAME"),
            new KeyValuePair<string, string>("appSrv_wsHost", "WEBSITE_HOSTNAME"),
            new KeyValuePair<string, string>("appSrv_wsOwner", "WEBSITE_OWNER_NAME")
        };

        // for testing only: override the heartbeat manager
        internal IHeartbeatPropertyManager HeartbeatManager;

        private bool isInitialized = false;
        
        /// <summary>
        /// Initializes a new instance of the<see cref="AppServicesHeartbeatTelemetryModule" /> class.
        /// </summary>
        public AppServicesHeartbeatTelemetryModule() : this(null)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppServicesHeartbeatTelemetryModule" /> class. This is
        /// internal, and allows for overriding the Heartbeat Property Manager to test this module with.
        /// </summary>
        /// <param name="hbeatPropManager">The heartbeat property manager to use when setting/updating env var values.</param>
        internal AppServicesHeartbeatTelemetryModule(IHeartbeatPropertyManager hbeatPropManager)
        {
            this.HeartbeatManager = hbeatPropManager;
        }

        /// <summary>
        /// Initialize the default heartbeat provider for Azure App Services. This module
        /// looks for specific environment variables and sets them into the heartbeat 
        /// properties for Application Insights, if they exist.
        /// </summary>
        /// <param name="configuration">Unused parameter.</param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            this.UpdateHeartbeatWithAppServiceEnvVarValues();
            AppServiceEnvVarMonitor.Instance.MonitoredAppServiceEnvVarUpdatedEvent += this.UpdateHeartbeatWithAppServiceEnvVarValues;
        }

        /// <summary>
        /// Signal the AppServicesHeartbeatTelemetryModule to update the values of the 
        /// Environment variables we use in our heartbeat payload.
        /// </summary>
        public void UpdateHeartbeatWithAppServiceEnvVarValues()
        {
            try
            {
                var hbeatManager = this.GetHeartbeatPropertyManager();
                if (hbeatManager != null)
                {
                    this.isInitialized = this.AddAppServiceEnvironmentVariablesToHeartbeat(hbeatManager, isUpdateOperation: this.isInitialized);
                }
            }
            catch (Exception appSrvEnvVarHbeatFailure)
            {
                WindowsServerEventSource.Log.AppServiceHeartbeatPropertySettingFails(appSrvEnvVarHbeatFailure.ToInvariantString());
            }
        }

        /// <summary>
        /// Remove our event handler from the environment variable monitor.
        /// </summary>
        public void Dispose()
        {
            AppServiceEnvVarMonitor.Instance.MonitoredAppServiceEnvVarUpdatedEvent -= this.UpdateHeartbeatWithAppServiceEnvVarValues;
        }

        internal bool AddAppServiceEnvironmentVariablesToHeartbeat(IHeartbeatPropertyManager hbeatManager, bool isUpdateOperation = false)
        {
            bool hasBeenUpdated = false;

            if (hbeatManager == null)
            {
                WindowsServerEventSource.Log.AppServiceHeartbeatSetCalledWithNullManager();
            }
            else
            {
                foreach (var kvp in this.WebHeartbeatPropertyNameEnvVarMap)
                {
                    try
                    {
                        string hbeatValue = Environment.GetEnvironmentVariable(kvp.Value);
                        string hbeatKey = kvp.Key.ToString();
                        if (isUpdateOperation)
                        {
                            hbeatManager.SetHeartbeatProperty(hbeatKey, hbeatValue);
                        }
                        else
                        {
                            hbeatManager.AddHeartbeatProperty(hbeatKey, hbeatValue, true);
                        }

                        hasBeenUpdated = true;
                    }
                    catch (Exception heartbeatValueException)
                    {
                        WindowsServerEventSource.Log.AppServiceHeartbeatPropertyAquisitionFailed(kvp.Value, heartbeatValueException.ToInvariantString());
                    }
                }
            }

            return hasBeenUpdated;
        }

        private IHeartbeatPropertyManager GetHeartbeatPropertyManager()
        {
            if (this.HeartbeatManager != null)
            {
                // early exit - this is the internal test scenario
                return this.HeartbeatManager;
            }

            IHeartbeatPropertyManager hbeatManager = null;
            var telemetryModules = TelemetryModules.Instance;

            try
            {
                foreach (var module in telemetryModules.Modules)
                {
                    if (module is IHeartbeatPropertyManager hman)
                    {
                        hbeatManager = hman;
                    }
                }
            }
            catch (Exception hearbeatManagerAccessException)
            {
                WindowsServerEventSource.Log.AppServiceHeartbeatManagerAccessFailure(hearbeatManagerAccessException.ToInvariantString());
            }

            if (hbeatManager == null)
            {
                WindowsServerEventSource.Log.AppServiceHeartbeatManagerNotAvailable();
            }

            return hbeatManager;
        }
    }
}