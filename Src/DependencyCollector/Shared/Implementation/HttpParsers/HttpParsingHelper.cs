﻿namespace Microsoft.ApplicationInsights.DependencyCollector.Implementation.HttpParsers
{
    using System;
    using System.Collections.Generic;

    internal static class HttpParsingHelper
    {
        private static readonly char[] RequestPathEndDelimiters = new char[] { '?', '#' };

        /// <summary>
        /// Builds a resource operation moniker in the format of "VERB /a/*/b/*/c".
        /// </summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="resourcePath">The resource path represented as a list of resource type and resource ID pairs.</param>
        /// <returns>Operation moniker string.</returns>
        internal static string BuildOperationMoniker(string verb, List<KeyValuePair<string, string>> resourcePath)
        {
            var tokens = new List<string>(4 * resourcePath.Count + 2);

            if (!string.IsNullOrEmpty(verb))
            {
                tokens.Add(verb);
                tokens.Add(" ");
            }

            foreach (var resource in resourcePath)
            {
                tokens.Add("/");
                tokens.Add(resource.Key);
                if (resource.Value != null)
                {
                    tokens.Add("/*");
                }
            }

            return string.Concat(tokens);
        }

        /// <summary>
        /// Parses request path into REST resource path represented as a list of resource type and resource ID pairs.
        /// </summary>
        /// <param name="requestPath">The request path.</param>
        /// <returns>A list of resource type and resource ID pairs.</returns>
        internal static List<KeyValuePair<string, string>> ParseResourcePath(string requestPath)
        {
            List<string> tokens = TokenizeRequestPath(requestPath);

            int pairCount = (tokens.Count + 1) / 2;
            var results = new List<KeyValuePair<string, string>>(pairCount);
            for (int i = 0; i < pairCount; i++)
            {
                int keyIdx = 2 * i;
                int valIdx = keyIdx + 1;
                string key = tokens[keyIdx];
                string value = valIdx == tokens.Count ? null : tokens[valIdx];
                if (!string.IsNullOrEmpty(key))
                {
                    results.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            return results;
        }

        /// <summary>
        /// Tokenizes request path.
        /// E.g. the string <code>"/a/b/c/d?e=f"</code> will be tokenized into <code>[ "a", "b", "c", "d" ]</code>.
        /// </summary>
        /// <param name="requestPath">The request path.</param>
        /// <returns>List of tokens.</returns>
        internal static List<string> TokenizeRequestPath(string requestPath)
        {
            var slashPrefixShift = requestPath[0] == '/' ? 1 : 0;
            int endIdx = requestPath.IndexOfAny(RequestPathEndDelimiters, slashPrefixShift);
            List<string> tokens = Split(requestPath, '/', slashPrefixShift, endIdx);

            return tokens;
        }

        /// <summary>
        /// Splits substring by given delimiter.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="startIdx">
        /// The index at which splitting will start. 
        /// This is not validated and expected to be within input string range.
        /// </param>
        /// <param name="endIdx">
        /// The index at which splitting will end. 
        /// If -1 then string will be split till it's end.
        /// This is not validated and expected to be less than string length.
        /// </param>
        /// <returns>A list of substrings.</returns>
        internal static List<string> Split(string str, char delimiter, int startIdx, int endIdx)
        {
            if (endIdx < 0)
            {
                endIdx = str.Length;
            }

            if (endIdx <= startIdx)
            {
                return new List<string>(0);
            }

            var results = new List<string>(16);

            int idx = startIdx;
            while (idx <= endIdx)
            {
                int cutIdx = str.IndexOf(delimiter, idx, endIdx - idx);
                if (cutIdx < 0)
                {
                    cutIdx = endIdx;
                }

                results.Add(str.Substring(idx, cutIdx - idx));
                idx = cutIdx + 1;
            }

            return results;
        }

        /// <summary>
        /// Checks if a string ends with any of the specified suffixes.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="suffixes">The suffixes.</param>
        /// <returns><code>true</code> if string ends with any of the suffixes.</returns>
        internal static bool EndsWithAny(string str, params string[] suffixes)
        {
            foreach (var suffix in suffixes)
            {
                if (str.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
