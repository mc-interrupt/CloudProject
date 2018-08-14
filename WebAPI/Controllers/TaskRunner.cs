using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WebAPI.Model.Routines;
using WebAPI.Model.Services;
using WebAPI.Model.OptionBindings;

using Amazon.ElasticMapReduce;
using Amazon.ElasticMapReduce.Model;
using System;

namespace WebAPI.Controllers
{
    public class TaskRunnerController : Controller
    {
        IAmazonElasticMapReduce EMRClient { get; }
        ITaskConfigurationService TaskConfigurationService { get; }


        public TaskRunnerController(IAmazonElasticMapReduce emrClient, ITaskConfigurationService taskConfigurationService)
        {
            EMRClient = emrClient;
            TaskConfigurationService = taskConfigurationService;
        }

        [HttpGet]
        public async Task<Result> RunTask(string groupId, string taskId)
        {
            JobConfiguration jobConfiguration = TaskConfigurationService.GetTaskConfiguration(groupId, taskId);

            try
            {
                await EMR.RunJob(EMRClient, jobConfiguration);
                return new Result() { ErrorCode = Constants.ErrorCodes.OK };
            }
            catch (Exception exception) { }

            return new Result() { ErrorCode = Constants.ErrorCodes.FAILURE };
        }
    }
}