using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace Uhuru.CloudFoundry.ServiceBase
{
    /// <summary>
    /// This structure contains service error information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct ServiceErrorCode
    {
        /// <summary>
        /// Gets or sets an error code.
        /// </summary>
        public int ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an http error code for the service error.
        /// </summary>
        public HttpErrorCode HttpError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ServiceException : Exception
    {

        /// <summary>
        /// 30000 - 30099  400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode InvalidContent = new ServiceErrorCode() { ErrorCode = 30000, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionInvalidContentType };

        /// <summary>
        /// 30000 - 30099  400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode MalFormattedRequest = new ServiceErrorCode() { ErrorCode = 30001, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionMalformedContent };

        /// <summary>
        /// 30000 - 30099  400 Bad Request
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode UnknownLabel = new ServiceErrorCode() { ErrorCode = 30002, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionUnknownLabel };

        /// <summary>
        /// 30100 - 30199  401 Unauthorized
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly ServiceErrorCode NotAuthorized = new ServiceErrorCode() { ErrorCode = 30100, HttpError = HttpErrorCode.HttpNotAuthorized, Message = Strings.ServiceExceptionNotAuthorized };

        /// <summary>
        /// 30300 - 30399  404 Not Found 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode NotFound = new ServiceErrorCode() { ErrorCode = 30300, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.ServiceExceptionNotFound };

        /// <summary>
        /// 30500 - 30599  500 Internal Error
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode InternalError = new ServiceErrorCode() { ErrorCode = 30500, HttpError = HttpErrorCode.HttpInternal, Message = Strings.ServiceExceptionInternalError };

        /// <summary>
        /// 30600 - 30699  503 Service Unavailable
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode ServiceUnavailable = new ServiceErrorCode() { ErrorCode = 30600, HttpError = HttpErrorCode.HttpServiceUnavailable, Message = Strings.ServiceExceptionServiceUnavailable };

        /// <summary>
        /// 30700 - 30799  500 Gateway Timeout
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public static readonly ServiceErrorCode GatewayTimeout = new ServiceErrorCode() { ErrorCode = 30700, HttpError = HttpErrorCode.HttpGatewayTimeout, Message = Strings.ServiceExceptionGatewayTimeout };

        ServiceErrorCode errorCode;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="serviceErrorCode">The service error code.</param>
        /// <param name="httpError">An HTTP error code.</param>
        /// <param name="message">The service error message.</param>
        /// <param name="formattingParameters">Formatting arguments for the message.</param>
        public ServiceException(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            this(new ServiceErrorCode() { ErrorCode = serviceErrorCode, HttpError = httpError, Message = String.Format(CultureInfo.InvariantCulture, message, formattingParameters) })
        {

        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="errorCode">Service error code.</param>
        public ServiceException(ServiceErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="errorCode">Service error code.</param>
        /// <param name="arguments">Formatting arguments for the message.</param>
        public ServiceException(ServiceErrorCode errorCode, params string[] arguments)
        {
            this.errorCode = errorCode;
            this.errorCode.Message = String.Format(CultureInfo.InvariantCulture, this.errorCode.Message, arguments);
        }

        /// <summary>
        /// Converts the ServiceError to a string.
        /// </summary>
        /// <returns>A string conataining information about the error.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, Strings.ServiceExceptionAsString, errorCode.ErrorCode, errorCode.Message);
        }
        
        /// <summary>
        /// Converts the error to a JSON-serializable dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>() {
                {"status", errorCode.HttpError},
                {"msg", new Dictionary<string, object>(){
                    {"code", errorCode.ErrorCode},
                    {"description", errorCode.Message}
                }
                }
            };
        }
    }
}
