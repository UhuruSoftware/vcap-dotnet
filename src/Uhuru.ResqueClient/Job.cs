using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

// -----------------------------------------------------------------------
// <copyright file="$safeitemrootname$.cs" company="$registeredorganization$">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace Uhuru.ResqueClient
{
    public class Job
    {
        private Dictionary<string, object> payload;
        private string queue;
    
        public Job(string queue, Dictionary<string, object> payload)
        {
            this.queue = queue;
            this.payload = payload;
            

        }

        public string Queue
        {
            get
            {
                return this.queue;
            }
        }

        public Dictionary<string, object> Payload
        {
            get
            {
                return this.payload;
            }
        }

        public Worker Worker
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public void Perform()
        {
            string className = payload["class"].ToString();
            if (!this.Worker.JobClasses.Keys.Contains(className))
            {
                throw new NotImplementedException();
            }
            string fqClassName = this.Worker.JobClasses[className];
            string args = payload["args"].ToString();

            Type classType = Type.GetType(fqClassName);
            var instance = Activator.CreateInstance(classType);
            classType.InvokeMember("Perform", System.Reflection.BindingFlags.InvokeMethod, null, instance, new object[] { args });
        }

        public void Fail()
        {
            throw new System.NotImplementedException();
        }
    }
}
