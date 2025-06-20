// Employee.cs
namespace ServiceBusQueueSender // Or a more general namespace like 'SharedModels' or 'YourApp.Models'
{
    public class Employee
    {
        public string FirstName { get; set; } = string.Empty; // Initialize to avoid null warnings
        public string LastName { get; set; } = string.Empty;
        public int Salary { get; set; }

        // Optional: Override ToString() for easier console output
        public override string ToString()
        {
            return $"Employee: {FirstName} {LastName}, Salary: {Salary:C0}";
        }
    }
}