// See https://aka.ms/new-console-template for more information

using Azure.Messaging.ServiceBus;
//using System.Text.Json.Serialization;
//using Newtonsoft.Json;
//using System.Text;
using System.Text.Json;
string connectionString = "Endpoint=sb://mentossrvbus.servicebus.windows.net/;SharedAccessKeyName=queuepolicy;SharedAccessKey=YGl/cJ+F7sejJKEHfCZ0SHEZOBGd4Xxvs+ASbNPBePg=;EntityPath=firstqueue";
string queueName = "firstqueue";

// service bus client
ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);

// service bus sender
ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);

// send message using service bus sender
//ServiceBusMessage serviceBusMessage = new ServiceBusMessage("hi from vs");
//await serviceBusSender.SendMessageAsync(serviceBusMessage);
Employee employee = new Employee() { FirstName = "Skill", LastName = "Mellon", Salary = 27000 };
//var employeeDataInJson = JsonConvert.SerializeObject(employee);
//var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(employeeDataInJson));
string jsonString = JsonSerializer.Serialize(employee);
var msg = new ServiceBusMessage(jsonString);

await serviceBusSender.SendMessageAsync(msg);

Console.WriteLine("message sent!");
class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Salary { get; set; }
}
