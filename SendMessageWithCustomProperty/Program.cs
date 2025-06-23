using Azure.Messaging.ServiceBus;

public class Program
{
    private const string TopicConnectionString = "Endpoint=sb://srvbusstandard.servicebus.windows.net/;SharedAccessKeyName=connection;SharedAccessKey=73jnWHoPwRFGvg+LDEVmo+ey1ur1ZQPRv+ASbFcDV8Y=;EntityPath=firsttopic";
    private const string TopicName = "firsttopic";
    private const string SubscriptionName = "filter-subs1";

    public static async Task Main(string[] args)
    {
        await using var client = new ServiceBusClient(TopicConnectionString);
        await using var sender = client.CreateSender(TopicName);

        try
        {
            string messageBody = $"Date {DateTime.Now}";
            //var message = new ServiceBusMessage(messageBody)
            //{
            //    ApplicationProperties =
            //    {
            //        {
            //            "test","true"
            //        }
            //    }
            //};
            //await sender.SendMessageAsync(message);

            var message = new ServiceBusMessage(messageBody);
            message.ApplicationProperties.Add("test", "true");

            await sender.SendMessagesAsync(new[] { message }); //message in here is a collection
            Console.WriteLine("Message sent successfully!");

        }
        catch (Exception ex) 
        {
            Console.Error.WriteLine(ex.Message);
        }

    }
}