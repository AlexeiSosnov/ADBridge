using ADBridgeService.Contracts;
using ADClient;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADBridgeService.AppServices
{
    internal class GetPasswordExpirationService : IGetPasswordExpirationService
    {
        public GetPasswordExpirationService(
            IADStore adStore,
            IConfiguration appConfiguration,
            ILogger logger
        )
        {
            this.adStore = adStore;
            this.appConfiguration = appConfiguration;
            this.logger = logger.ForContext<GetPasswordExpirationService>();
        }

        public PasswordExpirationDTO GetDomainUserPasswordExpirationDateOrNull(string userDomainAccount)
        {
            using var logContext = LogContext.PushProperty("userDomainAccount", userDomainAccount);

            try
            {
                var connectionConfig = ADConnectionConfig.FromConfigurationOrThrow(this.appConfiguration);
                LogBeginGetPasswordDetails(userDomainAccount, connectionConfig);

                using var usersRepository = this.adStore.NewUsersRepositoryOrThrow(connectionConfig);
                var userWithPassword = usersRepository.GetUserPasswordDetails(userDomainAccount);
                if (userWithPassword == null)
                {
                    LogUserNotFound(userDomainAccount, connectionConfig);
                    return null;
                }

                LogUserIsFound(userDomainAccount, userWithPassword);
                return NewDTO(userWithPassword);
            }
            catch (ConfigurationException ex)
            {
                LogInvalidConfiguration(userDomainAccount, ex);
                throw;
            }
            catch (ConnectionException ex)
            {
                LogADConnectionFailed(userDomainAccount, ex);
                throw;
            }
            catch (Exception ex)
            {
                LogGetPasswordDetailsFailed(userDomainAccount, ex);
                throw;
            }
        }

        private PasswordExpirationDTO NewDTO(ADUserWithPassword userWithPassword)
        {
            return new PasswordExpirationDTO
            {
                Account = userWithPassword.GetAccountName(),
                NeverExpires = userWithPassword.IsPasswordNeverExpires(),
                ExpirationDate = userWithPassword.GetPasswordExpirationDate()
            };
        }

        private void LogBeginGetPasswordDetails(string userDomainAccount, ADConnectionConfig connectionConfig)
        {
            this.logger.Information(
                "Срок действия пароля для учетки {userDomainAccount}. Поиск пользователя в хранилище AD {connection}",
                userDomainAccount,
                connectionConfig.ToString()
            );
        }

        private void LogUserNotFound(string userDomainAccount, ADConnectionConfig connectionConfig)
        {
            this.logger.Error("Срок действия пароля для учетки {userDomainAccount}. Учетка не найдена в хранилище AD {connection}",
                userDomainAccount,
                connectionConfig.ToString()
            );
        }

        private void LogUserIsFound(string userDomainAccount, ADUserWithPassword userWithPassword)
        {
            this.logger.Information("Срок действия пароля для учетки {userDomainAccount}. Найден пользователь, логин {samAccountName}, полное имя {distinguishedName}",
                userDomainAccount,
                userWithPassword.GetAccountName(),
                userWithPassword.GetDistinguishedName()
            );
        }

        private void LogInvalidConfiguration(string userDomainAccount, ConfigurationException ex)
        {
            this.logger.Fatal(
                ex,
                "Срок действия пароля для учетки {userDomainAccount}. Некорректная конфигурация подключения к AD",
                userDomainAccount
            );
        }

        private void LogADConnectionFailed(string userDomainAccount, ConnectionException ex)
        {
            this.logger.Error(
                ex,
                "Срок действия пароля для учетки {userDomainAccount}. Ошибка подключения к AD",
                userDomainAccount
            );
        }

        private void LogGetPasswordDetailsFailed(string userDomainAccount, Exception ex)
        {
            this.logger.Error(
                ex,
                "Срок действия пароля для учетки {userDomainAccount}. Ошибка получения срока действия пароля",
                userDomainAccount
            );
        }

        private readonly IADStore adStore;
        private readonly IConfiguration appConfiguration;
        private readonly ILogger logger;

    }
}
