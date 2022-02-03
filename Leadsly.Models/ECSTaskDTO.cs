using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class ECSTaskDTO
    {
        /// <summary>
        /// The Amazon Resource Name (ARN) of the task.
        /// </summary>       
        
        public string TaskArn { get; set; }

        /// <summary>
        /// The ARN of the task definition that creates the task.
        /// </summary>
        public string TaskDefinitionArn { get; set; }
                
        public string CPU { get; set; }
        /// <summary>
        /// The health status for the task. It's determined by the health of the essential containers in the task. 
        /// If all essential containers in the task are reporting as HEALTHY, the task status also reports as HEALTHY. 
        /// If any essential containers in the task are reporting as UNHEALTHY or UNKNOWN, the task status also reports as UNHEALTHY or UNKNOWN.
        /// Valid Values: HEALTHY | UNHEALTHY | UNKNOWN
        /// </summary>
        public string HealthStatus { get; set; }
        /// <summary>
        /// The connectivity status of a task. 
        /// Valid Values: CONNECTED | DISCONNECTED
        /// </summary>
        public string Connectivity { get; set; }
        /// <summary>
        /// The connectivity status of a task.
        /// Valid Values: CONNECTED | DISCONNECTED
        /// </summary>
        public long CreatedAt { get; set; }
        /// <summary>
        /// The infrastructure where your task runs on. For more information, see Amazon ECS launch types in the Amazon Elastic Container Service Developer Guide.
        /// </summary>
        public string LaunchType { get; set; }
        /// <summary>
        /// The stop code indicating why a task was stopped. The stoppedReason might contain additional details.
        /// Valid Values: EC2 | FARGATE | EXTERNAL
        /// </summary>
        public ECSTaskStopCodes StopCode { get; set; }
        /// <summary>
        /// The reason that the task was stopped.
        /// </summary>
        public string StoppedReason { get; set; }
        /// <summary>
        /// The ARN of the cluster that hosts the task.
        /// </summary>
        public string ClusterArn { get; set; }
        /// <summary>
        /// The Unix timestamp for the time when the task last went into CONNECTED status.
        /// </summary>
        public long ConnectivityAt { get; set; }
        /// <summary>
        /// The ARN of the container instances that host the task
        /// </summary>
        public string ContainerInstanceArn { get; set; }

        /// <summary>
        /// The containers that's associated with the task.
        /// </summary>
        public List<ContainerDTO> Containers { get; set; }

        public enum ECSTaskStopCodes
        {
            None,
            TaskFailedToStart,
            EssentialContainerExited,
            UserInitiated,
            TerminationNotice,
            ServiceSchedulerInitiated,
            SpotInterruption
        }
    }
}
