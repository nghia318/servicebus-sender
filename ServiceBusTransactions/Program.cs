using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions; // Still needed for TransactionScope

public class Program
{
    // === CONFIGURATION ===
    private const string ConnectionString = "";
    private const string Queue1Name = "queue_1";
    private const string Queue2Name = "queue_2";
    private const string Queue3Name = "queue_3";
    // =====================

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Azure Service Bus Cross-Entity Transaction Example");

        // Use 'await using' for proper disposal of Service Bus clients and senders/receivers
        await using var client = new ServiceBusClient(ConnectionString, new ServiceBusClientOptions { EnableCrossEntityTransactions = true });
        await using var receiverQueue1 = client.CreateReceiver(Queue1Name);
        await using var senderQueue2 = client.CreateSender(Queue2Name);
        await using var senderQueue3 = client.CreateSender(Queue3Name);

        Console.WriteLine($"Listening for messages on '{Queue1Name}'...");
        Console.WriteLine("Press Ctrl+C to stop.");

        while (true) // Keep listening until cancelled or an unhandled error occurs
        {
            try
            {
                // Receive up to 10 messages from Queue 1, waiting up to 5 seconds
                IReadOnlyList<ServiceBusReceivedMessage> receivedMessages =
                    await receiverQueue1.ReceiveMessagesAsync(maxMessages: 10, maxWaitTime: TimeSpan.FromSeconds(5));

                if (receivedMessages.Count > 0)
                {
                    Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Received {receivedMessages.Count} message(s) from '{Queue1Name}'.");
                    foreach (var message in receivedMessages)
                    {
                        // Delegate the transactional processing of each message to a dedicated method
                        await ProcessMessageInTransactionAsync(message, receiverQueue1, senderQueue2, senderQueue3);
                    }
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] No messages received from '{Queue1Name}'. Waiting...");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation cancelled. Exiting loop.");
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] An error occurred: {ex.Message}");
                // Add more robust error handling here, e.g., logging, exponential backoff, circuit breaking
                await Task.Delay(2000); // Wait a bit before retrying
            }
        }

        Console.WriteLine("Application exiting.");
    }

    /// <summary>
    /// Processes a single Service Bus message within a transaction scope.
    /// This method demonstrates the 'Receive-Process-Send-Complete' atomic pattern.
    /// </summary>
    private static async Task ProcessMessageInTransactionAsync(
        ServiceBusReceivedMessage message,
        ServiceBusReceiver receiver,
        ServiceBusSender sender2,
        ServiceBusSender sender3)
    {
        string messageBody = message.Body.ToString();
        Console.WriteLine($"\n  Processing message ID: {message.MessageId}, Body: '{messageBody}'");

        // The TransactionScope ensures that all operations within it either succeed or fail together.
        // If an exception occurs before transaction.Complete(), all operations are rolled back.
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // 1. Send to Queue 2
                await sender2.SendMessageAsync(new ServiceBusMessage(messageBody));
                Console.WriteLine($"    -> Sent message to '{sender2.EntityPath}'");

                // 2. Send to Queue 3
                await sender3.SendMessageAsync(new ServiceBusMessage(messageBody));
                Console.WriteLine($"    -> Sent message to '{sender3.EntityPath}'");

                // 3. Complete the original message from Queue 1
                await receiver.CompleteMessageAsync(message);
                Console.WriteLine($"    -> Completed message ID {message.MessageId} from '{receiver.EntityPath}'");

                // If all operations above succeeded, commit the transaction
                transaction.Complete();
                Console.WriteLine($"  Transaction for message ID {message.MessageId} successful!");
            }
            catch (Exception ex)
            {
                // If any error occurs, the transaction will automatically abort (rollback)
                // when it goes out of the 'using' scope.
                // This means:
                // - Messages sent to Queue 2 and Queue 3 will be rolled back (not appear in their queues).
                // - The original message from Queue 1 will return to Queue 1, becoming available for re-delivery.
                Console.Error.WriteLine($"  TRANSACTION FAILED for message ID {message.MessageId}: {ex.Message}");
                Console.Error.WriteLine($"  Message will be returned to '{receiver.EntityPath}' for re-delivery.");

                // Do NOT call transaction.Complete() here, let it rollback.
                // Do NOT call AbandonMessageAsync on the receiver; TransactionScope handles it.
            }
        }
    }
}