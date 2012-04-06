// -----------------------------------------------------------------------
// <copyright file="VcapComponent.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using Uhuru.Configuration;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// Represents a Cloud Foundry component, that has to be registered, and uses a NATS connection.
    /// </summary>
    public class VCAPComponent : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VCAPComponent"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "No easy way to get around this without a lot of refactoring.")]
        public VCAPComponent()
        {
            this.VarzLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            this.Varz = new Dictionary<string, object>();
            this.Discover = new Dictionary<string, object>();

            this.ConstructReactor();

            this.UUID = Guid.NewGuid().ToString("N");

            // Initialize Index from config file
            if (this.Index != null)
            {
                this.UUID = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.Index, this.UUID);
            }

            this.Host = NetworkInterface.GetLocalIPAddress(UhuruSection.GetSection().DEA.LocalRoute);
            VCAPReactor.Uri = new Uri(UhuruSection.GetSection().DEA.MessageBus);

            // http server port
            this.Port = NetworkInterface.GrabEphemeralPort();

            this.Authentication = new string[] { Credentials.GenerateCredential(32), Credentials.GenerateCredential(32) };
        }

        /// <summary>
        /// Gets or sets the varz lock for the varz property.
        /// </summary>
        public ReaderWriterLockSlim VarzLock { get; set; }

        /// <summary>
        /// Gets or sets the varz value to be published.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization.")]
        public Dictionary<string, object> Varz { get; set; }

        /// <summary>
        /// Gets or sets the healthz value to be published.
        /// </summary>
        public string Healthz
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the discover.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "It is used for JSON (de)serialization.")]
        protected Dictionary<string, object> Discover { get; set; }

        /// <summary>
        /// Gets or sets the timestamp the component started.
        /// </summary>
        protected DateTime StartedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the UUID of the VCAP component.
        /// </summary>
        protected string UUID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the VCAP component (DEA, CloudControlser, Node, Healthmanager, etc.).
        /// </summary>
        protected string ComponentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        protected string Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the NATS server URI.
        /// </summary>
        protected Uri NatsUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host address of the VCAP component to be announced.
        /// </summary>
        protected string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server port.
        /// </summary>
        protected int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server authentication.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization.")]
        protected string[] Authentication
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the VCAP reactor.
        /// </summary>
        protected VCAPReactor VCAPReactor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server used for healthz and varz.
        /// </summary>
        protected MonitoringServer MonitoringServer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the cpu performance.
        /// </summary>
        /// <value>
        /// The cpu performance.
        /// </value>
        protected PerformanceCounter CpuPerformance
        {
            get;
            set;
        }

        /// <summary>
        /// Runs this the VCAP component. This method is non-blocking.
        /// </summary>
        public virtual void Run()
        {
            // Listen for discovery requests
            VCAPReactor.OnComponentDiscover += delegate(string msg, string reply, string subject)
            {
                this.UpdateDiscoverUptime();
                VCAPReactor.SendReply(reply, JsonConvertibleObject.SerializeToJson(this.Discover));
            };

            VCAPReactor.Start();

            this.Discover = new Dictionary<string, object>() 
            {
              { "type", this.ComponentType },
              { "index", this.Index },
              { "uuid", this.UUID },
              { "host", string.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.Host, this.Port) },
              { "credentials", this.Authentication },
              { "start", RubyCompatibility.DateTimeToRubyString(this.StartedAt = DateTime.Now) }
            };
            
            // Varz is customizable
            this.Varz = new Dictionary<string, object>();
            foreach (string key in this.Discover.Keys)
            {
                this.Varz[key] = this.Discover[key];
            }

            this.Varz["num_cores"] = Environment.ProcessorCount;

            // todo: change this to a more accurate method
            // consider:
            // PerformanceCounter upTime = new PerformanceCounter("System", "System Up Time");
            // upTime.NextValue();
            // TimeSpan ts2 = TimeSpan.FromSeconds(upTime.NextValue());
            // Console.WriteLine("{0}d {1}h {2}m {3}s", ts2.Days, ts2.Hours, ts2.Minutes, ts2.Seconds);
            this.Varz["system_start"] = RubyCompatibility.DateTimeToRubyString(DateTime.Now.AddMilliseconds(-Environment.TickCount));

            this.CpuPerformance = new PerformanceCounter();
            this.CpuPerformance.CategoryName = "Processor Information";
            this.CpuPerformance.CounterName = "% Processor Time";
            this.CpuPerformance.InstanceName = "_Total";
            this.CpuPerformance.NextValue();

            this.Healthz = "ok\n";

            this.StartHttpServer();

            // Also announce ourselves on startup..
            VCAPReactor.SendVCAPComponentAnnounce(JsonConvertibleObject.SerializeToJson(this.Discover)); 
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && MonitoringServer != null)
            {
                MonitoringServer.Dispose();
            }
        }

        /// <summary>
        /// Constructs the reactor for the VCAP component.
        /// </summary>
        protected virtual void ConstructReactor()
        {
            if (VCAPReactor == null)
            {
                VCAPReactor = new VCAPReactor();
            }
        }

        /// <summary>
        /// Updates the varz structure with uptime, cpu, memory usage ....
        /// </summary>
        protected void SnapshotVarz()
        {
            try
            {
                this.VarzLock.EnterWriteLock();

                TimeSpan span = DateTime.Now - this.StartedAt;
                this.Varz["uptime"] = string.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);

                float cpu = ((float)Process.GetCurrentProcess().TotalProcessorTime.Ticks / span.Ticks) * 100;

                // trim it to one decimal precision
                cpu = float.Parse(cpu.ToString("F1", CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);

                this.Varz["cpu"] = cpu;
                this.Varz["mem"] = Process.GetCurrentProcess().WorkingSet64 / 1024;

                // extra uhuru information
                this.Varz["cpu_time"] = Process.GetCurrentProcess().TotalProcessorTime;

                // this is the cpu percentage for the time span between the Nextvalue calls;
                this.Varz["system_cpu"] = this.CpuPerformance.NextValue();
                this.Varz["system_cpu_ticks"] = this.CpuPerformance.RawValue;

                // todo: add memory usage here
                // consider:
                // PerformanceCounter ramCounter;
                // ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                // ramCounter.NextValue();
                this.Varz["system_mem"] = null;
            }
            finally
            {
                this.VarzLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates the discover uptime.
        /// </summary>
        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - this.StartedAt;
            this.Discover["uptime"] = string.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        /// <summary>
        /// Starts the HTTP server used for healthz and varz.
        /// </summary>
        private void StartHttpServer()
        {
            MonitoringServer = new MonitoringServer(this.Port, this.Host, this.Authentication[0], this.Authentication[1]);

            MonitoringServer.VarzRequested += delegate(object sender, VarzRequestEventArgs response)
            {
                try
                {
                    this.VarzLock.EnterWriteLock();
                    response.VarzMessage = JsonConvertibleObject.SerializeToJson(this.Varz);
                }
                finally
                {
                    this.VarzLock.ExitWriteLock();
                }
            };

            MonitoringServer.HealthzRequested += delegate(object sender, HealthzRequestEventArgs response)
            {
                response.HealthzMessage = this.Healthz;
            };

            MonitoringServer.Start();
        }
    }
}
