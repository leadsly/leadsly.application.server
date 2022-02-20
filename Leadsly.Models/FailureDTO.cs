using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class FailureDTO
    {
        public Codes? Code { get; set; }
        public string ResourceId { get; set; }             
        public string Arn { get; set; }        
        public string Detail { get; set; }        
        public string Reason { get; set; }
    }

    public enum Codes
    {
        NONE,
        CONFIGURATION_DATA_MISSING,
        OBJECT_MAPPING,
        HEALTHCHECK_FAILURE,
        AWS_API_ERROR,
        AWS_API_RESPONSE_MISSING_DATA,
        AWS_CLOUD_UNEXPECTED_STATE,
        DATABASE_OPERATION_ERROR,
        ERROR
    }
}
