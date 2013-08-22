// -----------------------------------------------------------------------
// <copyright file="DeaStartMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Uhuru.CloudFoundry.DEA.Messages;
    using Uhuru.Configuration;
    using Uhuru.Utilities;

    class Staging
    {
        private string dropletDir;
        private DeaReactor nats;
        private StagingTaskRegistry stagingTaskRegistry;
        private string deaId;
        private string buildpacksDirectory;

        public Staging(string deaId, StagingTaskRegistry stagingTaskRegistry, string dropletDir, DeaReactor nats, string buildpacksDirectory)
        {
            this.dropletDir = dropletDir;
            this.nats = nats;
            this.deaId = deaId;
            this.stagingTaskRegistry = stagingTaskRegistry;
            this.buildpacksDirectory = buildpacksDirectory;
        }        

        public void HandleMessage(StagingStartMessageRequest message, string replyTo)
        {
            StagingTask task = new StagingTask(message, dropletDir, buildpacksDirectory);
            stagingTaskRegistry.Register(task);

            NotifySetupCompletion(message, replyTo, task);
            NotifyUpload(message, replyTo, task);
            NotifyStop(message, replyTo, task);

            task.Start();
        }        

        private void NotifySetupCompletion(StagingStartMessageRequest message, string replyTo, StagingTask task) 
        {
            task.AfterSetup += delegate(Exception error)
            {
                StagingStartMessageResponse response = new StagingStartMessageResponse();
                response.TaskId = task.TaskId;
                response.TaskStreamingLogURL = task.StreamingLogUrl;
                if (error != null)
                {
                    response.Error = error.ToString();
                }
                this.nats.SendReply(replyTo, response.SerializeToJson());
            };
        }
       
        private void NotifyUpload(StagingStartMessageRequest message, string replyTo, StagingTask task) 
        {
            task.AfterUpload += delegate(Exception error)
            {
                StagingStartMessageResponse response = new StagingStartMessageResponse();
                response.TaskId = task.TaskId;
                response.TaskLog = task.TaskLog;
                if (error != null)
                {
                    response.Error = error.ToString();
                }
                response.DetectedBuildpack = task.DetectedBuildpack;
                response.DropletSHA = task.DropletSHA;

                this.nats.SendReply(replyTo, response.SerializeToJson());
                stagingTaskRegistry.Unregister(task);
            };  
        }
        
        private void NotifyStop(StagingStartMessageRequest message, string replyTo, StagingTask task) 
        {
            task.AfterStop += delegate(Exception error)
            {
                StagingStartMessageResponse response = new StagingStartMessageResponse();
                response.TaskId = task.TaskId;                
                if (error != null)
                {
                    response.Error = error.ToString();
                }
                this.nats.SendReply(replyTo, response.SerializeToJson());
                stagingTaskRegistry.Unregister(task);
            };            
        }

        
    }
}
