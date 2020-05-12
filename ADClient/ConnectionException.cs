using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    public class ConnectionException : InvalidOperationException
    {
        public ConnectionException(
            string message,
            Exception innerConnectionException
        )
            : base(message, innerConnectionException)
        {
        }
    }
}
