namespace BankMore.CurrentAccount.Infrastructure.Kafka;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = "";
    public string TopicTransfersCompleted { get; set; } = "";
    public string TopicFeesCompleted { get; set; } = "";
    public string TopicDlq { get; set; } = "";
    public string ConsumerGroupId { get; set; } = "";
}
