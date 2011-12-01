using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.MsSqlNode.Base
{
    struct ServiceErrorCode
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

    enum HttpErrorCode
    {
        HTTP_BAD_REQUEST = 400,
        HTTP_NOT_AUTHORIZED = 401,
        HTTP_FORBIDDEN = 403,
        HTTP_NOT_FOUND = 404,
        HTTP_INTERNAL = 500,
        HTTP_SERVICE_UNAVAIL = 503,
        HTTP_GATEWAY_TIMEOUT = 504
    }


    class ServiceError : Exception
    {
        // Error Code is defined here
        //
        // e.g.
        // ERR_NAME  = [err_code, http_status,     err_message_template]
        // NOT_FOUND = [30300,    HTTP_NOT_FOUND,  '%s not found!'    ]

        // 30000 - 30099  400 Bad Request
        public static readonly ServiceErrorCode INVALID_CONTENT = new ServiceErrorCode() { ErrorCode = 30000, HttpError = HttpErrorCode.HTTP_BAD_REQUEST, Message = "Invalid Content-Type" };
        public static readonly ServiceErrorCode MALFORMATTED_REQ = new ServiceErrorCode() { ErrorCode = 30001, HttpError = HttpErrorCode.HTTP_BAD_REQUEST, Message = "Malformatted request" };
        public static readonly ServiceErrorCode UNKNOWN_LABEL = new ServiceErrorCode() { ErrorCode = 30002, HttpError = HttpErrorCode.HTTP_BAD_REQUEST, Message = "Unknown label" };

        // 30100 - 30199  401 Unauthorized
        public readonly ServiceErrorCode NOT_AUTHORIZED = new ServiceErrorCode() { ErrorCode = 30100, HttpError = HttpErrorCode.HTTP_NOT_AUTHORIZED, Message = "Not authorized" };

        // 30200 - 30299  403 Forbidden

        // 30300 - 30399  404 Not Found
        public static readonly ServiceErrorCode NOT_FOUND = new ServiceErrorCode() { ErrorCode = 30300, HttpError = HttpErrorCode.HTTP_NOT_FOUND, Message = "{0} not found" };

        // 30500 - 30599  500 Internal Error
        public static readonly ServiceErrorCode INTERNAL_ERROR = new ServiceErrorCode() { ErrorCode = 30500, HttpError = HttpErrorCode.HTTP_INTERNAL, Message = "Internal Error" };

        // 30600 - 30699  503 Service Unavailable
        public static readonly ServiceErrorCode SERVICE_UNAVAILABLE = new ServiceErrorCode() { ErrorCode = 30600, HttpError = HttpErrorCode.HTTP_SERVICE_UNAVAIL, Message = "Service unavailable" };

        // 30700 - 30799  500 Gateway Timeout
        public static readonly ServiceErrorCode GATEWAY_TIMEOUT = new ServiceErrorCode() { ErrorCode = 30700, HttpError = HttpErrorCode.HTTP_GATEWAY_TIMEOUT, Message = "Gateway timeout" };

        ServiceErrorCode errorCode;

        public ServiceError(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            this(new ServiceErrorCode() { ErrorCode = serviceErrorCode, HttpError = httpError, Message = String.Format(message, formattingParameters) })
        {

        }

        public ServiceError(ServiceErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        public ServiceError(ServiceErrorCode errorCode, params string[] arguments)
        {
            this.errorCode = errorCode;
            this.errorCode.Message = String.Format(this.errorCode.Message, arguments);
        }

        public override string ToString()
        {
 	        return String.Format("Error Code: {0}, Error Message: #{1}", errorCode.ErrorCode, errorCode.Message);
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
