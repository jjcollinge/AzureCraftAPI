using AzureCraftAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCraftAPI.Model
{
    public class NamedPartition
    {
        public PartitionInfo Partition { get; set; }
        public string Name { get; set; }
    }
}
