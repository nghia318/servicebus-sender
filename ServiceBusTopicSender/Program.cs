using Azure.Messaging.ServiceBus;

public class Program
{
    private const string TopicConnectionString = "Endpoint=sb://srvbusstandard.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Fh5mM7JdPLDzgJ/KuNqtMPMj8O3p/YgSU+ASbLzmXb8=";
    private const string TopicName = "firsttopic";

    public static async Task Main(string[] args)
    {
        await using var client = new ServiceBusClient(TopicConnectionString);
        await using var sender = client.CreateSender(TopicName);

        var msg = new ServiceBusMessage("topic to subs");
        await sender.SendMessageAsync(msg);
    }
}