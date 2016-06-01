namespace AzureCraftAPI.Model
{
    public class PartitionInfo
    {
        public string PartitionId { get; internal set; }
        public string PartitionKey { get; internal set; }
        public string FriendlyPartitionId { get; internal set; }
        public string PartitionReplicaAddress { get; internal set; }
    }
}