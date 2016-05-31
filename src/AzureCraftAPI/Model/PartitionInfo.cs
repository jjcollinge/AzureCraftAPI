namespace AzureCraftAPI.Service
{
    public class PartitionInfo
    {
        public string Name { get; internal set; }
        public string PartitionId { get; internal set; }
        public char PartitionKey { get; internal set; }
        public string FriendlyPartitionId { get; internal set; }
        public string PartitionReplicaAddress { get; internal set; }
    }
}