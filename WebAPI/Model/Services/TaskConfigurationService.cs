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
        }

        public DynamoDBStructure GetDynamoDBStructure(string groupId, string taskId)
        {
            return Configuration.GetSection($"DynamoDBStructure:{groupId}:{taskId}").Get<DynamoDBStructure>();
        }
    }
}