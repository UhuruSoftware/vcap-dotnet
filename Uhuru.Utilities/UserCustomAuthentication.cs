using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace Uhuru.Utilities
{
    /// <summary>
    /// Class used for custom user/password authentication.
    /// This is used in the file server and the healthz/varz server.
    /// </summary>
    public class UserCustomAuthentication : UserNamePasswordValidator
    {

        string validUsername;
        string validPassword;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="userName">Username that is allowed access.</param>
        /// <param name="password">Password that is allowed access.</param>
        public UserCustomAuthentication(string userName, string password)
        {
            validPassword = password;
            validUsername = userName;
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

            if (!(userName == validUsername && password == validPassword))
            {
                throw new FaultException("Unknown Username or Incorrect Password");
            }
        }
    }
}
