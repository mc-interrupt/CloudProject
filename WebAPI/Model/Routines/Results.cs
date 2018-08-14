using System.Collections.Generic;
using Amazon.EC2.Model;

namespace WebAPI.Model.Routines
{
    public class Result
    {
        public int ErrorCode { get; set; }
    }

    public class Instance
    {
        public Instance(string instanceType, IEnumerable<Tag> tags)
        {
            InstanceType = instanceType;
            Tags = tags;
        }

        public string InstanceType { get; private set;}
        public IEnumerable<Tag> Tags { get; private set; }
    }

    public class DescribeInstancesResult
    {
        public DescribeInstancesResult(Result result, IEnumerable<Instance> instances)
        {
            Result = result;
            Instances = instances;
        }

        public Result Result { get; private set; }
        public IEnumerable<Instance> Instances { get; private set; }
    }
}