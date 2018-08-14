using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using WebAPI.Model.Routines;
using WebAPI.Model.Services;
using WebAPI.Model.OptionBindings;

using Amazon.EC2;
using Amazon.EC2.Model;

namespace WebAPI.Controllers
{
    public class DataPreparationController : Controller
    {
        const int INSTANCE_STATE_CODE_PENDING = 0;
        const int INSTANCE_STATE_CODE_RUNNING = 16;

        ITaskConfigurationService TaskConfigurationService { get; set; }
        IAmazonEC2 EC2Client { get; set; }

        public DataPreparationController(IAmazonEC2 ec2Client, ITaskConfigurationService taskConfigurationService)
        {
            TaskConfigurationService = taskConfigurationService;
            EC2Client = ec2Client;
        }

        [HttpGet]
        public async Task<Result> StartDataServer()
        {
            if (EC2Client == null || TaskConfigurationService == null)
                return new Result() { ErrorCode = Constants.ErrorCodes.FAILURE };

            InstanceConfiguration instanceConfiguration = TaskConfigurationService.GetDataPreparationConfiguration();

            var runInstanceResponse = await Instances.LaunchInstance(EC2Client, instanceConfiguration);
            var instanceId = runInstanceResponse.Reservation.Instances[0].InstanceId;

            bool isInstanceRunning = false;
            DescribeInstanceStatusResponse describeInstanceStatusResponse;

            GetInstanceStatusRequestConfiguration instanceStatusReqConfig = new GetInstanceStatusRequestConfiguration()
            {
                InstanceIds = new List<string>() { instanceId },
                IncludeAllInstances = true
            };

            // timeout?
            while (!isInstanceRunning)
            {
                describeInstanceStatusResponse = await Instances.GetInstanceStatus(EC2Client, instanceStatusReqConfig);

                var instance = describeInstanceStatusResponse.InstanceStatuses.Where(i => i.InstanceId == instanceId)
                                                                              .FirstOrDefault();

                if (instance != null)
                {
                    switch (instance.InstanceState.Code)
                    {
                        case INSTANCE_STATE_CODE_PENDING:
                            continue;
                        case INSTANCE_STATE_CODE_RUNNING:
                            isInstanceRunning = true;
                            break;
                        default:
                            throw new System.Exception("Instance stopped.");
                    }
                }
                else
                {
                    throw new System.Exception("Instance not found.");
                }
            }

            return new Result { ErrorCode = Constants.ErrorCodes.OK };
        }

        public async Task<DescribeInstancesResult> ListDataServers()
        {
            GetInstancesRequestConfiguration reqConfiguration = new GetInstancesRequestConfiguration()
            {
                Filters = new List<Filter>()
                {
                    new Filter("instance-state-name", new List<string>() { "pending", "running", "shutting-down", "stopping", "stopped" }),
                    new Filter($"tag:Name", new List<string>() { Constants.EC2.EC2_INSTANCE_DATA_PREPARATION_TAG })
                },
                MaxInstances = 10
            };

            try
            {
                return await Instances.GetInstances(EC2Client, reqConfiguration);
            }
            catch { }

            return new DescribeInstancesResult(new Result() {ErrorCode = Constants.ErrorCodes.FAILURE }, new List<Model.Routines.Instance>());
        }
    }
}