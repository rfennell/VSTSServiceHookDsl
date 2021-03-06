﻿//------------------------------------------------------------------------------------------------- 
// <copyright file="JsonDataProvider.cs" company="Black Marble">
// Copyright (c) Black Marble. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureDevOpsEventsProcessor.AlertDetails;
using AzureDevOpsEventsProcessor.Interfaces;

namespace AzureDevOpsEventsProcessor.Providers
{
    /// <summary>
    /// Extracts event data from a Json bloc
    /// </summary>
    public class JsonDataProvider : IEventDataProvider
    {
        private readonly JObject eventJson;

        /// <summary>
        /// Instance of nLog interface
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventJson">The raw JSON</param>
        public JsonDataProvider(JObject eventJson)
        {
            this.eventJson = eventJson;
        }

        /// <summary>
        /// Who changed the work item
        /// </summary>
        /// <returns></returns>
        private string GetChangedBy()
        {
            // this might be in a few places
            try
            {
                return eventJson["resource"]["revision"]["fields"]["System.ChangedBy"].ToString();
            }
            catch (NullReferenceException)
            {
                try
                {
                    return eventJson["resource"]["fields"]["System.ChangedBy"].ToString();
                }
                catch (NullReferenceException)
                {

                    var allChangedFields = GetWorkItemChangedAlertFields();
                    return allChangedFields.SingleOrDefault(c => c.ReferenceName.Equals("System.ChangedBy")).NewValue;
                }
            }
        }

        /// <summary>
        /// List of changed work item field
        /// </summary>
        /// <returns></returns>
        private List<WorkItemChangedAlertDetails> GetWorkItemChangedAlertFields()
        {
            var returnValue = new List<WorkItemChangedAlertDetails>();

#pragma warning disable S3217 // "Explicit" conversions of "foreach" loops should not be used
            foreach (JProperty item in eventJson["resource"]["fields"])
#pragma warning restore S3217 // "Explicit" conversions of "foreach" loops should not be used
            {
                var details = new WorkItemChangedAlertDetails();
                details.ReferenceName = item.Name;
                switch (GetEventType())
                {
                    case "workitem.updated":
                        details.NewValue = RemoveFieldNulls(item.First["newValue"]);
                        details.OldValue = RemoveFieldNulls(item.First["oldValue"]);
                        break;
                    case "workitem.created":
                        details.NewValue = RemoveFieldNulls(item.Value);
                        details.OldValue = string.Empty;
                        break;
                }
                returnValue.Add(details);

            }

            return returnValue;

        }

        /// <summary>
        /// Safety method to make sure no nulls as passed to the email client
        /// </summary>
        /// <param name="fieldValue">The value to add to a message</param>
        /// <returns>A safe string</returns>
        private static string RemoveFieldNulls(JToken fieldValue)
        {
            try
            {
                return fieldValue.ToString();
            }
            catch (NullReferenceException)
            {
                return string.Empty;
            }
        }

        /// Adds all the change data from the alert xml
        /// </summary>
        /// <param name="eventXml">The xml data</param>
        /// <returns>List of changes</returns>
        public BuildAlertDetails GetBuildDetails()
        {

            var returnValue = new BuildAlertDetails()
            {
                Id = Convert.ToInt32(eventJson["resource"]["id"]),
                BuildUri = new Uri(eventJson["resource"]["uri"].ToString()),
                BuildUrl = new Uri(eventJson["resource"]["url"].ToString()),
                Summary = eventJson["resource"]["buildNumber"].ToString(),
                Status = eventJson["resource"]["status"].ToString()
            };

            return returnValue;

        }

        /// <summary>
        /// Gets the event type
        /// </summary>
        /// <returns>The user ID</returns>
        private int GetWorkItemId()
        {
            switch (GetEventType())
            {
                case "workitem.updated":
                    return Convert.ToInt32(eventJson["resource"]["revision"]["id"]);
                default:
                    return Convert.ToInt32(eventJson["resource"]["id"]);
            }
        }

