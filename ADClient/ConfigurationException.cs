using System;
using System.Collections.Generic;
using System.Text;

namespace ADClient
{
    public class ConfigurationException : InvalidOperationException
    {
        public ConfigurationException(string message)
            : base(message)
        {
        }
    }
}
