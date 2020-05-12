using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;

namespace ADClient
{
    internal class ADUsersRepository : IADUsersRepository
    {
        public ADUsersRepository(
            PrincipalContext ad,
            ADConnectionConfig connectionConfig,
            ILogger logger
        )
        {
            this.ad = ad;
            this.connectionConfig = connectionConfig;
            this.logger = logger;
        }

        public ADUserWithPassword GetUserPasswordDetails(string account)
        {
            try
            {
                // Здесь при расширении условий поиска будем использовать спецификацию
                var userPrincipal = FindUserPrincipalOrNull(ad, account);
                if (userPrincipal == null)
                {
                    LogUserNotFound(account);
                    return null;
                }

                return new ADUserWithPassword(userPrincipal);
            }
            catch (Exception ex)
            {
                LogFindUserError(account, ex);
                // любые ошибки, которые здесь могут возникнуть -- это ошибки подключения к AD
                throw new ConnectionException(
                    string.Format("ADUsers. Ошибка поиска пользователя {0} в AD ({1}",
                        account,
                        connectionConfig.ToString()
                    ),
                    ex
                );
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            this.ad.Dispose();
            this.disposed = true;

            GC.SuppressFinalize(this);
        }

        private UserPrincipal FindUserPrincipalOrNull(PrincipalContext ad, string account)
        {
            var byAccount = new UserPrincipal(ad);
            byAccount.SamAccountName = account;

            using var principalSearcher = new PrincipalSearcher(byAccount);
            // заставляем догрузить свойство для срока действия пароля, чтобы воспользоваться маршалингом
            // это невозможно, если используется локальная машина (хранилище SAM)
            if (ad.ContextType != ContextType.Machine)
            {
                var directorySearcher = principalSearcher.GetUnderlyingSearcher() as DirectorySearcher;
                directorySearcher.PropertiesToLoad.Add(ADUserWithPassword.GetPasswordExpiryPropertyName());
            }

            return principalSearcher.FindOne() as UserPrincipal;
        }

        private void LogUserNotFound(string account)
        {
            this.logger.LogInformation("ADUsers. Пользователь с учетной записью {account} не найден в хранилище AD {connection}",
                account,
                connectionConfig.ToString()
            );
        }

        private void LogFindUserError(string account, Exception ex)
        {
            this.connectionConfig.LogConfigForError(
                this.logger,
                ex,
                string.Format("ADUsers. Ошибка поиска пользователя {0}", account)
            );
        }

        private readonly PrincipalContext ad;
        private readonly ADConnectionConfig connectionConfig;
        private readonly ILogger logger;

        private bool disposed = false;
    }
}
