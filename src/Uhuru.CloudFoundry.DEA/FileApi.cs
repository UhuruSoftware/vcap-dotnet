// -----------------------------------------------------------------------
// <copyright file="FileApi.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Uhuru.CloudFoundry.DEA.DirectoryServer;
    using Uhuru.Utilities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class FileApi : IDeaClient
    {
        /// <summary>
        /// Gets or sets the reference to the collection of Droplet Instances.
        /// </summary>
        /// <value>
        /// The collection of Droplet Instances.
        /// </value>
        private static DropletCollection InstanceRegistry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path key.
        /// </summary>
        /// <value>
        /// The path key.
        /// </value>
        private static string PathKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum URL age in seconds.
        /// </summary>
        /// <value>
        /// The maximum URL age in seconds.
        /// </value>
        private static int MaxUrlAgeSeconds
        {
            get;
            set;
        }

        /// <summary>
        /// Sets configuration paramaeters for the DEA File API.
        /// </summary>
        /// <param name="instanceRegistry">The collection of droplet instances.</param>
        /// <param name="pathKey">A string key used to generate SHA512 HMAC hashes.</param>
        /// <param name="maxUrlAgeSeconds">The max URL age in seconds.</param>
        public static void Configure(DropletCollection instanceRegistry, string pathKey, int maxUrlAgeSeconds)
        {
            FileApi.InstanceRegistry = instanceRegistry;
            FileApi.MaxUrlAgeSeconds = maxUrlAgeSeconds;
            FileApi.PathKey = pathKey;
        }

        /// <summary>
        /// Generates a file URL.
        /// </summary>
        /// <param name="instanceId">A droplet instance id.</param>
        /// <param name="path">The path that the client reqested.</param>
        /// <returns>A url that gets sent to the client.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Response gets serialized to JSON.")]
        public static string GenerateFileUrl(string instanceId, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }

            long timeStamp = DateTime.Now.ToBinary();

            string hmac = FileApi.CreateHMACHexdigest(instanceId, path, timeStamp);

            return string.Format(CultureInfo.InvariantCulture, "/instance_paths/{0}?timestamp={1}&hmac={2}&path={3}", instanceId, timeStamp, hmac, path);
        }

        /// <summary>
        /// Looks up the path in the DEA.
        /// </summary>
        /// <param name="path">The path to lookup.</param>
        /// <returns>
        /// A PathLookupResponse containing the response from the DEA.
        /// </returns>
        public PathLookupResponse LookupPath(Uri path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            string instanceId = path.Segments[2];
            NameValueCollection queryStrings = System.Web.HttpUtility.ParseQueryString(path.Query);

            if (!queryStrings.AllKeys.Contains("hmac"))
            {
                return CreateErrorReply((int)System.Net.HttpStatusCode.BadRequest, Strings.HMACParameterRequired);
            }

            if (!queryStrings.AllKeys.Contains("timestamp"))
            {
                return CreateErrorReply((int)System.Net.HttpStatusCode.BadRequest, Strings.TimeStampParameterRequired);
            }

            if (!queryStrings.AllKeys.Contains("path"))
            {
                return CreateErrorReply((int)System.Net.HttpStatusCode.BadRequest, Strings.PathParameterRequired);
            }

            string hmac = queryStrings["hmac"];

            long timeStamp;

            if (!long.TryParse(queryStrings["timestamp"], out timeStamp))
            {
                return CreateErrorReply((int)System.Net.HttpStatusCode.BadRequest, Strings.InvalidTimestampParameter);
            }

            string localPath = queryStrings["path"];

            PathLookupResponse verifyHMACResponse = FileApi.VerifyHMAC(hmac, instanceId, localPath, timeStamp);
            if (verifyHMACResponse != null)
            {
                return verifyHMACResponse;
            }

            PathLookupResponse checkUrlAgeResponse = FileApi.CheckUrlAge(timeStamp);
            if (checkUrlAgeResponse != null)
            {
                return checkUrlAgeResponse;
            }

            DropletInstance instance = null;

            FileApi.InstanceRegistry.ForEach(
                (dropletInstance) =>
                {
                    if (dropletInstance.Properties.InstanceId == instanceId)
                    {
                        instance = dropletInstance;
                    }
                });

            if (instance == null)
            {
                Logger.Warning(Strings.UnknownInstanceId, instanceId);
                return CreateErrorReply((int)System.Net.HttpStatusCode.NotFound, Strings.UnknownInstance);
            }

            if (!instance.Properties.InstancePathAvailable)
            {
                Logger.Warning(Strings.InstancePathUnavailable, instanceId);
                return CreateErrorReply((int)System.Net.HttpStatusCode.ServiceUnavailable, Strings.InstanceUnavailable);
            }

            string fullPath = Path.Combine(instance.Properties.Directory, localPath);

            if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            {
                return CreateErrorReply((int)System.Net.HttpStatusCode.NotFound, Strings.EntityNotFound);
            }

            string realPath = Path.GetFullPath(fullPath);
            if (!realPath.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warning(Strings.RequestedPathOutsideInstance, fullPath, realPath);
                return CreateErrorReply((int)System.Net.HttpStatusCode.Forbidden, Strings.NotAccessible);
            }

            return new PathLookupResponse()
            {
                Path = realPath
            };
        }

        /// <summary>
        /// Creates an HMAC hash.
        /// </summary>
        /// <param name="instanceId">A droplet instance id.</param>
        /// <param name="path">The requested path.</param>
        /// <param name="timeStamp">The current time stamp.</param>
        /// <returns>An SHA512 HMAC.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Keeping same case as Ruby code.")]
        private static string CreateHMACHexdigest(string instanceId, string path, long timeStamp)
        {
            using (HMACSHA512 hmac = new HMACSHA512(ASCIIEncoding.ASCII.GetBytes(FileApi.PathKey)))
            {
                byte[] hash = hmac.ComputeHash(ASCIIEncoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", instanceId, path, timeStamp)));

                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Verifies an HMAC hash.
        /// </summary>
        /// <param name="hexDigest">The HMAC to be verified.</param>
        /// <param name="otherHexdigest">The HMAC, as it should be.</param>
        /// <returns>True if both HMACs are the same.</returns>
        private static bool VerifyHMACHexDigest(string hexDigest, string otherHexdigest)
        {
            if (hexDigest.Length != otherHexdigest.Length)
            {
                return false;
            }

            // We explicity do not short circuit here in order to avoid a timing
            // attack.
            bool verified = true;

            for (int i = 0; i < hexDigest.Length / 2; i++)
            {
                if (Convert.ToByte(hexDigest.Skip(i * 2).Take(2), CultureInfo.InvariantCulture) != Convert.ToByte(otherHexdigest.Skip(i * 2).Take(2), CultureInfo.InvariantCulture))
                {
                    verified = false;
                    break;
                }
            }

            return verified;
        }

        /// <summary>
        /// Verifies that an HMAC is correct.
        /// </summary>
        /// <param name="givenHMAC">An HMAC from a client.</param>
        /// <param name="instanceId">A droplet instance id.</param>
        /// <param name="path">The path the client wants to view.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns>Null if everything is ok, otherwise a PathLookupResponse containing an error.</returns>
        private static PathLookupResponse VerifyHMAC(string givenHMAC, string instanceId, string path, long timeStamp)
        {
            string expectedHMAC = FileApi.CreateHMACHexdigest(instanceId, path, timeStamp);

            if (!FileApi.VerifyHMACHexDigest(expectedHMAC, givenHMAC))
            {
                Logger.Warning(Strings.HMACMismatch);
                return FileApi.CreateErrorReply((int)System.Net.HttpStatusCode.Unauthorized, Strings.InvalidHMAC);
            }

            return null;
        }

        /// <summary>
        /// Checks the age of a request.
        /// </summary>
        /// <param name="timeStamp">The time stamp of the URL.</param>
        /// <returns>Null if everything is ok, otherwise a PathLookupResponse containing an error.</returns>
        private static PathLookupResponse CheckUrlAge(long timeStamp)
        {
            double urlAgeSecs = (DateTime.Now - DateTime.FromBinary(timeStamp)).TotalSeconds;
            int maxAgeSecs = FileApi.MaxUrlAgeSeconds;

            if (urlAgeSecs > maxAgeSecs)
            {
                Logger.Warning(Strings.URLTooOld, urlAgeSecs, maxAgeSecs);
                return CreateErrorReply((int)System.Net.HttpStatusCode.BadRequest, Strings.URLExpired);
            }

            return null;
        }

        /// <summary>
        /// Creates a PathLookupResponse that contains an error.
        /// </summary>
        /// <param name="errorCode">An HTTP error code.</param>
        /// <param name="message">An explanatory message.</param>
        /// <returns>A PathLookupResponse that contains an error message.</returns>
        private static PathLookupResponse CreateErrorReply(int errorCode, string message)
        {
            return new PathLookupResponse()
            {
                Error = new PathLookupResponse.PathLookupResponseError()
                {
                    ErrorCode = errorCode,
                    Message = message
                }
            };
        }
    }
}
