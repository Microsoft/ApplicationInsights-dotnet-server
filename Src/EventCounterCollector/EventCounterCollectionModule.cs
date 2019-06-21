﻿namespace Microsoft.ApplicationInsights.Extensibility.EventCounterCollector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Common;
    using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;

    /// <summary>
    /// Telemetry module for collecting EventCounters.
    /// https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.Tracing/documentation/EventCounterTutorial.md.
    /// </summary>
    public class EventCounterCollectionModule : ITelemetryModule, IDisposable
    {
        /// <summary>
        /// TelemetryClient used to send data.
        /// </summary>        
        private TelemetryClient client = null;

        /// <summary>
        /// Determines how often collection takes place. 
        /// This is not customizable for users as the backend expects 60 sec interval for performance counters.
        /// </summary>
        private TimeSpan collectionPeriod = TimeSpan.FromSeconds(60);

        private EventCounterListener eventCounterListener;
        private bool disposed = false;
        private bool isInitialized = false;      

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCounterCollectionModule"/> class.
        /// </summary>
        public EventCounterCollectionModule()
        {
            this.Counters = new List<EventCounterCollectionRequest>();
        }

        /// <summary>
        /// Gets the list of counter names to collect. Each should have the name of EventSource publishing the counter, and counter name.
        /// </summary>
        public IList<EventCounterCollectionRequest> Counters { get; private set; }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }        

        /// <summary>
        /// Initialize method is called after all configuration properties have been loaded from the configuration.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            try
            {
                EventCounterCollectorEventSource.Log.ModuleIsBeingInitializedEvent(string.Format(
                                CultureInfo.InvariantCulture,
                                "EventCounters count: '{0}'",
                                this.Counters?.Count ?? 0));
                if (!this.isInitialized)
                {
                    this.client = new TelemetryClient(configuration);
                    this.eventCounterListener = new EventCounterListener(this.client, this.Counters);
                    this.client.Context.GetInternalContext().SdkVersion = SdkVersionUtils.GetSdkVersion("evtc:");
                    this.isInitialized = true;
                    EventCounterCollectorEventSource.Log.ModuleInitializedSuccess();
                }
            }
            catch (Exception ex)
            {
                EventCounterCollectorEventSource.Log.ModuleException("Initialization", ex.Message);
            }                        
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        /// <param name="disposing">The method has been called directly or indirectly by a user's code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.eventCounterListener != null)
                    {
                        this.eventCounterListener.Dispose();
                    }
                }

                this.disposed = true;
            }
        }
    }
}