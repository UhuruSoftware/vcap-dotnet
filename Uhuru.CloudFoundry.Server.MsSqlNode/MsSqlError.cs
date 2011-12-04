using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.ServiceBase;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    class MsSqlError : ServiceException
    {
        public static readonly ServiceErrorCode MSSQL_DISK_FULL = new ServiceErrorCode() { ErrorCode = 31001, HttpError = HttpErrorCode.HttpInternal, Message = "Node disk is full." };
        public static readonly ServiceErrorCode MSSQL_CONFIG_NOT_FOUND = new ServiceErrorCode() { ErrorCode = 31002, HttpError = HttpErrorCode.HttpNotFound, Message = "MsSql configuration {0} not found." };
        public static readonly ServiceErrorCode MSSQL_CRED_NOT_FOUND = new ServiceErrorCode() { ErrorCode = 31003, HttpError = HttpErrorCode.HttpNotFound, Message = "MsSql credential {0} not found." };
        public static readonly ServiceErrorCode MSSQL_LOCAL_DB_ERROR = new ServiceErrorCode() { ErrorCode = 31004, HttpError = HttpErrorCode.HttpInternal, Message = "MsSql node local db error." };
        public static readonly ServiceErrorCode MSSQL_INVALID_PLAN = new ServiceErrorCode() { ErrorCode = 31005, HttpError = HttpErrorCode.HttpInternal, Message = "Invalid plan {0}." };

        public MsSqlError(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            base(serviceErrorCode, httpError, message, formattingParameters)
        {

        }

        public MsSqlError(ServiceErrorCode errorCode)
            : base(errorCode)
        {
        }

        public MsSqlError(ServiceErrorCode errorCode, params string[] arguments)
            : base(errorCode, arguments)
        {
        }
    }
}