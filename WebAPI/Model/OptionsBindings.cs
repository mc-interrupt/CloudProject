namespace WebAPI.Model.OptionBindings
{
    public class JobConfiguration
    {
        public JobConfiguration() { }

        public string ClusterName { get; set; }
        public string ReleaseLabel { get; set; }
        public Application[] Applications { get; set; }
        public string EC2KeyName { get; set; }
        public int InstanceCount { get; set; }
        public bool KeepJobAlive { get; set; }
        public string MasterInstanceType { get; set; }
        public string SlaveInstanceType { get; set; }
        public string ServiceRole { get; set; }
        public string JobFlowRole { get; set; }
        public BootstrapActionConfig[] BootstrapActions { get; set; }
        public StepConfig[] StepConfigurations { get; set; }
        public string LogUri { get; set; }
    }

    public class Application
    {
        public Application() { }

        public string Name { get; set; }
    }

    public class BootstrapActionConfig
    {
        public BootstrapActionConfig() { }

        public string Name { get; set; } = string.Empty;
        public ScriptBootstrapAction ScriptBootstrapAction { get; set; }
    }

    public class ScriptBootstrapAction
    {
        public ScriptBootstrapAction() { }

        public string Path { get; set; } = string.Empty;
        public string[] Args { get; set; }
    }

    public class StepConfig
    {
        public StepConfig() { }

        public string Name { get; set; } = string.Empty;
        public string ActionOnFailure { get; set; } = string.Empty;

        public HadoopJarStep HadoopJarStep { get; set; }
    }

    public class HadoopJarStep
    {
        public HadoopJarStep() { }

        public string Jar { get; set; } = string.Empty;
        public string[] Args { get; set; }
    }

    public class InstanceConfiguration
    {
        public string ImageId { get; set; }
        public string InstanceType { get; set;}
        public string InstanceProfileSpecificationName { get; set; }
        public string LaunchScript { get; set; }
        public TagSpecification[] TagSpecifications { get; set; }
        public BlockDeviceMapping[] BlockDeviceMappings { get; set; }
        public string KeyName { get; set; }
    }

    public class TagSpecification
    {
        public string ResourceType { get; set; }
        public Tag[] Tags { get; set; }
    }

    public class Tag
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class BlockDeviceMapping
    {
        public string DeviceName { get; set; }
        public Ebs Ebs { get; set; }
    }

    public class Ebs
    {
        public bool DeleteOnTermination { get; set; }
        public string SnapshotId { get; set; }
        public int VolumeSize { get; set; }
    }

    public class DynamoDBStructure
    {
        public string[] Attributes { get; set; }
    }
}