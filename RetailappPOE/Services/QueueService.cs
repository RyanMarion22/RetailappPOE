using Azure.Storage.Queues;
using System.Text.Json;

namespace RetailappPOE.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage")
                ?? throw new InvalidOperationException("AzureStorage connection string not found.");

            string queueName = "orders-queue";

            _queueClient = new QueueClient(connectionString, queueName);

            // Create queue if it doesn't exist
            _queueClient.CreateIfNotExists();
        }

        /// <summary>
        /// Send a message to the queue (serialized as JSON).
        /// </summary>
        public async Task SendMessageAsync<T>(T message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(jsonMessage);
        }

        /// <summary>
        /// Receive the next message from the queue.
        /// </summary>
        public async Task<T?> ReceiveMessageAsync<T>()
        {
            var response = await _queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                string messageText = response.Value.MessageText;
                await _queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);

                return JsonSerializer.Deserialize<T>(messageText);
            }

            return default;
        }
    }
}
