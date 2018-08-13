namespace Microsoft.ApplicationInsights.Common
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Web;

    using Microsoft.ApplicationInsights.W3C;
    using Microsoft.ApplicationInsights.Web.Implementation;

#pragma warning disable 612, 618
    internal class ActivityHelpers
    {
        internal const string RequestActivityItemName = "Microsoft.ApplicationInsights.Web.Activity";

        internal static string RootOperationIdHeaderName { get; set; }

        internal static string ParentOperationIdHeaderName { get; set; }

        internal static bool IsW3CTracingEnabled { get; set; } = false;

        /// <summary> 
        /// Checks if given RequestId is hierarchical.
        /// </summary>
        /// <param name="requestId">Request id.</param>
        /// <returns>True if requestId is hierarchical false otherwise.</returns>
        internal static bool IsHierarchicalRequestId(string requestId)
        {
            return !string.IsNullOrEmpty(requestId) && requestId[0] == '|';
        }

        internal static bool TryParseCustomHeaders(HttpRequest request, out string rootId, out string parentId)
        {
            rootId = parentId = null;
            if (ParentOperationIdHeaderName != null && RootOperationIdHeaderName != null)
            {
                parentId = request.UnvalidatedGetHeader(ParentOperationIdHeaderName);
                rootId = request.UnvalidatedGetHeader(RootOperationIdHeaderName);

                if (rootId?.Length == 0)
                {
                    rootId = null;
                }

                if (parentId?.Length == 0)
                {
                    parentId = null;
                }
            }

            return rootId != null || parentId != null;
        }

        internal static void ExtractW3CContext(HttpRequest request, Activity activity)
        {
            var traceParent = request.UnvalidatedGetHeader(W3CConstants.TraceParentHeader);
            if (traceParent != null)
            {
                var traceParentStr = StringUtilities.EnforceMaxLength(traceParent, InjectionGuardConstants.TraceParentHeaderMaxLength);
                activity.SetTraceparent(traceParentStr);

                if (activity.ParentId == null)
                {
                    activity.SetParentId(activity.GetTraceId());
                }
            }
            else
            {
                activity.GenerateW3CContext();
            }

            var traceState = request.UnvalidatedGetHeaders().GetHeaderValue(
                W3CConstants.TraceStateHeader,
                InjectionGuardConstants.TraceStateHeaderMaxLength,
                InjectionGuardConstants.TraceStateMaxPairs)?.ToList();
            if (traceState != null && traceState.Any())
            {
                var pairsExceptAppId = traceState.Where(s => !s.StartsWith(W3CConstants.ApplicationIdTraceStateField + "=", StringComparison.Ordinal));
                string traceStateExceptAppId = string.Join(",", pairsExceptAppId);

                activity.SetTracestate(StringUtilities.EnforceMaxLength(traceStateExceptAppId, InjectionGuardConstants.TraceStateHeaderMaxLength));
            }

            if (!activity.Baggage.Any())
            {
                var baggage = request.Headers.GetNameValueCollectionFromHeader(RequestResponseHeaders.CorrelationContextHeader);

                if (baggage != null && baggage.Any())
                {
                    foreach (var item in baggage)
                    {
                        var itemName = StringUtilities.EnforceMaxLength(item.Key, InjectionGuardConstants.ContextHeaderKeyMaxLength);
                        var itemValue = StringUtilities.EnforceMaxLength(item.Value, InjectionGuardConstants.ContextHeaderValueMaxLength);
                        activity.AddBaggage(itemName, itemValue);
                    }
                }
            }
        }
    }
#pragma warning restore 612, 618
}