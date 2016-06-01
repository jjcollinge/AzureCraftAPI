using AzureCraftAPI.Model;
using System;
using System.Collections.Generic;
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

        public PartitionInfo GetPartitionInfo(string lastname)
        {
            // Create a new webrequest to hit backend service
            WebRequest request = WebRequest.Create($"{baseURI}?lastname={lastname}");
            WebResponse response = request.GetResponse();

            Dictionary<string, string> content = ParseResponse(response);

            PartitionInfo partitionInfo = new PartitionInfo();
            var key = lastname[0].ToString();

            if (!content["Result"].Contains("already exists"))
            {
                // Unique name added to the backend service
                partitionInfo = HandleUniqueName(key, content);
            }
            else
            {
                // None unique name - not added to backend service
                partitionInfo = HandleNonUniqueName(key);
            }

            return partitionInfo;
        }

        private PartitionInfo HandleNonUniqueName(string key)
        {
            PartitionInfo partitionInfo = new PartitionInfo();

            if (partitions.ContainsKey(key))
            {
                // PartitonInfo already exists for this partition key
                partitionInfo = partitions[key];
            }
            else
            {
                // PartitonInfo doesn't exists for this partition key
                // and we have no way of getting it...
                // Hijack another partition or throw if none exist
                partitionInfo = partitions.LastOrDefault().Value;
            }

            return partitionInfo;
        }

        private PartitionInfo HandleUniqueName(string key, Dictionary<string, string> content)
        {
            PartitionInfo partitionInfo = new PartitionInfo();

            if (!partitions.ContainsKey(key))
            {
                // Unique partition key...
                partitionInfo = HandleUniquePartitionKey(key, content);
            }
            else
            {
                // Non unique partition key...
                partitionInfo = HandleNonUniquePartitionKey(key);
            }

            return partitionInfo;
        }

        private PartitionInfo HandleNonUniquePartitionKey(string key)
        {
            // Set the friendly id to the existing partition index
            return partitions[key];
        }

        private PartitionInfo HandleUniquePartitionKey(string key, Dictionary<string, string> content)
        {
            PartitionInfo partitionInfo = new PartitionInfo();

            // Set the partition key
            partitionInfo.PartitionKey = key;

            // Get the partition Id - Guid
            partitionInfo.PartitionId = content["Processing service partition ID"];

            // Get the URL of the partition service
            partitionInfo.PartitionReplicaAddress = content["Processing service replica address"];

            // Create a new index for this partition key
            var index = partitions.Count.ToString();

            // Set the partitions friendly Id
            partitionInfo.FriendlyPartitionId = index;

            // Store the partition against the partition key
            partitions.Add(partitionInfo.PartitionKey, partitionInfo);

            return partitionInfo;
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