        /// <summary>
        /// Gets the event type
        /// </summary>
        /// <returns>The user ID</returns>
        public string GetEventType()
        {
            return eventJson["eventType"].ToString();
        }

        /// <summary>
        /// Gets the URL of the source TPC
        /// </summary>
        /// <returns>The URL</returns>
        public Uri GetServerUrl()
        {
            string longUri;
            switch (GetEventType())
            {
                case "workitem.updated":
                    longUri = eventJson["resource"]["revision"]["url"].ToString();
                    break;
                case "tfvc.checkin":
                case "workitem.created":
                case "build.complete":
                case "git.push":
                    longUri = eventJson["resource"]["url"].ToString();
                    break;
                default:
                    logger.Info(string.Format("AzureDevOpsEventsProcessor: Unhandled event cannot processed:{0}", GetEventType()));
                    return null;

            }
            // trim off so we only get TPC
            longUri = longUri.Substring(0, longUri.IndexOf("_api") - 1);
            // make sure we have a TPC name
            if (longUri.ToLower().EndsWith(".visualstudio.com"))
            {
                longUri += "/defaultcollection";
            }
            return new Uri(longUri);
        }

        /// <summary>
        /// The subscription ID from the alert
        /// It is returned as string so can be passed in place of event type
        /// </summary>
        /// <returns>The GUID of the subscriptionID as a string</returns>
        public string GetSubsriptionID()
        {
            var id = "00000000-0000-0000-0000-000000000000";
            try
            {
                id = eventJson["subscriptionId"].ToString();
            }
            catch {
                // production always seems to have this but some old test data files don't henc the catch
            }
            return id;
        }
                     
        /// <summary>
        /// Converts the alerts XML to a object
        /// </summary>
        /// <returns>The checkin details</returns>
        public CheckInAlertDetails GetCheckInDetails()
        {

            var returnValue = new CheckInAlertDetails();

            returnValue.Comment = eventJson["resource"]["comment"].ToString();
            returnValue.Changeset = Convert.ToInt32(eventJson["resource"]["changesetId"]);

            returnValue.Summary = eventJson["message"]["text"].ToString();
            returnValue.Committer = eventJson["resource"]["author"]["displayName"].ToString();
            //TeamProject = node.SelectSingleNode("TeamProject").InnerText,
            return returnValue;
        }

        public PushAlertDetails GetPushDetails()
        {
            var returnValue = new PushAlertDetails();

            returnValue.Repo = eventJson["resource"]["repository"]["id"].ToString();
            returnValue.PushId = Convert.ToInt32(eventJson["resource"]["pushId"]);
            return returnValue;
        }

        internal ReleaseAlertDetails GetReleaseDetails()
        {
            var returnValue = new ReleaseAlertDetails();
            try
            {
                returnValue.Id = Convert.ToInt32(eventJson["resource"]["release"]["id"]);
            } catch (NullReferenceException)
            {
                returnValue.Id = Convert.ToInt32(eventJson["resource"]["environment"]["release"]["id"]);
            }
            return returnValue;
        }

        internal MessagePostDetails GetMessagePostDetails()
        {
            var returnValue = new MessagePostDetails();
            returnValue.PostRoomId = Convert.ToInt32(eventJson["resource"]["postedRoomId"]);
            returnValue.Content = eventJson["resource"]["content"].ToString();
            return returnValue;
        }

        public WorkItemAlertDetails GetWorkItemDetails()
        {
            var returnValue = new WorkItemAlertDetails();
            returnValue.ChangedBy = this.GetChangedBy();
            returnValue.ChangedAlertFields = this.GetWorkItemChangedAlertFields();
            returnValue.Id = this.GetWorkItemId();
            return returnValue;
        }

        internal PullAlertDetails GetPullDetails()
        {
            var returnValue = new PullAlertDetails();

            returnValue.Repo = eventJson["resource"]["repository"]["id"].ToString();
            returnValue.PullId = Convert.ToInt32(eventJson["resource"]["pullRequestId"]);
            return returnValue;
        }
    }
}