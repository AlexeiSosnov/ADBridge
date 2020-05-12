using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.CompilerServices;
using System.Text;

namespace ADClient
{
    /// <summary>
    /// Сведения о пароле пользователя в AD
    /// </summary>
    public class ADUserWithPassword
    {
        internal ADUserWithPassword(
            UserPrincipal userInStore
        )
        {
            // сбор всех значений в конструкторе позволит избежать ошибок доступа к разорванному соединению,
            // если объект сведений о пароле будет использоваться вне скоупа соединения с AD 
            this.accountName = userInStore.SamAccountName;
            this.distinguishedName = userInStore.DistinguishedName;

            var directoryEntry = userInStore.GetUnderlyingObject() as DirectoryEntry;
            this.rawPasswordExpirationDate = GetRawPasswordExpirationDate(directoryEntry);
            // если не задано значение атрибута AD, воспользуемся вычислением через COM
            if (!this.rawPasswordExpirationDate.HasValue)
            {
                this.nativePasswordExpirationDate = GetNativePasswordExpirationDate(directoryEntry);
            }

            // пароль не протухает, если явно задан атрибут Password never expires, или недоступен
            // атрибут msDS-UserPasswordExpiryTimeComputed со сроком действия пароля,
            // или на уровне доменной политики установлен бесконечный срок действия пароля
            this.passwordNeverExpires = userInStore.PasswordNeverExpires
                || this.rawPasswordExpirationDate == long.MaxValue
                || IsNativeExpirationDateNeverExpires();
        }

        public virtual string GetAccountName()
        {
            return this.accountName;
        }

        public virtual string GetDistinguishedName()
        {
            return this.distinguishedName;
        }

        public bool IsPasswordNeverExpires()
        {
            return this.passwordNeverExpires;
        }

        public DateTime? GetPasswordExpirationDate()
        {
            if (IsPasswordNeverExpires())
            {
                return null;
            }

            // если мы получили значение срока действия пароля через COM-интерфейс, его и вернем
            if (this.nativePasswordExpirationDate.HasValue)
            {
                // для перестраховки: если атрибут равен 1 января 1970 года, пароль считается непротухающим
                if (IsNativeExpirationDateNeverExpires())
                {
                    return null;
                }

                return this.nativePasswordExpirationDate;
            }


            // https://docs.microsoft.com/ru-ru/openspecs/windows_protocols/ms-adts/f9e9b7e2-c7ac-4db6-ba38-71d9696981e9
            // если значение pwdLastSet = 0 или null, атрибут msDS-UsedPasswordExpiryTimeComputed = 0, это означает, что пользователь
            // должен немедленно сменить пароль
            if (this.rawPasswordExpirationDate == 0)
            {
                return DateTime.MinValue;
            }

            // если на уровне доменной политики задан непротухающий пароль, значение атрибута будет равно long.MaxValue
            // тогда считаем, что срок протухания пароля не задан
            if (this.rawPasswordExpirationDate == long.MaxValue)
            {
                return null;
            }

            // иначе это время в формате FileTime, см. https://www.informit.com/articles/article.aspx?p=474649&seqNum=3
            return DateTime.FromFileTime(this.rawPasswordExpirationDate.Value);
        }

        private static long? GetRawPasswordExpirationDate(DirectoryEntry directoryEntry)
        {
            var oValues = directoryEntry.Properties[GetPasswordExpiryPropertyName()];
            if (oValues == null || oValues.Count == 0)
            {
                return null;
            }

            return (long)oValues[0];
        }

        private static DateTime? GetNativePasswordExpirationDate(DirectoryEntry directoryEntry)
        {
            // получение через InvokeGet гарантированно работает, даже если срок действия пароля
            // устанавливается на уровне групповой политики 
            return (DateTime)directoryEntry.InvokeGet("PasswordExpirationDate");
        }
        
        private bool IsNativeExpirationDateNeverExpires()
        {
            // если пароль не протухает, PasswordExpirationDate возвращает 01.01.1970
            // эта дата возвращается с типом Unspecified, поэтому надо привести его к UTC
            return this.nativePasswordExpirationDate.HasValue
                && new DateTimeOffset(DateTime.SpecifyKind(this.nativePasswordExpirationDate.Value, DateTimeKind.Utc)).ToUnixTimeMilliseconds() == 0;
        }

        internal static string GetPasswordExpiryPropertyName()
        {
            return "msDS-UserPasswordExpiryTimeComputed";
        }

        private readonly string accountName;
        private readonly string distinguishedName;
        private readonly bool passwordNeverExpires;
        private readonly long? rawPasswordExpirationDate;
        private readonly DateTime? nativePasswordExpirationDate;
    }
}
