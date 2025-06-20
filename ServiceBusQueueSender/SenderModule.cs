// ServiceBusSenderModule.cs
using Azure.Messaging.ServiceBus;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// The Employee class is now in its own file, but in the same namespace.
// If Employee were in a different namespace (e.g., 'YourApp.Models'),
// you'd add 'using YourApp.Models;' here.

namespace ServiceBusQueueSender
{
    public class SenderModule : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private ServiceBusClient? _client;
        private ServiceBusSender? _sender;

        public SenderModule(string connectionString, string queueName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public async Task InitializeAsync()
        {
            if (_client == null)
            {
                _client = new ServiceBusClient(_connectionString);
                _sender = _client.CreateSender(_queueName);
                Console.WriteLine($"SenderModule initialized for queue: '{_queueName}'");
            }
        }

        public async Task SendMessageAsync(string messageBody)
        {
            if (_sender == null)
            {
                Console.Error.WriteLine("Sender not initialized. Call InitializeAsync() first.");
                return;
            }

            try
            {
                var serviceBusMessage = new ServiceBusMessage(messageBody);
                await _sender.SendMessageAsync(serviceBusMessage);
                Console.WriteLine($"Sent string message: '{messageBody}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sending string message '{messageBody}': {ex.Message}");
            }
        }

        public async Task SendMessageIdAsync(string  messageBody, string messageId)
        {
            if (_sender == null)
            {
                Console.Error.WriteLine("Sender not initialized. Call InitializeAsync() first.");
                return;
            }

            try
            {
                var serviceBusMessage = new ServiceBusMessage(messageBody);
                serviceBusMessage.MessageId = messageId;
                await _sender.SendMessageAsync(serviceBusMessage);
                Console.WriteLine($"Sent string message: '{messageBody}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sending string message '{messageBody}': {ex.Message}");
            }
        }

        public async Task SendObjectAsJsonMessageAsync<T>(T data)
        {
            if (_sender == null)
            {
                Console.Error.WriteLine("Sender not initialized. Call InitializeAsync() first.");
                return;
            }

            try
            {
                string jsonString = JsonSerializer.Serialize(data);
                var serviceBusMessage = new ServiceBusMessage(jsonString);

                await _sender.SendMessageAsync(serviceBusMessage);
                Console.WriteLine($"Sent JSON message: '{jsonString}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error sending JSON message for object of type {typeof(T).Name}: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("Disposing Service Bus sender resources...");
            if (_sender != null)
            {
                await _sender.DisposeAsync().ConfigureAwait(false);
                _sender = null;
            }
            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
                _client = null;
            }
            GC.SuppressFinalize(this);
            Console.WriteLine("Service Bus sender resources disposed.");
        }
    }
}