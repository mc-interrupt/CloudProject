using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Amazon.ElasticMapReduce;
using Amazon.ElasticMapReduce.Model;

using WebAPI.Model.OptionBindings;

namespace WebAPI.Model.Routines
{
    public struct RunJobConfiguration   
    {
        public string ClusterName { get; set; }
        public string ReleaseLabel { get; set; }
        public IEnumerable<Amazon.ElasticMapReduce.Model.Application> Applications { get; set; }
        public string EC2KeyName { get; set; }
        public int InstanceCount { get; set; }
        public bool KeepJobAlive { get; set; }
        public string MasterInstanceType { get; set; }
        public string SlaveInstanceType { get; set; }
        public string ServiceRole { get; set; }
        public string JobFlowRole { get; set; }
        public IEnumerable<Amazon.ElasticMapReduce.Model.BootstrapActionConfig> BootstrapActions { get; set; }
        public IEnumerable<Amazon.ElasticMapReduce.Model.StepConfig> StepConfigurations { get; set; }
        public string LogUri { get; set; }
    }

    public static class EMR
    {
        public static async Task<ListClustersResponse> ListJobs(IAmazonElasticMapReduce emrClient)
        {
            return await emrClient.ListClustersAsync();
        }
        public static async Task<RunJobFlowResponse> RunJob(IAmazonElasticMapReduce emrClient, JobConfiguration configuration)
        {
            RunJobFlowRequest request = new RunJobFlowRequest();

            request.Name = configuration.ClusterName;
            request.ReleaseLabel = configuration.ReleaseLabel;
            request.Applications = configuration.Applications.Select(app => new Amazon.ElasticMapReduce.Model.Application() { Name = app.Name }).ToList();

            request.Instances = new JobFlowInstancesConfig();
            request.Instances.Ec2KeyName = configuration.EC2KeyName;
            request.Instances.InstanceCount = configuration.InstanceCount;
            request.Instances.KeepJobFlowAliveWhenNoSteps = configuration.KeepJobAlive;
            request.Instances.MasterInstanceType = configuration.MasterInstanceType;
            request.Instances.SlaveInstanceType = configuration.SlaveInstanceType;

            request.ServiceRole = configuration.ServiceRole;
            request.JobFlowRole = configuration.JobFlowRole;

            request.BootstrapActions = configuration.BootstrapActions.Select(bootstrap => new Amazon.ElasticMapReduce.Model.BootstrapActionConfig()
                                        {
                                            Name = bootstrap.Name,
                                            ScriptBootstrapAction = new ScriptBootstrapActionConfig()
                                            {
                                                Path = bootstrap.ScriptBootstrapAction.Path,
                                                Args = new List<string>(bootstrap.ScriptBootstrapAction.Args)
                                            }
                                        }).ToList();

            request.Steps = configuration.StepConfigurations.Select(s => new Amazon.ElasticMapReduce.Model.StepConfig()
                                                            {
                                                                Name = s.Name,
                                                                ActionOnFailure = ParseActionOnFailure(s.ActionOnFailure),
                                                                HadoopJarStep = new HadoopJarStepConfig()
                                                                {
                                                                    Jar = s.HadoopJarStep.Jar,
                                                                    Args = new List<string>(s.HadoopJarStep.Args)
                                                                }
                                                            }).ToList();


            request.LogUri = configuration.LogUri;

            return await emrClient.RunJobFlowAsync(request);
        }

        static ActionOnFailure ParseActionOnFailure(string value)
        {
            const string CONTINUE = "CONTINUE";
            const string CANCEL_AND_WAIT = "CANCEL_AND_WAIT";
            const string TERMINATE_JOB_FLOW = "TERMINATE_JOB_FLOW";
            const string TERMINATE_CLUSTER = "TERMINATE_CLUSTER";

            switch (value)
            {
                case CONTINUE:
                    return ActionOnFailure.CONTINUE;

                case CANCEL_AND_WAIT:
                    return ActionOnFailure.CANCEL_AND_WAIT;

                case TERMINATE_JOB_FLOW:
                    return ActionOnFailure.TERMINATE_JOB_FLOW;

                case TERMINATE_CLUSTER:
                    return ActionOnFailure.TERMINATE_CLUSTER;

                default:
                    return null;
            }
        }
    }
}