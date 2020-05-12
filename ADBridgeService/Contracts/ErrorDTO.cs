using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ADBridgeService.Contracts
{
    /// <summary>
    /// Простое описание ошибки для возврата вместе с ошибочными статусами
    /// </summary>
    [DataContract]
    [Serializable]
    public class ErrorDTO
    {
        public ErrorDTO(string error)
        {
            this.Error = error;
        }

        [DataMember]
        public string Error { get; set; }
    }
}
