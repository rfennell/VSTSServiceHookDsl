﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using TFSEventsProcessor.Helpers;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TFSEventsProcessor.Tests.Helpers
{
    static class ServiceHookTestData
    {

        /// <summary>
        /// The json we get from the TFS server call
        /// </summary>
        /// <returns></returns>
        internal static JObject GetEventJson(string eventName)
        {
            return JObject.Parse(File.ReadAllText(FolderHelper.GetRootedPath($".\\TestDataFiles\\RestData\\Alerts\\{eventName}.json")));
        }

    }
}
