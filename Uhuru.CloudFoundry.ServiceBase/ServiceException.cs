// -----------------------------------------------------------------------
// <copyright file="ServiceException.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Enum detailing possible HTTP error codes.
    /// </summary>
    public enum HttpErrorCode
    {
        /// <summary>
        /// No error (0).
        /// </summary>
        None = 0,
    
        /// <summary>
        /// Bad request (400).
        /// </summary>
        HttpBadRequest = 400,
     
        /// <summary>
        /// Not authorized (401).
        /// </summary>
        HttpNotAuthorized = 401,
      
        /// <summary>
        /// Forbidden (403).
        /// </summary>
        HttpForbidden = 403,
        
        /// <summary>
        /// Not found (404).
        /// </summary>
        HttpNotFound = 404,
       
        /// <summary>
        /// Internal error (500).
        /// </summary>
        HttpInternal = 500,
       
        /// <summary>
        /// Service unavailable (503).
        /// </summary>
        HttpServiceUnavailable = 503,
       
        /// <summary>
        /// Gateway timeout (504).
        /// </summary>
        HttpGatewayTimeout = 504
    }

    /// <summary>
    /// Exception class raised by a system service.
    /// </summary>
    [Serializable]
    public class ServiceException : Exception
    {
        /// <summary>
        /// Error codes between 30000 - 30099; 400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode InvalidContent = new ServiceErrorCode() { ErrorCode = 30000, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionInvalidContentType };

        /// <summary>
        /// Error codes between 30000 - 30099; 400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode MalFormattedRequest = new ServiceErrorCode() { ErrorCode = 30001, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionMalformedContent };

        /// <summary>
        /// Error codes between 30000 - 30099; 400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode UnknownLabel = new ServiceErrorCode() { ErrorCode = 30002, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionUnknownLabel };

        /// <summary>
        /// Error codes between 30100 - 30199; 401 Unauthorized
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public readonly ServiceErrorCode NotAuthorized = new ServiceErrorCode() { ErrorCode = 30100, HttpError = HttpErrorCode.HttpNotAuthorized, Message = Strings.ServiceExceptionNotAuthorized };

        /// <summary>
        /// Error codes between 30300 - 30399; 404 Not Found 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode NotFound = new ServiceErrorCode() { ErrorCode = 30300, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.ServiceExceptionNotFound };

        /// <summary>
        /// Error codes between 30500 - 30599; 500 Internal Error
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode InternalError = new ServiceErrorCode() { ErrorCode = 30500, HttpError = HttpErrorCode.HttpInternal, Message = Strings.ServiceExceptionInternalError };

        /// <summary>
        /// Error codes between 30600 - 30699; 503 Service Unavailable
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode ServiceUnavailable = new ServiceErrorCode() { ErrorCode = 30600, HttpError = HttpErrorCode.HttpServiceUnavailable, Message = Strings.ServiceExceptionServiceUnavailable };

        /// <summary>
        /// Error codes between 30700 - 30799; 500 Gateway Timeout
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "This is a constant-like field that defines a service error.")]
        public static readonly ServiceErrorCode GatewayTimeout = new ServiceErrorCode() { ErrorCode = 30700, HttpError = HttpErrorCode.HttpGatewayTimeout, Message = Strings.ServiceExceptionGatewayTimeout };

        /// <summary>
        /// Error code of the service exception.
        /// </summary>
        private ServiceErrorCode errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="serviceErrorCode">The service error code.</param>
        /// <param name="httpError">An HTTP error code.</param>
        /// <param name="message">The service error message.</param>
        /// <param name="formattingParameters">Formatting arguments for the message.</param>
        public ServiceException(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            this(new ServiceErrorCode() { ErrorCode = serviceErrorCode, HttpError = httpError, Message = string.Format(CultureInfo.InvariantCulture, message, formattingParameters) })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="errorCode">Service error code.</param>
        public ServiceException(ServiceErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="errorCode">Service error code.</param>
        /// <param name="arguments">Formatting arguments for the message.</param>
        public ServiceException(ServiceErrorCode errorCode, params string[] arguments)
        {
            this.errorCode = errorCode;
            this.errorCode.Message = string.Format(CultureInfo.InvariantCulture, this.errorCode.Message, arguments);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        public ServiceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="streamingContext">The streaming context.</param>
        protected ServiceException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Converts the ServiceError to a string.
        /// </summary>
        /// <returns>A string conataining information about the error.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, Strings.ServiceExceptionAsString, this.errorCode.ErrorCode, this.errorCode.Message);
        }

        /// <summary>
        /// Converts the error to a JSON-serializable dictionary.
        /// </summary>
        /// <returns>A dictionary that is ready to be serialized to a ruby compatible JSON string.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>() 
            {
                { "status", this.errorCode.HttpError },
                { "msg", new Dictionary<string, object>()
                {
                    { "code", this.errorCode.ErrorCode },
                    { "description", this.errorCode.Message }
                }
                }
            };
        }
    }
}
