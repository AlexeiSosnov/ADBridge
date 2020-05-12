using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ADBridgeService.Contracts
{
    /// <summary>
    /// Данные о протухании пароля. Здесь только учетка и время;
    /// условия повторной проверки времени протухания пароля (когда спрашивать в следущий раз)
    /// -- ответственность веб-сервера (MobNet или ИБ/МБ)
    /// </summary>
    [Serializable]
    [DataContract]
    public class PasswordExpirationDTO
    {
        /// <summary>
        /// Учетная запись пользователя
        /// </summary>
        [DataMember]
        public string Account { get; set; }

        /// <summary>
        /// Пароль никогда не протухает, в этом случае ExpirationTime не задан
        /// </summary>
        [DataMember]
        public bool NeverExpires { get; set; }

        /// <summary>
        /// Дата и время протухания пароля, не задано если NeverExpires
        /// </summary>
        [DataMember]
        public DateTime? ExpirationDate { get; set; }
    }
}
