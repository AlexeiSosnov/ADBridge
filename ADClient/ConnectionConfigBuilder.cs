using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    internal class ConnectionConfigBuilder
    {
        internal ConnectionConfigBuilder()
        {
        }

        internal ADConnectionConfig BuildCheckedOrThrow(IConfiguration appConfiguration)
        {
            var config = appConfiguration.GetSection("ADConnection").Get<ADConnectionConfig>();
            CheckConnectionConfigOrThrow(config);

            return config;
        }

        /// <summary>
        /// Получает конфигурацию подключения по названию домена. Подсекция конфигурации в DomainConnections 
        /// должна совпадать по названию с названием домена (вплоть до реестра), переменные окружения должны включить название домена
        /// </summary>
        internal ADConnectionConfig BuildCheckedForDomainOrThrow(IConfiguration appConfiguration, string domain)
        {
            var config = new ADConnectionConfig();
            config.Domain = GetDomainStringValue("Domain");
            config.DcAddress = GetDomainStringValue("DcAddress");
            config.DcUserName = GetDomainStringValue("DcUserName");
            config.DcPassword = GetDomainStringValue("DcPassword");
            config.UsersContainer = GetDomainStringValue("UsersContainer");

            CheckConnectionConfigOrThrow(config);
            return config;

            string GetDomainStringValue(string key)
            {
                return appConfiguration.GetValue<string>(GetDomainConfigKey(key));
            }

            string GetDomainConfigKey(string key)
            {
                return string.Format("DomainConnections:{0}:{1}", domain, key);
            }
        }

        internal void CheckConnectionConfigOrThrow(ADConnectionConfig config)
        {
            if (config == null)
            {
                throw new ConfigurationException("ADConnection. Не задана конфигурация подключения к AD. Необходимо создать секцию ADConnection или указать переменные окружения");
            }

            // если указана локальная машина, не должно быть опций подключения к домену
            if (config.IsLocalMachine)
            {
                CheckConfigForLocalMachineOrThrow(config);
                return;
            }

            if (!string.IsNullOrEmpty(config.DcAddress))
            {
                CheckConfigForDcOrThrow(config);
                return;
            }

            if (!string.IsNullOrEmpty(config.Domain))
            {
                CheckConfigForDomainOrThrow(config);
                return;
            }

            CheckIsEmptyOrThrow(config);
        }

        private void CheckIsEmptyOrThrow(ADConnectionConfig config)
        {
            // должен быть указан хотя бы один вариант подключения к хранилищу AD
            if (!config.IsLocalMachine
                && string.IsNullOrEmpty(config.Domain)
                && string.IsNullOrEmpty(config.DcAddress)
                )
            {
                throw new ConfigurationException("ADConnection. Некорректная конфигурация подключения к AD. В секции ADConnection необходимо указать один из параметров IsLocalMachine, Domain, DcAddress");
            }
        }

        private void CheckConfigForDcOrThrow(ADConnectionConfig config)
        {
            // защита от случайных пробелов
            if (config.DcAddress.Trim() == string.Empty)
            {
                throw new ConfigurationException("ADConnection. Адрес контроллера домена не может быть пустым. Необходимо указать непустой параметр DcAddress");
            }

            // при подключении к DC, если задан логин, то должен быть указан и пароль
            if (!string.IsNullOrEmpty(config.DcUserName)
                && string.IsNullOrEmpty(config.DcPassword))
            {
                throw new ConfigurationException(
                    string.Format("ADConnection. Для подключения к контоллеру домена {0} необходимо указать пароль пользователя {1} в параметре DcPassword. Пароль можно указать в переменной окружения",
                    config.DcAddress,
                    config.DcUserName
                    )
                );
            }
        }

        private void CheckConfigForDomainOrThrow(ADConnectionConfig config)
        {
            // защита от случайных пробелов
            if (config.Domain.Trim() == string.Empty)
            {
                throw new ConfigurationException("ADConnection. Название домена для подключения не может быть пустым. Необходимо указать непустой параметр Domain");
            }

            if (!string.IsNullOrEmpty(config.MachineName))
            {
                throw new ConfigurationException(
                    string.Format("ADConnection. Используется домен {0}, подключение к хранилищу SAM на машине {1} невозможно. Необходимо удалить параметр LocalMachine",
                    config.Domain,
                    config.MachineName
                    )
                );
            }

            // если указаны логин или пароль для подключения к DC, скорее всего опечатка в названии параметра конфигурации DcAddress
            if (!string.IsNullOrEmpty(config.DcUserName) || !string.IsNullOrEmpty(config.DcPassword))
            {
                throw new ConfigurationException(
                    string.Format("ADConnection. Используется домен {0}, указаны логин или пароль для подключения к DС. Возможно, неправильно указан параметр DcAddress адреса контроллера",
                    config.Domain
                    )
                );
            }
        }

        private void CheckConfigForLocalMachineOrThrow(ADConnectionConfig config)
        {
            if (!string.IsNullOrEmpty(config.Domain))
            {
                throw new ConfigurationException(
                    string.Format("ADConnection. Подключение к домену {0} не поддерживается при использовании хранилища SAM на локальной машине. Необходимо очистить параметр Domain",
                    config.Domain
                    )
                );
            }
        }
    }
}
