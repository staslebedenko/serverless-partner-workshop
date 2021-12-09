namespace Domain
{
    public class Prediction
    {
        public Guid Id { get; set; }

        public int Rate { get; set; }

        public string PartitionKey { get; set; }
    }
}
