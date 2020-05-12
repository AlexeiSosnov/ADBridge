using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    /// <summary>
    /// Репозиторий пользователей в AD
    /// Репозиторий необходимо диспозить после получения данных о пользователях или их изменении
    /// </summary>
    public interface IADUsersRepository : IDisposable
    {
        /// <summary>
        /// Загружает сведения о пароле пользователя (дату протухания, 
        /// дату последнего изменения и т.п.) из AD
        /// </summary>
        ADUserWithPassword GetUserPasswordDetails(string account);
    }
}
