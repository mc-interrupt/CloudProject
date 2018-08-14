using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Globalization;

using WebAPI.Model.Services;
using WebAPI.Model.OptionBindings;

namespace WebAPI.Controllers
{
    public class TaskResultFetcherController : Controller
    {
        ITaskConfigurationService TaskConfigurationService { get; set; }
        IAmazonDynamoDB DynamoDB { get; set; }

        public TaskResultFetcherController(ITaskConfigurationService taskConfigurationService, IAmazonDynamoDB dynamoDB)
        {
            TaskConfigurationService = taskConfigurationService;
            DynamoDB = dynamoDB;
        }

        [HttpGet]
        public async Task<IEnumerable<Dictionary<string, string>>> DefaultTaskData(string groupId, string taskId, bool sort, params string[] selection)
        {
            string table = "Default";
            var scanResult = await DynamoDB.ScanAsync(table, new List<string>() {"Airport", "Ranking", "RoachAttr"});

            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

            scanResult.Items.ForEach(r =>
            {
                var dict = new Dictionary<string, string>();

                foreach (var kvp in r)
                    dict.Add(kvp.Key, kvp.Value.S);

                result.Add(dict);
            });

            return result;
        }

        public async Task<IEnumerable<Dictionary<string, string>>> GetTaskResult(string groupId, string taskId, string sortAttribute, bool ascending, params string[] selection)
        {
            DynamoDBStructure dynamoDbStructure = TaskConfigurationService.GetDynamoDBStructure(groupId, taskId);

            string table = $"{groupId}{taskId}";

            IEnumerable<Dictionary<string, string>> result;
            List<Dictionary<string, string>> raw = new List<Dictionary<string, string>>();

            if (selection == null || selection.Length == 0)
            {
                var scanResult = await DynamoDB.ScanAsync(table, new List<string>(dynamoDbStructure.Attributes));

                scanResult.Items.ForEach(r =>
                {
                    var dict = new Dictionary<string, string>();

                    foreach (var kvp in r)
                        dict.Add(kvp.Key, kvp.Value.S);

                    raw.Add(dict);
                });
            }
            else
            {
                QueryRequest request = new QueryRequest();
                request.TableName = table;
                request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>();

                for (int i = 0; i < selection.Length; i+=2)
                {
                    var condition = $"{selection[i]}=:a{i+1}";

                    if (i == 0)
                        request.KeyConditionExpression = condition;
                    else
                        request.KeyConditionExpression = request.KeyConditionExpression + $" AND {condition}";          

                    request.ExpressionAttributeValues.Add($":a{i+1}", new AttributeValue(selection[i+1]));          
                }

                var response = await DynamoDB.QueryAsync(request);

                response.Items.ForEach(r =>
                {
                    var dict = new Dictionary<string, string>();

                    foreach (var kvp in r)
                        dict.Add(kvp.Key, kvp.Value.S);

                    raw.Add(dict);
                });
            }

            result = raw;

            if (!string.IsNullOrEmpty(sortAttribute))
            {
                result = ascending ?
                            raw.OrderBy(p => Convert.ToDouble(p[sortAttribute], CultureInfo.InvariantCulture)) :
                            raw.OrderByDescending(p => Convert.ToDouble(p[sortAttribute], CultureInfo.InvariantCulture));
            }

            return result;
        }
    }
}