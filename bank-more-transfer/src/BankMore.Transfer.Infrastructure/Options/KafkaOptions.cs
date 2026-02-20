namespace BankMore.Transfer.Infrastructure.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = "";
    public string TopicTransfersCompleted { get; set; } = "";
}
