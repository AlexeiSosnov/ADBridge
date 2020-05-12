using ADClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADBridgeService.AppServices
{
    internal class UserInDomain
    {
        internal static UserInDomain FromAccount(string account)
        {
            // 1. пробуем форму domain\username
            var domainParts = account.Split('\\');
            if (domainParts.Length == 2)
            {
                return new UserInDomain(domainParts[0], domainParts[1]);
            }

            // 2. пробуем форму upn username@domain
            var upnParts = account.Split('@');
            if (upnParts.Length == 2)
            {
                return new UserInDomain(upnParts[0], upnParts[1]);
            }


            return new UserInDomain(string.Empty, account);
        }

        private UserInDomain(
            string domain,
            string username
        )
        {
            this.domain = domain;
            this.username = username;
        }

        public string GetUsername()
        {
            return this.username;
        }

        public ADConnectionConfig GetADContainerConnectionConfigOrThrow(IConfiguration appConfiguration)
        {
            if (string.IsNullOrEmpty(this.domain))
            {
                return ADConnectionConfig.FromConfigurationOrThrow(appConfiguration);
            }

            return ADConnectionConfig.FromConfigurationForDomainOrThrow(appConfiguration, this.domain);
        }

        private readonly string domain;
        private readonly string username;
    }
}
