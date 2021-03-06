﻿using NUnit.Framework;
using NLog;

namespace AzureDevOpsEventsProcessor.Tests.Dsl
{
    using Interfaces;
    using AzureDevOpsEventsProcessor.Providers;

    [TestFixture]
    public class DslLoggingProcessingTests
    {
   
    
        [Test]
        public void Can_log_debug_message_to_nlog()
        {
            // arrange
            var emailProvider = new Moq.Mock<IEmailProvider>();
            var azureDevOpsProvider = new Moq.Mock<IAzureDevOpsProvider>();
            var eventDataProvider = new Moq.Mock<IEventDataProvider>();
            
            // create a memory logger
            var memLogger = Helpers.Logging.CreateMemoryTargetLogger(LogLevel.Debug);
            var engine = new AzureDevOpsEventsProcessor.Dsl.DslProcessor();

            // act
            engine.RunScript(
                @"TestDataFiles\Scripts\logging\logmessage.py", 
                azureDevOpsProvider.Object, 
                emailProvider.Object, 
                eventDataProvider.Object);

            // assert
            Assert.AreEqual(7, memLogger.Logs.Count);
            // memLogger.Logs[0] is the log message from the runscript method
            // memLogger.Logs[1] is the log message from the runscript method
            // memLogger.Logs[2] is the log message from the runscript method
            Assert.AreEqual("DEBUG | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is a debug line", memLogger.Logs[3]);
            Assert.AreEqual("INFO | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is a info line", memLogger.Logs[4]);
            Assert.AreEqual("ERROR | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is an error line", memLogger.Logs[5]);
            Assert.AreEqual("DEBUG | AzureDevOpsEventsProcessor.Dsl.DslLibrary | List: [List item 1] [List item 2] ", memLogger.Logs[6]);

        }

        [Test]
        public void Can_log_info_and_error_messages_only_to_nlog()
        {
            // arrange
            var emailProvider = new Moq.Mock<IEmailProvider>();
            var azureDevOpsProvider = new Moq.Mock<IAzureDevOpsProvider>();
            var eventDataProvider = new Moq.Mock<IEventDataProvider>();

            // create a memory logger
            var memLogger = Helpers.Logging.CreateMemoryTargetLogger(LogLevel.Info);
            var engine = new AzureDevOpsEventsProcessor.Dsl.DslProcessor();

            // act
            engine.RunScript(@"TestDataFiles\Scripts\logging\logmessage.py", azureDevOpsProvider.Object, emailProvider.Object, eventDataProvider.Object);

            // assert
            Assert.AreEqual(5, memLogger.Logs.Count);
            // memLogger.Logs[0] is the log message from the runscript method
            // memLogger.Logs[1] is the log message from the runscript method
            // memLogger.Logs[2] is the log message from the runscript method
            Assert.AreEqual("INFO | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is a info line", memLogger.Logs[3]);
            Assert.AreEqual("ERROR | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is an error line", memLogger.Logs[4]);

        }

        [Test]
        public void Can_log_error_messages_only_to_nlog()
        {
            // arrange
            var emailProvider = new Moq.Mock<IEmailProvider>();
            var azureDevOpsProvider = new Moq.Mock<IAzureDevOpsProvider>();
            var eventDataProvider = new Moq.Mock<IEventDataProvider>();


            // create a memory logger
            var memLogger = Helpers.Logging.CreateMemoryTargetLogger(LogLevel.Error);
            var engine = new AzureDevOpsEventsProcessor.Dsl.DslProcessor();

            // act
            engine.RunScript(@"TestDataFiles\Scripts\logging\logmessage.py", azureDevOpsProvider.Object, emailProvider.Object, eventDataProvider.Object);

            // assert
            Assert.AreEqual(1, memLogger.Logs.Count);
            Assert.AreEqual("ERROR | AzureDevOpsEventsProcessor.Dsl.DslLibrary | This is an error line", memLogger.Logs[0]);

        }

     
    }
}
