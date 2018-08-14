using WebAPI.Model.OptionBindings;
using Microsoft.Extensions.Configuration;

namespace WebAPI.Model.Services
{
    public interface ITaskConfigurationService
    {
        InstanceConfiguration GetDataPreparationConfiguration();
        JobConfiguration GetTaskConfiguration(string groupId, string taskId);
        DynamoDBStructure GetDynamoDBStructure(string groupId, string taskId);
    }

    class TaskConfigurationService : ITaskConfigurationService
    {
        IConfiguration Configuration { get; }

        public TaskConfigurationService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public InstanceConfiguration GetDataPreparationConfiguration()
        {
            return Configuration.GetSection("DataPreparation:InstanceConfiguration").Get<InstanceConfiguration>();
        }

        public JobConfiguration GetTaskConfiguration(string groupId, string taskId)
        {
            return Configuration.GetSection($"TaskConfigurations:{groupId}:{taskId}").Get<JobConfiguration>();

            /*
            IConfigurationSection section = Configuration.GetSection($"TaskConfigurations:{taskId}");

            RunJobConfiguration jobConfiguration = new RunJobConfiguration();

            jobConfiguration.ClusterName = section["ClusterName"];
            jobConfiguration.ReleaseLabel = section["ReleaseLabel"];

            var apps = from app in section.GetSection("Applications").GetChildren()
                       select new Application() { Name = app.Value };

            jobConfiguration.Applications = apps.ToList();

            jobConfiguration.EC2KeyName = section["EC2KeyName"];
            jobConfiguration.InstanceCount = section.GetValue<int>("InstanceCount");
            jobConfiguration.KeepJobAlive = section.GetValue<bool>("KeepJobAlive");
            jobConfiguration.MasterInstanceType = section["MasterInstanceType"];
            jobConfiguration.SlaveInstanceType = section["SlaveInstanceType"];
            jobConfiguration.ServiceRole = section["ServiceRole"];
            jobConfiguration.JobFlowRole = section["JobFlowRole"];

            List<BootstrapActionConfig> bootstrapActions = new List<BootstrapActionConfig>();

            foreach (var action in section.GetSection("BootstrapActions").GetChildren())
            {
                BootstrapActionConfig bootstrapConfig = new BootstrapActionConfig();

                bootstrapConfig.Name = action["Name"];
                bootstrapConfig.ScriptBootstrapAction = new ScriptBootstrapActionConfig();
                bootstrapConfig.ScriptBootstrapAction.Path = action["ScriptBootstrapAction:Path"];

                var args = from arg in action.GetSection("ScriptBootstrapAction:Args").GetChildren()
                           select arg.Value;

                bootstrapConfig.ScriptBootstrapAction.Args = args.ToList();

                bootstrapActions.Add(bootstrapConfig);
            }

            jobConfiguration.BootstrapActions = bootstrapActions;

            List<StepConfig> stepConfigurations = new List<StepConfig>();

            foreach (var stepConfigConfig in section.GetSection("StepConfigurations").GetChildren())
            {
                StepConfig stepConfig = new StepConfig();
                stepConfig.Name = stepConfigConfig["Name"];
                stepConfig.ActionOnFailure = stepConfigConfig["ActionOnFailure"] == "CONTINUE" ?
                                                                                    ActionOnFailure.CONTINUE :
                                                                                    ActionOnFailure.CANCEL_AND_WAIT;

                stepConfig.HadoopJarStep = new HadoopJarStepConfig();
                stepConfig.HadoopJarStep.Jar = stepConfigConfig["HadoopJarStep:Jar"];

                var args = from arg in stepConfigConfig.GetSection("HadoopJarStep:Args").GetChildren()
                           select arg.Value;

                stepConfig.HadoopJarStep.Args = args.ToList();

                stepConfigurations.Add(stepConfig);
            }

            jobConfiguration.StepConfigurations = stepConfigurations;
            */
        }

        public DynamoDBStructure GetDynamoDBStructure(string groupId, string taskId)
        {
            return Configuration.GetSection($"DynamoDBStructure:{groupId}:{taskId}").Get<DynamoDBStructure>();
        }
    }
}