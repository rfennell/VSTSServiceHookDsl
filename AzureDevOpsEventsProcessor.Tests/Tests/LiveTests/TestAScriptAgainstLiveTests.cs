﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.QualityTools.Testing.Fakes;

namespace AzureDevOpsEventsProcessor.Tests.Live
{
    using IronPython.Hosting;

    using Moq;

    using NLog;

    using AzureDevOpsEventsProcessor.Providers;
    using Interfaces;
    using Helpers;

    /// <summary>
    /// These are last resort tests, when you have a script that does show its faults through logging
    /// Usually commented out
    /// </summary>
    [TestFixture]
    public class TestAScriptAgainstLiveTests
    {
        [Ignore("Only uncomment this test if wish to do local debug against remote instance")]
        [Test]
        public void Can_run_a_script_against_live()
        {
            // arrange
            var memLogger = Helpers.Logging.CreateMemoryTargetLogger(LogLevel.Debug);
            var uri = new Uri("https://dev.azure.com/instance");
            var pat = "<YOUR PAT>";
            var wi = 98578;
            var eventType = "workitem.updated";

            var emailProvider = new Moq.Mock<IEmailProvider>();
            var azureDevOpsProvider = new AzureDevOpsProvider(uri, pat);
            var eventDataProvider =  new Moq.Mock<IEventDataProvider>();
     
            var engine = new AzureDevOpsEventsProcessor.Dsl.DslProcessor();
            var args = new Dictionary<string, object>
            {
                { "Arguments", new[] { eventType, wi.ToString() } },
            };

            // act
            // add the name of the script you want to locally debug
            engine.RunScript(
                @"TestDataFiles\Scripts\AzureDevOps\alerts\markparentasblocked.py",
                args,
                azureDevOpsProvider,
                emailProvider.Object,
                eventDataProvider.Object
                );

            // assert

            // dump the log to look at
            Console.Write(string.Join(Environment.NewLine, memLogger.Logs));
            // Always fail as this is a manual test in relity
            Assert.IsFalse(true);
            
        }

    }
}
