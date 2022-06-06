using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.OptionsJsonModels
{
    public class DatabaseConnections
    {
        public IAMAuth IAMAuth { get; set; }
        public AuthCredentials AuthCredentials { get; set; }
    }

    public class IAMAuth
    {
        public string Database { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }
    }

    public class AuthCredentials 
    {
        public string AwsRegion { get; set; }
        public string Key { get; set; }
    }
}
