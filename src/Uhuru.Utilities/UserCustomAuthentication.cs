// -----------------------------------------------------------------------
// <copyright file="UserCustomAuthentication.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Security;

    /// <summary>
    /// Class used for custom user/password authentication.
    /// This is used in the file server and the healthz/varz server.
    /// </summary>
    public class UserCustomAuthentication : UserNamePasswordValidator
    {
        /// <summary>
        /// The username that passes the validation check.
        /// </summary>
        private string validUsername;

        /// <summary>
        /// The password that passes the validation check.
        /// </summary>
        private string validPassword;

        /// <summary>
        /// Initializes a new instance of the UserCustomAuthentication class
        /// </summary>
        /// <param name="userName">Username that is allowed access.</param>
        /// <param name="password">Password that is allowed access.</param>
        public UserCustomAuthentication(string userName, string password)
        {
            this.validPassword = password;
            this.validUsername = userName;
        }

        /// <summary>
        /// This method is called when a server needs to check if credentials are ok.
        /// </summary>
        /// <param name="userName">Username to verify.</param>
        /// <param name="password">Password to verify.</param>
        public override void Validate(string userName, string password)
        {
            if (null == userName)
            {
                throw new ArgumentNullException("userName");
            }

            if (null == password)
            {
                throw new ArgumentNullException("password");
            }

            if (!(userName == this.validUsername && password == this.validPassword))
            {
                throw new SecurityException("Unknown username or incorrect password");
            }
        }
    }
}
