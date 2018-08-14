using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using WebAPI.Model.OptionBindings;

using Amazon.EC2;
using Amazon.EC2.Model;


namespace WebAPI.Model.Routines
{
    public struct GetInstanceStatusRequestConfiguration
    {
        public IEnumerable<string> InstanceIds { get; set; }
        public bool IncludeAllInstances { get; set; }
    }

    public struct GetInstancesRequestConfiguration
    {
        public IEnumerable<Filter> Filters { get; set; }
        public int MaxInstances { get; set; }
    }

    // todo: move to a service (dependency injection, scoped)
    public static class Instances
    {
        public static async Task<RunInstancesResponse> LaunchInstance(IAmazonEC2 ec2Client, InstanceConfiguration launchConfiguration)
        {
            // var script = string.Empty;
            
            // using (StreamReader sr = new StreamReader(launchConfiguration.LaunchScript))
            // {
            //     script += sr.ReadToEnd();
            // }

            RunInstancesRequest request = new RunInstancesRequest()
            {
                // Amazon Linux AMI 2017.09.1 (HVM), SSD Volume Type
                ImageId = launchConfiguration.ImageId,
                InstanceType = launchConfiguration.InstanceType,
                BlockDeviceMappings = launchConfiguration.BlockDeviceMappings.Select(mapping => new Amazon.EC2.Model.BlockDeviceMapping()
                {
                    DeviceName = mapping.DeviceName,
                    Ebs = new EbsBlockDevice() {
                        DeleteOnTermination = mapping.Ebs.DeleteOnTermination,
                        SnapshotId = mapping.Ebs.SnapshotId,
                        VolumeSize = mapping.Ebs.VolumeSize
                    }
                }).ToList(),
                MinCount = 1,
                MaxCount = 1,
                TagSpecifications = launchConfiguration.TagSpecifications.Select(specification =>
                {
                    return new Amazon.EC2.Model.TagSpecification() {

                        ResourceType = ParseResourceType(specification.ResourceType),
                        Tags = specification.Tags.Select(tag =>
                        {
                            return new Amazon.EC2.Model.Tag() {
                                Key = tag.Key,
                                Value = tag.Value
                            };
                        }).ToList()
                    };
                }).ToList(),
                UserData = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(launchConfiguration.LaunchScript.Replace("\\n", "\n"))),
                IamInstanceProfile = new IamInstanceProfileSpecification()
                {
                    Name = launchConfiguration.InstanceProfileSpecificationName
                },
                KeyName = launchConfiguration.KeyName
                //SecurityGroups = new List<string>() { "tmpGroup" }
            };

            return await ec2Client.RunInstancesAsync(request);
        }

        public static async Task<DescribeInstanceStatusResponse> GetInstanceStatus(IAmazonEC2 ec2Client, GetInstanceStatusRequestConfiguration requestConfiguration)
        {
            DescribeInstanceStatusRequest request = new DescribeInstanceStatusRequest()
            {
                InstanceIds = new List<string>(requestConfiguration.InstanceIds),
                IncludeAllInstances = requestConfiguration.IncludeAllInstances
            };

            return await ec2Client.DescribeInstanceStatusAsync(request);
        }

        public static async Task<DescribeInstancesResult> GetInstances(IAmazonEC2 ec2Client, GetInstancesRequestConfiguration requestConfiguration)
        {
            var request = new DescribeInstancesRequest()
            {
                Filters = new List<Filter>(requestConfiguration.Filters),
                MaxResults = requestConfiguration.MaxInstances
            };

            try
            {
                var result = await ec2Client.DescribeInstancesAsync(request);

                List<Instance> instances = new List<Instance>();

                // check HTTP status code
                foreach (var reservation in result.Reservations)
                {
                    foreach (var instance in reservation.Instances)
                    {
                        instances.Add(new Instance(instance.InstanceType, instance.Tags));
                    }
                }

                return new DescribeInstancesResult(new Result() { ErrorCode = -1}, instances);
            }
            catch { }

            return new DescribeInstancesResult(new Result() { ErrorCode = -1 }, new List<Instance>() { });
        }

        static ResourceType ParseResourceType(string value)
        {
            const string INSTANCE = "Instance";

            switch (value)
            {
                case INSTANCE:
                    return ResourceType.Instance;

                default:
                    return null;
            }
        }
    }
}