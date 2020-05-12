using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    /// <summary>
    /// Хранилище AD, фабрика репозиториев пользователей
    /// </summary>
    public interface IADStore
    {
        /// <summary>
        /// Создает репозиторий пользователей AD с заданными опциями подключения к хранилищу AD
        /// </summary>
        IADUsersRepository NewUsersRepositoryOrThrow(ADConnectionConfig connectionConfig);
    }
}
