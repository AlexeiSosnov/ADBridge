using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Text;

namespace ADClient
{
    /// <summary>
    /// Коннектор к AD через PrincipalContex (DirectoryServices.AcccountManagement)
    /// Можно будет представить коннектор и через DirectoryEntry
    /// </summary>
    internal class PrincipalContextConnector
    {
        internal PrincipalContext BindToAD(ADConnectionConfig connectionConfig)
        {
            if (connectionConfig.IsLocalMachine)
            {
                return BindToSAM(connectionConfig);
            }

            if (!string.IsNullOrEmpty(connectionConfig.Domain)
                && string.IsNullOrEmpty(connectionConfig.DcAddress))
            {
                return BindToCurrentDc(connectionConfig);
            }

            return BindToDc(connectionConfig);
        }

        private PrincipalContext BindToSAM(ADConnectionConfig connectionConfig)
        {
            string machineName = LOCAL_MACHINE;
            if (!string.IsNullOrEmpty(connectionConfig.MachineName))
            {
                machineName = connectionConfig.MachineName;
            }

            return new PrincipalContext(ContextType.Machine, machineName);
        }

        private PrincipalContext BindToCurrentDc(ADConnectionConfig connectionConfig)
        {
            if (string.IsNullOrEmpty(connectionConfig.UsersContainer))
            {
                return new PrincipalContext(ContextType.Domain, connectionConfig.Domain);
            }

            // если задан контейнер для пользователей, подставим его в качестве корневого узла
            return new PrincipalContext(
                ContextType.Domain,
                connectionConfig.Domain,
                container: connectionConfig.UsersContainer
            );
        }

        private PrincipalContext BindToDc(ADConnectionConfig connectionConfig)
        {
            // возможно, здесь нужен application directory
            var contextType = ContextType.Domain;
            if (string.IsNullOrEmpty(connectionConfig.UsersContainer))
            {
                return new PrincipalContext(
                    contextType,
                    name: connectionConfig.DcAddress,
                    userName: connectionConfig.DcUserName,
                    password: connectionConfig.DcPassword
                );
            }

            return new PrincipalContext(
                contextType,
                name: connectionConfig.DcAddress,
                container: connectionConfig.UsersContainer,
                userName: connectionConfig.DcUserName,
                password: connectionConfig.DcPassword
            );
        }

        // Для PrincipalContext, 
        private const string LOCAL_MACHINE = null;
    }
}
