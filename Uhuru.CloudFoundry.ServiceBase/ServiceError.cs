using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace Uhuru.CloudFoundry.ServiceBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct ServiceErrorCode
    {
        public int ErrorCode
        {
            get;
            set;
        }

        public HttpErrorCode HttpError
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }

    public enum HttpErrorCode
    {
        None = 0,
        HttpBadRequest = 400,
        HttpNotAuthorized = 401,
        HttpForbidden = 403,
        HttpNotFound = 404,
        HttpInternal = 500,
        HttpServiceUnavailable = 503,
        HttpGatewayTimeout = 504
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ServiceException : Exception
    {
        // Error Code is defined here
        //
        // e.g.
        // ERR_NAME  = [err_code, http_status,     err_message_template]
        // NOT_FOUND = [30300,    HTTP_NOT_FOUND,  '%s not found!'    ]

        // 30000 - 30099  400 Bad Request
        public static readonly ServiceErrorCode InvalidContent = new ServiceErrorCode() { ErrorCode = 30000, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionInvalidContentType };
        public static readonly ServiceErrorCode MalFormattedRequest = new ServiceErrorCode() { ErrorCode = 30001, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionMalformedContent };
        public static readonly ServiceErrorCode UnknownLabel = new ServiceErrorCode() { ErrorCode = 30002, HttpError = HttpErrorCode.HttpBadRequest, Message = Strings.ServiceExceptionUnknownLabel };

        // 30100 - 30199  401 Unauthorized
        public readonly ServiceErrorCode NotAuthorized = new ServiceErrorCode() { ErrorCode = 30100, HttpError = HttpErrorCode.HttpNotAuthorized, Message = Strings.ServiceExceptionNotAuthorized };

        // 30200 - 30299  403 Forbidden

        // 30300 - 30399  404 Not Found
        public static readonly ServiceErrorCode NotFound = new ServiceErrorCode() { ErrorCode = 30300, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.ServiceExceptionNotFound };

        // 30500 - 30599  500 Internal Error
        public static readonly ServiceErrorCode InternalError = new ServiceErrorCode() { ErrorCode = 30500, HttpError = HttpErrorCode.HttpInternal, Message = Strings.ServiceExceptionInternalError };

        // 30600 - 30699  503 Service Unavailable
        public static readonly ServiceErrorCode ServiceUnavailable = new ServiceErrorCode() { ErrorCode = 30600, HttpError = HttpErrorCode.HttpServiceUnavailable, Message = Strings.ServiceExceptionServiceUnavailable };

        // 30700 - 30799  500 Gateway Timeout
        public static readonly ServiceErrorCode GatewayTimeout = new ServiceErrorCode() { ErrorCode = 30700, HttpError = HttpErrorCode.HttpGatewayTimeout, Message = Strings.ServiceExceptionGatewayTimeout };

        ServiceErrorCode errorCode;

        public ServiceException(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            this(new ServiceErrorCode() { ErrorCode = serviceErrorCode, HttpError = httpError, Message = String.Format(CultureInfo.InvariantCulture, message, formattingParameters) })
        {

        }

        public ServiceException(ServiceErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        public ServiceException(ServiceErrorCode errorCode, params string[] arguments)
        {
            this.errorCode = errorCode;
            this.errorCode.Message = String.Format(CultureInfo.InvariantCulture, this.errorCode.Message, arguments);
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, Strings.ServiceExceptionAsString, errorCode.ErrorCode, errorCode.Message);
        }
            
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
