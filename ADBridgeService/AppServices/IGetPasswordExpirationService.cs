using ADBridgeService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADBridgeService.AppServices
{
    /// <summary>
    /// Сервис получения срока действия пароля для пользователя AD
    /// </summary>
    public interface IGetPasswordExpirationService
    {
        /// <summary>
        /// Получает сведения о сроке действия пароля для доменного пользователя
        /// Возвращает null, если пользователь не найден
        /// </summary>
        PasswordExpirationDTO GetDomainUserPasswordExpirationDateOrNull(string userDomainAccount);
    }
}
