using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace Uhuru.CloudFoundry.UI
{
    public class CloudTarget
    {
        string username;
        string encryptedPassword;
        string targetUrl;
        Guid targetId;


        public CloudTarget(string username, SecureString password, string targetUrl)
        {
            CloudCredentialsEncryption encryptor = new CloudCredentialsEncryption();

            this.username = username;
            this.encryptedPassword =  encryptor.Encrypt(password);
            this.targetUrl = targetUrl;
            this.targetId = Guid.NewGuid();
        }

        internal CloudTarget(string username, string encryptedPassword, string targetUrl, Guid targetId)
        {
            this.username = username;
            this.encryptedPassword = encryptedPassword;
            this.targetUrl = targetUrl;
            this.targetId = targetId;
        }

        public string Username
        {
            get { return username; }
        }

        public SecureString Password
        {
            get 
            {
                CloudCredentialsEncryption encryptor = new CloudCredentialsEncryption();
                return encryptor.Decrypt(encryptedPassword); 
            }
        }

        public string EncryptedPassword
        {
            get 
            { 
                return encryptedPassword; 
            }
        }

        public string TargetUrl
        {
            get { return targetUrl; }
        }

        public Guid TargetId
        {
            get { return targetId; }
            set { targetId = value; }
        }

        public string DisplayName
        {
            get 
            {
                return TargetUrl + "/" + Username;
            }
        }
    }
}
