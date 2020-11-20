﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Compatibility.Logger;
using Microsoft.Omex.Extensions.Compatibility.TimedScopes;
using Microsoft.Omex.Extensions.Compatibility.Validation;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Compatibility.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void AddOmexCompatibilityServices_RegisterTypes()
		{
			Mock<ILogger> mockLogger = new Mock<ILogger>();
			Mock<ILoggerFactory> mockFactory = new Mock<ILoggerFactory>();
			mockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddSingleton(mockFactory.Object)
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();

			EventId eventId = new EventId(1);
			Category category = new Category("Test");
			LogLevel logLevel = LogLevel.Error;
			string logMessage = "TestLogMessage";
			Exception exception = new Exception();

			mockLogger.Invocations.Clear();
			ULSLogging.LogTraceTag(eventId, category, logLevel, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.LogTraceTag not calling ILogger");

			mockLogger.Invocations.Clear();
			ULSLogging.ReportExceptionTag(eventId, category, exception, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.ReportExceptionTag not calling ILogger");

			mockLogger.Invocations.Clear();
			Code.Validate(false, logMessage, eventId);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "Code.Validate not calling ILogger");

			using (TimedScope startedTimedScope = new TimedScopeDefinition("TestStartedTimedScope").Create(TimedScopeResult.SystemError))
			{
				AssertResult(ActivityResultStrings.SystemError);
			}

			using (TimedScope notStartedTimedScope = new TimedScopeDefinition("TestNotStartedTimedScope").Create(TimedScopeResult.ExpectedError, false))
			{
				notStartedTimedScope.Start();
				AssertResult(ActivityResultStrings.ExpectedError);
			}
		}

		private static void AssertResult(string expectedResult)
		{
			string? value = Activity.Current?.Tags.FirstOrDefault(p => string.Equals(p.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
			Assert.AreEqual(expectedResult, value);
		}

		[TestMethod]
		public void Code_Expects_ThrowException()
		{
			
			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			Assert.ThrowsException<OmexCompatibilityInitializationException>(() => Code.Expects<OmexCompatibilityInitializationException>(false, logMessage, eventId));

		}

		[TestMethod]
		public void Code_ExpectsArgument_ThrowException()
		{
			
			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			Assert.ThrowsException<ArgumentNullException>(() => Code.ExpectsArgument<int?>(null, logMessage, eventId));

		}

		[TestMethod]
		public void Code_ExpectsArgument_ReturnArgumentValue()
		{

			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			int? value = 2;
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			int? argumentValue = Code.ExpectsArgument<int?>(2, logMessage, eventId);

			Assert.AreEqual(value, argumentValue);

		}

		[TestMethod]
		public void Code_ExpectsNotNullOrWhiteSpaceArgument_ThrowArgumentException()
		{

			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			Assert.ThrowsException<ArgumentException>(() => Code.ExpectsNotNullOrWhiteSpaceArgument(" ", logMessage, eventId));

		}

		[TestMethod]
		public void Code_ExpectsNotNullOrWhiteSpaceArgument_ThrowArgumentNullException()
		{

			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			Assert.ThrowsException<ArgumentNullException>(() => Code.ExpectsNotNullOrWhiteSpaceArgument(null, logMessage, eventId));

		}

		[TestMethod]
		public void Code_ExpectsNotNullOrWhiteSpaceArgument_ReturnArgumentValue()
		{

			EventId eventId = new EventId(1);
			string logMessage = "TestLogMessage";
			string value = "test";
			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddTimedScopes()
						.AddOmexCompatibilityServices();
				})
				.Build()
				.Start();
			string argumentValue = Code.ExpectsNotNullOrWhiteSpaceArgument("test", logMessage, eventId);

			Assert.AreEqual(value, argumentValue);

		}


	}
}
