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
        private Dictionary<string, string> partitions; 

        private AlphabetPartitionService()
        {
            partitions = new Dictionary<string, string>();
        }

        public static AlphabetPartitionService GetSingleton()
        {
            if (singleton == null)
                singleton = new AlphabetPartitionService();
            return singleton;
        }

        public PartitionInfo GetPartitionInfo(string lastname)
        {
            PartitionInfo partitionInfo = new PartitionInfo();
            WebRequest request = WebRequest.Create($"{baseURI}?lastname={lastname}");
            WebResponse response = request.GetResponse();
            Dictionary<string, string> kvps = new Dictionary<string, string>();

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();

                // Parse string
                result = result.Replace("<br>", "\n");
                result = result.Replace("<p>", "\n");

                using (StringReader stringReader = new StringReader(result))
                {
                    string line = string.Empty;
                    do
                    {
                        line = stringReader.ReadLine();
                        if (line != null)
                        {
                            var kvp = line.Split(':');

                            // URL hack [protocol]:[IPorFQDN]:[port]
                            if (kvp.Length > 3)
                                kvp[1] += kvp[2].Trim() + kvp[3].Trim();

                            kvps.Add(kvp[0].Trim(), kvp[1].Trim());
                        }

                    } while (line != null);
                }
            }

            if (!kvps["Result"].Contains("already exists"))
            {
                var startIndex = kvps["Result"].IndexOf(" ") + 1;
                var length = kvps["Result"].Substring(startIndex).IndexOf(" ");
                partitionInfo.Name = kvps["Result"].Substring(startIndex, length);
                partitionInfo.PartitionId = kvps["Processing service partition ID"];
                partitionInfo.PartitionReplicaAddress = kvps["Processing service replica address"];
                partitionInfo.PartitionKey = partitionInfo.Name[0];

                if(!partitions.ContainsKey(partitionInfo.PartitionReplicaAddress))
                {
                    var friendlyId = partitions.Count.ToString();
                    partitions.Add(partitionInfo.PartitionReplicaAddress, friendlyId);
                    partitionInfo.FriendlyPartitionId = friendlyId;
                }
                else
                {
                    partitionInfo.FriendlyPartitionId = partitions[partitionInfo.PartitionReplicaAddress];
                }
            }

            return partitionInfo;
        }
    }
}
