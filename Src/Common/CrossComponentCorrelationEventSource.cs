﻿namespace Microsoft.ApplicationInsights.Common
{
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using System;
    using System.Globalization;

#if NETCORE
    using System.Diagnostics.Tracing;
    using System.Reflection;
#elif NET45
    using System.Diagnostics.Tracing;
#elif NET40
    using Microsoft.Diagnostics.Tracing;
#endif

    /// <summary>
    /// ETW EventSource tracing class.
    /// </summary>
    [EventSource(Name = "Microsoft-ApplicationInsights-Extensibility-CrossComponentCorrelation")]
    internal sealed class CrossComponentCorrelationEventSource : EventSource
    {
        public static readonly CrossComponentCorrelationEventSource Log = new CrossComponentCorrelationEventSource();

        private CrossComponentCorrelationEventSource()
        {
            this.ApplicationName = this.GetApplicationName();
        }

        public string ApplicationName { [NonEvent]get; [NonEvent]private set; }

        [Event(
            1,
            Keywords = Keywords.UserActionable,
            Message = "Failed to retrieve App ID for the current application insights resource. Make sure the configured instrumentation key is valid. Error: {0}",
            Level = EventLevel.Warning)]
        public void FetchAppIdFailed(string exception, string appDomainName = "Incorrect")
        {
            this.WriteEvent(1, exception, this.ApplicationName);
        }

        [Event(
            2,
            Keywords = Keywords.Diagnostics,
            Message = "Failed to add cross component correlation header. Error: {0}",
            Level = EventLevel.Warning)]
        public void SetHeaderFailed(string exception, string appDomainName = "Incorrect")
        {
            this.WriteEvent(2, exception, this.ApplicationName);
        }

        [Event(
            3,
            Keywords = Keywords.Diagnostics,
            Message = "Failed to determine cross component correlation header. Error: {0}",
            Level = EventLevel.Warning)]
        public void GetHeaderFailed(string exception, string appDomainName = "Incorrect")
        {
            this.WriteEvent(3, exception, this.ApplicationName);
        }

        [NonEvent]
        private string GetApplicationName()
        {
            string name;
            try
            {
#if NETCORE
                name = typeof(CrossComponentCorrelationEventSource).GetTypeInfo().Assembly.GetName().FullName;
#else
                name = AppDomain.CurrentDomain.FriendlyName;
#endif
            }
            catch (Exception exp)
            {
                name = "Undefined " + exp;
            }

            return name;
        }

        /// <summary>
        /// Keywords for the <see cref="CrossComponentCorrelationEventSource"/>.
        /// </summary>
        public sealed class Keywords
        {
            /// <summary>
            /// Key word for user actionable events.
            /// </summary>
            public const EventKeywords UserActionable = (EventKeywords)0x1;

            /// <summary>
            /// Key word for diagnostics events.
            /// </summary>
            public const EventKeywords Diagnostics = (EventKeywords)0x2;
        }
    }
}