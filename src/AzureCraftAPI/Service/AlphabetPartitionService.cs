using AzureCraftAPI.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AzureCraftAPI.Service
{
    public class AlphabetPartitionService
    {
        private static AlphabetPartitionService singleton;
        private string baseURI = "http://localhost:8081/alphabetpartitions";
        private Dictionary<string, PartitionInfo> partitions;

        private AlphabetPartitionService()
        {
            // Store as <'W', PartitionInfo>
            partitions = new Dictionary<string, PartitionInfo>();
        }

        public static AlphabetPartitionService GetSingleton()
        {
            if (singleton == null)
                singleton = new AlphabetPartitionService();
            return singleton;
        }

        public PartitionInfo GetPartitionInfo(string name)
        {
            // Create a new webrequest to hit backend service
            WebRequest request = WebRequest.Create($"{baseURI}?lastname={name}");
            WebResponse response = request.GetResponse();

            Dictionary<string, string> content = ParseResponse(response);

            // First letter of name is the key
            var key = name[0].ToString();

            PartitionInfo partitionInfo;

            if (partitions.ContainsKey(key))
            {
                // Load partition from memory
                partitionInfo = partitions[key];
            }
            else
            {
                // Load partition from response
                partitionInfo = CreatePartitionFromContent(key, content);
                partitions.Add(key, partitionInfo);
            }

            return partitionInfo;
        }

        private PartitionInfo CreatePartitionFromContent(string key, Dictionary<string, string> content)
        {
            PartitionInfo partition = new PartitionInfo();

            // Set the partition key
            partition.PartitionKey = key;

            // Get the partition Id - Guid
            partition.PartitionId = content["Processing service partition ID"];

            // Get the URL of the partition service
            partition.PartitionReplicaAddress = content["Processing service replica address"].ToLower();

            // Create a new index for this partition key
            var index = partitions.Count.ToString();

            // Set the partitions friendly Id
            partition.FriendlyPartitionId = index;

            return partition;
        }

        private static Dictionary<string, string> ParseResponse(WebResponse response)
        {
            // Read the stream and store kvp in a Dictionary
            Dictionary<string, string> kvps = new Dictionary<string, string>();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                // Get the string
                string result = streamReader.ReadToEnd();

                // Normalise newlines
                result = result.Replace("<br>", "\n");
                result = result.Replace("<p>", "\n");

                // Parse the string and split into newlines
                using (StringReader stringReader = new StringReader(result))
                {
                    string line = string.Empty;
                    do
                    {
                        line = stringReader.ReadLine();
                        if (line != null)
                        {
                            // Expected format: [Heading]:[Content]\n
                            var kvp = line.Split(':');

                            // HACK for URL foramt: [Heading]:[protocol]:[IPorFQDN]:[port]
                            if (kvp.Length > 3)
                                kvp[1] += kvp[2].Trim() + kvp[3].Trim();

                            // Add to dictionary
                            kvps.Add(kvp[0].Trim(), kvp[1].Trim());
                        }

                    } while (line != null);
                }
            }

            return kvps;
        }
    }
}
