// Program.cs
using System;
using System.Threading.Tasks;
using ServiceBusSenderModule; // This using statement covers both SenderModule and Employee

public class Program
{
    private const string ConnectionString = "Endpoint=sb://mentossrvbus.servicebus.windows.net/;SharedAccessKeyName=queuepolicy;SharedAccessKey=YGl/cJ+F7sejJKEHfCZ0SHEZOBGd4Xxvs+ASbNPBePg=;EntityPath=firstqueue";
    private const string QueueName = "firstqueue";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Service Bus Sender Application Started.");

        await using var senderModule = new SenderModule(ConnectionString, QueueName);

        try
        {
            await senderModule.InitializeAsync();

            Console.WriteLine("\n--- Sending Messages ---");

            // Send json body with Employee class
            //var employee = new Employee() { FirstName = "Skill", LastName = "Mellon", Salary = 27000 };
            //await senderModule.SendObjectAsJsonMessageAsync(employee);

            // Send message with message-id, try duplicate detection
            //await senderModule.SendMessageIdAsync("bts", "30");

            //await senderModule.SendMessageAsync("This is example");


            Console.WriteLine("\nAll messages sent!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unhandled error occurred in Program.cs: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Service Bus Sender Application Finished.");
        }
    }
}