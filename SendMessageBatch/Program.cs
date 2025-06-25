using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Threading.Tasks;

public class Program
{
    // Service Bus configuration
    private const string ConnectionString = ""; // Connection string for Service Bus namespace
    private const string TopicName = "firsttopic"; // Name of the Service Bus topic
    private const int BatchSize = 5; // Number of messages to send in the batch

    public static async Task Main(string[] args)
    {
        // Initialize Service Bus clients
        await using var client = new ServiceBusClient(ConnectionString);
        var adminClient = new ServiceBusAdministrationClient(ConnectionString);
        var sender = client.CreateSender(TopicName);

        try
        {
            // Validate topic existence
            Console.WriteLine($"Checking if topic '{TopicName}' exists...");
            if (!await adminClient.TopicExistsAsync(TopicName))
            {
                throw new Exception($"Topic '{TopicName}' does not exist in the Service Bus namespace.");
            }
            Console.WriteLine($"Topic '{TopicName}' exists.");

            // Send a batch of messages
            await SendMessageBatchAsync(sender);
            Console.WriteLine($"Successfully sent a batch of {BatchSize} messages to topic '{TopicName}'.");
        }
        catch (ServiceBusException ex)
        {
            Console.WriteLine($"\nServiceBusException: {ex.Message}\nReason: {ex.Reason}\nStackTrace: {ex.StackTrace}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            Console.WriteLine("\nExiting!");
        }
    }

    private static async Task SendMessageBatchAsync(ServiceBusSender sender)
    {
        // Create a batch
        using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();
        Console.WriteLine($"Creating a batch of {BatchSize} messages for topic '{TopicName}'...");

        // Add messages to the batch
        for (int i = 1; i <= BatchSize; i++)
        {
            var message = new ServiceBusMessage($"Message {i} sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            if (!batch.TryAddMessage(message))
            {
                throw new Exception($"Failed to add message {i} to the batch. Batch size limit exceeded.");
            }
            Console.WriteLine($"Added message {i} to batch.");
        }

        // Verify batch is not null and has messages
        if (batch == null || batch.Count == 0)
        {
            throw new Exception("Batch is null or empty.");
        }
        Console.WriteLine($"Batch contains {batch.Count} messages.");

        // Send the batch
        Console.WriteLine("Sending batch to topic...");
        await sender.SendMessagesAsync(batch);
    }
}