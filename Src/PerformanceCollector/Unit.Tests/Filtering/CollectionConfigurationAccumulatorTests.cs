﻿namespace Unit.Tests
{
    using System;
    using System.Linq;

    using Microsoft.ApplicationInsights.Extensibility.Filtering;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CollectionConfigurationAccumulatorTests
    {
        [TestMethod]
        public void CollectionConfigurationAccumulatorPreparesMetricAccumulatorsTest()
        {
            // ARRANGE
            string[] error;
            var metricInfo = new OperationalizedMetricInfo()
                                 {
                                     Id = "Metric1",
                                     TelemetryType = TelemetryType.Request,
                                     Projection = "Name",
                                     Aggregation = AggregationType.Min,
                                     FilterGroups = new FilterConjunctionGroupInfo[0]
                                 };

            var collectionConfigurationInfo = new CollectionConfigurationInfo() { Metrics = new[] { metricInfo } };
            var collectionConfiguration = new CollectionConfiguration(collectionConfigurationInfo, out error);

            // ACT
            var accumulator = new CollectionConfigurationAccumulator(collectionConfiguration);

            // ASSERT
            Assert.AreSame(collectionConfiguration, accumulator.CollectionConfiguration);
            Assert.AreEqual("Metric1", accumulator.MetricAccumulators.Single().Key);
            Assert.AreEqual(AggregationType.Min, accumulator.MetricAccumulators.Single().Value.AggregationType);
        }

        [TestMethod]
        public void CollectionConfigurationAccumulatorPreparesMetricAccumulatorsForMetricsTest()
        {
            // ARRANGE
            string[] error;
            var metricInfo = new OperationalizedMetricInfo()
                                 {
                                     Id = "Metric1",
                                     TelemetryType = TelemetryType.Metric,
                                     Projection = "Value",
                                     Aggregation = AggregationType.Min,
                                     FilterGroups = new FilterConjunctionGroupInfo[0]
                                 };

            var collectionConfigurationInfo = new CollectionConfigurationInfo() { Metrics = new[] { metricInfo } };
            var collectionConfiguration = new CollectionConfiguration(collectionConfigurationInfo, out error);

            // ACT
            var accumulator = new CollectionConfigurationAccumulator(collectionConfiguration);

            // ASSERT
            Assert.AreSame(collectionConfiguration, accumulator.CollectionConfiguration);
            Assert.AreEqual("Metric1", accumulator.MetricAccumulators.Single().Key);
            Assert.AreEqual(AggregationType.Min, accumulator.MetricAccumulators.Single().Value.AggregationType);
        }
    }
}