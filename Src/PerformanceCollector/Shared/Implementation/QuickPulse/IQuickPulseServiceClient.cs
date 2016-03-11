﻿namespace Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.QuickPulse
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;

    internal interface IQuickPulseServiceClient
    {
        /// <summary>
        /// Gets the QPS URI.
        /// </summary>
        Uri ServiceUri { get; }

        /// <summary>
        /// Pings QPS to check if it expects data right now.
        /// </summary>
        /// <param name="instrumentationKey">InstrumentationKey for which to submit data samples.</param>
        /// <param name="timestamp">Timestamp to pass to the server.</param>
        /// <returns><b>true</b> if data is expected, otherwise <b>false</b>.</returns>
        bool? Ping(string instrumentationKey, DateTimeOffset timestamp);

        /// <summary>
        /// Submits a data samples to QPS.
        /// </summary>
        /// <param name="samples">Data samples.</param>
        /// <param name="instrumentationKey">InstrumentationKey for which to submit data samples.</param>
        /// <param name="timerProvider">Time provider.</param>
        /// <returns><b>true</b> if the client is expected to keep sending data samples, <b>false</b> otherwise.</returns>
        bool? SubmitSamples(IEnumerable<QuickPulseDataSample> samples, string instrumentationKey, Clock timerProvider);
    }
}