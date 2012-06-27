// -----------------------------------------------------------------------
// <copyright file="FileServiceErrorException.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Runtime.Serialization;
    using Uhuru.CloudFoundry.ServiceBase;

    /// <summary>
    /// This is an exception class that is used when bad things happen in the MS SQL node.
    /// </summary>
    [Serializable]
    public class FileServiceErrorException : ServiceException
    {
        /// <summary>
        /// This is an error code used when disk is full.
        /// </summary>
        public static readonly ServiceErrorCode FileServiceDiskFull = new ServiceErrorCode() { ErrorCode = 31021, HttpError = HttpErrorCode.HttpInternal, Message = Strings.FileServiceErrorMessageDiskFull };

        /// <summary>
        /// This is an error code used when the configuration settings are not found.
        /// </summary>
        public static readonly ServiceErrorCode FileServiceConfigNotFound = new ServiceErrorCode() { ErrorCode = 31022, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.FileServiceErrorMessageConfigurationNotFound };

        /// <summary>
        /// This is an error code used when db credentials are not found.
        /// </summary>
        public static readonly ServiceErrorCode FileServiceCredentialsNotFound = new ServiceErrorCode() { ErrorCode = 31023, HttpError = HttpErrorCode.HttpNotFound, Message = Strings.FileServiceErrorMessageCredentialNotFound };

        /// <summary>
        /// This is an error code used when the local database of the node is not found.
        /// </summary>
        public static readonly ServiceErrorCode FileServiceLocalDBError = new ServiceErrorCode() { ErrorCode = 31002, HttpError = HttpErrorCode.HttpInternal, Message = Strings.FileServiceErrorMessageLocalDBError };

        /// <summary>
        /// This is an error code used when an invalid plan has been specified.
        /// </summary>
        public static readonly ServiceErrorCode FileServiceInvalidPlan = new ServiceErrorCode() { ErrorCode = 31025, HttpError = HttpErrorCode.HttpInternal, Message = Strings.FileServiceErrorMessageInvalidPlan };

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="serviceErrorCode">The service error code.</param>
        /// <param name="httpError">An HTTP error code.</param>
        /// <param name="message">The service error message.</param>
        /// <param name="formattingParameters">Formatting arguments for the message.</param>
        public FileServiceErrorException(int serviceErrorCode, HttpErrorCode httpError, string message, params string[] formattingParameters) :
            base(serviceErrorCode, httpError, message, formattingParameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public FileServiceErrorException(ServiceErrorCode errorCode)
            : base(errorCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="arguments">The arguments.</param>
        public FileServiceErrorException(ServiceErrorCode errorCode, params string[] arguments)
            : base(errorCode, arguments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        public FileServiceErrorException()
            : this(0, HttpErrorCode.None, "Uhuru File Service Node Error")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public FileServiceErrorException(string message)
            : this(0, HttpErrorCode.None, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public FileServiceErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceErrorException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="streamingContext">The streaming context.</param>
        protected FileServiceErrorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}