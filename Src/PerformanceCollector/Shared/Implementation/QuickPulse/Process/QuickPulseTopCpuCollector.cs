﻿namespace Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.QuickPulse
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;

    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.Implementation.QuickPulse.Helpers;

    /// <summary>
    /// Top CPU collector.
    /// </summary>
    internal sealed class QuickPulseTopCpuCollector : IQuickPulseTopCpuCollector
    {
        private readonly TimeSpan accessDeniedRetryInterval = TimeSpan.FromMinutes(1);

        private readonly Clock timeProvider;

        private readonly IQuickPulseProcessProvider processProvider;

        // process name => (last observation timestamp, last observation value)
        private readonly Dictionary<string, TimeSpan> processObservations = new Dictionary<string, TimeSpan>(StringComparer.Ordinal);

        private DateTimeOffset prevObservationTime;

        private TimeSpan? prevOverallTime;

        private DateTimeOffset lastReadAttempt = DateTimeOffset.MinValue;

        public QuickPulseTopCpuCollector(Clock timeProvider, IQuickPulseProcessProvider processProvider)
        {
            this.timeProvider = timeProvider;
            this.processProvider = processProvider;

            this.InitializationFailed = false;
            this.AccessDenied = false;
        }

        public bool InitializationFailed { get; private set; }

        public bool AccessDenied { get; private set; }

        public IEnumerable<Tuple<string, int>> GetTopProcessesByCpu(int topN)
        {
            try
            {
                DateTimeOffset now = this.timeProvider.UtcNow;

                if (this.InitializationFailed)
                {
                    // the initialization has failed, so we never attempt to do anything
                    return Enumerable.Empty<Tuple<string, int>>();
                }

                if (this.AccessDenied && now - this.lastReadAttempt < this.accessDeniedRetryInterval)
                {
                    // not enough time has passed since we got denied access, so don't retry just yet
                    return Enumerable.Empty<Tuple<string, int>>();
                }

                var procData = new List<Tuple<string, double>>();
                var encounteredProcs = new HashSet<string>();
                
                this.lastReadAttempt = now;

                TimeSpan? totalTime;
                foreach (var process in this.processProvider.GetProcesses(out totalTime))
                {
                    encounteredProcs.Add(process.ProcessName);
                    
                    TimeSpan lastObservation;
                    if (!this.processObservations.TryGetValue(process.ProcessName, out lastObservation))
                    {
                        // this is the first time we're encountering this process
                        this.processObservations.Add(process.ProcessName, process.TotalProcessorTime);

                        continue;
                    }

                    TimeSpan cpuTimeSinceLast = process.TotalProcessorTime - lastObservation;

                    this.processObservations[process.ProcessName] = process.TotalProcessorTime;

                    // use perf data if available; otherwise, calculate it ourselves
                    TimeSpan timeElapsedOnAllCoresSinceLast = (totalTime - this.prevOverallTime)
                                                              ?? TimeSpan.FromTicks((now - this.prevObservationTime).Ticks * Environment.ProcessorCount);

                    double usagePercentage = timeElapsedOnAllCoresSinceLast.Ticks > 0
                                                 ? (double)cpuTimeSinceLast.Ticks / timeElapsedOnAllCoresSinceLast.Ticks
                                                 : 0;

                    procData.Add(Tuple.Create(process.ProcessName, usagePercentage));
                }

                this.CleanState(encounteredProcs);

                this.prevObservationTime = now;
                this.prevOverallTime = totalTime;

                this.AccessDenied = false;

                // TODO: implement partial sort instead of full sort below
                return procData.OrderByDescending(p => p.Item2).Select(p => Tuple.Create(p.Item1, (int)(p.Item2 * 100))).Take(topN);
            }
            catch (Exception e)
            {
                QuickPulseEventSource.Log.ProcessesReadingFailedEvent(e.ToInvariantString());

                if (e is UnauthorizedAccessException || e is SecurityException)
                {
                    this.AccessDenied = true;
                }

                return Enumerable.Empty<Tuple<string, int>>();
            }
        }

        public void Initialize()
        {
            this.InitializationFailed = false;
            this.AccessDenied = false;
            
            try
            {
                this.processProvider.Initialize();
            }
            catch (Exception e)
            {
                QuickPulseEventSource.Log.ProcessesReadingFailedEvent(e.ToInvariantString());

                this.InitializationFailed = true;

                if (e is UnauthorizedAccessException || e is SecurityException)
                {
                    this.AccessDenied = true;
                }
            }
        }

        public void Close()
        {
            this.processProvider.Close();
        }

        private void CleanState(HashSet<string> encounteredProcs)
        {
            // remove processes that we haven't encountered this time around
            string[] processCpuKeysToRemove = this.processObservations.Keys.Where(p => !encounteredProcs.Contains(p)).ToArray();
            foreach (var key in processCpuKeysToRemove)
            {
                this.processObservations.Remove(key);
            }
        }
    }
}