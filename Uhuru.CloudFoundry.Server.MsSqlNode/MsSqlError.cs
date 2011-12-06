using System;
using Uhuru.CloudFoundry.ServiceBase;

namespace Uhuru.CloudFoundry.Server.MSSqlNode
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    class MSSqlError : ServiceException
    {
        public static readonly ServiceErrorCode MSSqlDiskFull = new ServiceErrorCode() { ErrorCode = 31001, HttpError = HttpErrorCode.HttpInternal, Message = Strings.SqlServerErrorMessageDiskFull };
        public static readonly ServiceErrorCode MSSqlConfigNotFound = new ServiceErrorCode() { ErrorCode = 31002, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.SqlServerErrorMessageConfigurationNotFound };
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly ServiceErrorCode MSSqlCredentialsNotFound = new ServiceErrorCode() { ErrorCode = 31003, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.SqlServerErrorMessageCredentialNotFound };
        public static readonly ServiceErrorCode MSSqlLocalDBError = new ServiceErrorCode() { ErrorCode = 31004, HttpError = HttpErrorCode.HttpInternal, Message = Strings.SqlServerErrorMessageLocalDBError };
        public static readonly ServiceErrorCode MSSqlInvalidPlan = new ServiceErrorCode() { ErrorCode = 31005, HttpError = HttpErrorCode.HttpInternal, Message = Strings.SqlServerErrorMessageInvalidPlan };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MSSqlError(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            base(serviceErrorCode, httpError, message, formattingParameters)
        {

        }

        public MSSqlError(ServiceErrorCode errorCode)
            : base(errorCode)
        {
        }

        public MSSqlError(ServiceErrorCode errorCode, params string[] arguments)
            : base(errorCode, arguments)
        {
        }
    }
}