using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Text;
using System.Threading;

namespace ADClient
{
    /// <summary>
    /// Хранилище AD создает классы репозиториев, предоставляя им сервисы для подключения к AD
    /// Репозитории не надо держать в контейнере или диспозить, зависимостью всегда имеем хранилище
    /// TODO:
    /// - возможное кэширование подключений
    /// - использование polly для ретрай-политик
    /// </summary>
    public class ADStore : IADStore
    {
        public ADStore(
            ILogger<ADStore> logger
        )
        {
            this.logger = logger;
            this.storeConnector = new PrincipalContextConnector();
        }

        public IADUsersRepository NewUsersRepositoryOrThrow(ADConnectionConfig connectionConfig)
        {
            var ad = BindToADOrThrow(connectionConfig);

            return new ADUsersRepository(
                ad,
                connectionConfig,
                this.logger
            );
        }

        internal PrincipalContext BindToADOrThrow(ADConnectionConfig connectionConfig)
        {
            // TODO: поискать результат в кэше
            // TODO: подключить polly
            try
            {
                var principalContext = this.storeConnector.BindToAD(connectionConfig);
                return principalContext;
            }
            catch (Exception ex)
            {
                connectionConfig.LogConfigForError(
                    this.logger, 
                    ex, 
                    "ADConnection. Ошибка подключения к AD"
                );
                throw new ConnectionException(
                    string.Format("ADConnection. Ошибка подключения к AD ({0}): {0}",
                    connectionConfig.ToString(),
                    ex.Message
                    ),
                    ex
                );

            }
        }

        private readonly PrincipalContextConnector storeConnector;
        private readonly ILogger<ADStore> logger;
    }
}
