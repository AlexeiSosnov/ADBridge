using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    /// <summary>
    /// Опции подключения к хранилищу AD
    /// </summary>
    public class ADConnectionConfig
    {
        public static ADConnectionConfig UseLocalMachine()
        {
            return new ADConnectionConfig
            {
                IsLocalMachine = true
            };
        }

        /// <summary>
        /// Опции подключения к текущему контроллеру домена
        /// </summary>
        public static ADConnectionConfig UseCurrentDc(string domainName, string optUsersContainer)
        {
            var config = new ADConnectionConfig
            {
                Domain = domainName
            };
            if (!string.IsNullOrEmpty(optUsersContainer))
            {
                config.UsersContainer = optUsersContainer;
            }

            return config;
        }

        public static ADConnectionConfig FromConfigurationOrThrow(IConfiguration appConfiguration)
        {
            var builder = new ConnectionConfigBuilder();
            return builder.BuildCheckedOrThrow(appConfiguration);
        }

        public static ADConnectionConfig FromConfigurationForDomainOrThrow(IConfiguration appConfiguration, string domain)
        {
            var builder = new ConnectionConfigBuilder();
            return builder.BuildCheckedForDomainOrThrow(appConfiguration, domain);
        }

        /// <summary>
        /// Признак использования хранилища SAM (на локальной машине или машине с заданным именем)
        /// </summary>
        public bool IsLocalMachine { get; set; }

        /// <summary>
        /// Название машины для подключения к SAM, используется только при заданном IsLocalMachine
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Название домена, не может использоваться вместе с LocalMachine
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Опционально, название контейнера с пользователями
        /// </summary>
        public string UsersContainer { get; set; }

        /// <summary>
        /// Опционально, адрес контроллера домена
        /// </summary>
        public string DcAddress { get; set; }

        /// <summary>
        /// Опционально, имя пользователя для подключения к контроллеру домена
        /// </summary>
        public string DcUserName { get; set; }

        /// <summary>
        /// Опционально, пароль для подключения к контроллеру домена 
        /// </summary>
        public string DcPassword { get; set; }

        internal void LogConfigForError(ILogger logger, string errorMessage)
        {
            logger.LogError(
                errorMessage + " " + "Параметры подключения: домен {Domain} / контроллер {DcAddress} (пользователь {DcUserName})",
                this.Domain, this.DcAddress, this.DcUserName
            );
        }

        internal void LogConfigForError(ILogger logger, Exception ex, string errorMessage)
        {
            logger.LogError(
                ex,
                errorMessage + " " + "Параметры подключения: домен {Domain} / контроллер {DcAddress} (пользователь {DcUserName})",
                this.Domain, this.DcAddress, this.DcUserName
            );
        }

        public override string ToString()
        {
            if (this.IsLocalMachine)
            {
                return string.Format("хранилище SAM ({0})",
                    !string.IsNullOrEmpty(this.MachineName) ? this.MachineName : "локальная машина"
                );
            }

            if (string.IsNullOrEmpty(this.DcAddress))
            {
                return string.Format("домен {0}, контейнер {1}",
                    this.Domain,
                    !string.IsNullOrEmpty(this.UsersContainer) ? this.UsersContainer : "root"
                );
            }

            return string.Format("контроллер {0} (пользователь {1}), домен {2}, контейнер {3}",
                this.DcAddress,
                this.DcUserName,
                this.Domain,
                !string.IsNullOrEmpty(this.UsersContainer) ? this.UsersContainer : "root"
            );
        }
    }
}
