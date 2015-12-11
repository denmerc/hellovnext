using APLPX.Entity;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using DB = APLPX.Client.Postgres.Models;
using ENT = APLPX.Entity;
using System.Configuration;
using Newtonsoft.Json;
using APLPX.Client.Contracts;
using System.Security.Cryptography;
using System.Text;
using System.Security.Authentication;
using APLPX.Client.Postgres.Helpers;

namespace APLPX.Client.Postgres
{
    public class NotificationService : INotificationDataService
    {
        private readonly string _smtpHost = "";
        private readonly string _smtpUser = "";
        private readonly string _smtpPassword = "";
        private const string _smtpDefaultReply = "donotreply@priceXpert.com";
        private readonly int _smtpPort;
        private readonly bool _isSSLEnabled = false;
        private readonly bool _sendEmail = false;
        private OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        public NotificationService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            GlobalParameters globalParams = new GlobalParameters();
            _smtpHost = globalParams.GetValue(GlobalParameters.PARAM_SMTPServer);
            _smtpPort = Convert.ToInt32(globalParams.GetValue(GlobalParameters.PARAM_SMTPPort));
            _isSSLEnabled = globalParams.GetValue(GlobalParameters.PARAM_SMTPSSLEnabled).Equals("Y");
            _sendEmail = globalParams.GetValue(GlobalParameters.PARAM_LoadSendEmail).Equals("Y");
            if (_sendEmail)
            {
                _smtpUser = globalParams.GetValue(GlobalParameters.PARAM_SMTPUser);
                _smtpPassword = globalParams.GetValue(GlobalParameters.PARAM_SMTPUserPassword);
            }

        }

        public void SendEmail(List<string> toAddresses, string subject, string body, bool isHtml)
        {
            try
            {
                foreach (var addy in toAddresses)
                {
                    Email.Send(_smtpDefaultReply, addy, subject, body, isHtml, _smtpHost, _smtpPort, _smtpUser, _smtpPassword, _isSSLEnabled);                  
                }
            }
            catch (Exception)
            {              
                throw;
            }
        }

        public void SendEmail(UserRoleType role, string subject, string body, bool isHtml)
        {
            try
            {
                //foreach (var user in Users.Role)
                //{
                //    Email.Send(_smtpDefaultReply, addy, subject, body, isHtml, _smtpHost, _smtpPort, _smtpUser, _smtpPassword, _isSSLEnabled);
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}
