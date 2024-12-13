using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EnvironmentalResourceManagement
{
    abstract class UserBase
    {
        public string Username { get; }

        protected UserBase(string username)
        {
            Username = username;
        }

        public abstract void Menu();
    }

    class Admin : UserBase
    {
        private List<Resource> resources = new List<Resource>();

        public Admin(string username) : base(username) { }

        public override void Menu()
        {
            while (true)
            {
                Console.WriteLine("\nAdmin Menu:");
                Console.WriteLine("1. Add Resource");
                Console.WriteLine("2. View Resources");
                Console.WriteLine("3. Update Resource");
                Console.WriteLine("4. Search Resource");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                            
                        AddResource();
                        break;
                    case "2":
                        ViewResources();
                        break;
                    case "3":
                        UpdateResource();
                        break;
                    case "4":
                        SearchResource();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void AddResource()
        {
            Console.Write("Enter Resource ID: ");
            string id = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("Invalid ID. It cannot be empty.");
                return;
            }

            if (resources.Any(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Resource ID already exists. Please use a unique ID.");
                return;
            }

            Console.Write("Enter Resource Type: ");
            string type = Console.ReadLine();

            Console.Write("Enter Environmental Impact (CO2e): ");
            if (!double.TryParse(Console.ReadLine(), out double impact))
            {
                Console.WriteLine("Invalid impact. Enter a numeric value.");
                return;
            }

            resources.Add(new Resource(id, type, impact));
            Console.WriteLine("Resource added successfully!");
        }

        private void ViewResources()
        {
            if (!resources.Any())
            {
                Console.WriteLine("No resources available.");
                return;
            }

            Console.WriteLine("\nResources:");
            foreach (var resource in resources)
            {
                Console.WriteLine(resource);
            }
        }

        private void UpdateResource()
        {
            Console.Write("Enter Resource ID to update: ");
            string id = Console.ReadLine();

            var resource = resources.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (resource != null)
            {
                Console.Write("Enter new Resource Type: ");
                string newType = Console.ReadLine();
                Console.Write("Enter new Environmental Impact (CO2e): ");
                if (!double.TryParse(Console.ReadLine(), out double newImpact))
                {
                    Console.WriteLine("Invalid impact. Enter a numeric value.");
                    return;
                }

                resource.Type = newType;
                resource.Impact = newImpact;
                Console.WriteLine("Resource updated successfully!");
            }
            else
            {
                Console.WriteLine("Resource not found.");
            }
        }

        private void SearchResource()
        {
            Console.Write("Enter Resource Type to search: ");
            string searchType = Console.ReadLine();
            var foundResources = resources.Where(r => r.Type.Contains(searchType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!foundResources.Any())
            {
                Console.WriteLine("No matching resources found.");
                return;
            }

            Console.WriteLine("\nSearch results:");
            foreach (var resource in foundResources)
            {
                Console.WriteLine(resource);
            }
        }
    }

    class User : UserBase
    {
        private readonly Dictionary<string, List<double>> userLogs;

        public User(string username, Dictionary<string, List<double>> logs) : base(username)
        {
            userLogs = logs;
        }

        public override void Menu()
        {
            while (true)
            {
                Console.WriteLine("\nUser Menu:");
                Console.WriteLine("1. Log Daily Carbon Footprint");
                Console.WriteLine("2. View Weekly Carbon Footprint");
                Console.WriteLine("3. Save and Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LogDailyFootprint();
                        break;
                    case "2":
                        DisplayWeeklyFootprint();
                        break;
                    case "3":
                        SaveDataAndExit();
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void LogDailyFootprint()
        {
            Console.Write("Enter day of the week (1=Monday, ..., 7=Sunday): ");
            if (int.TryParse(Console.ReadLine(), out int day) && day >= 1 && day <= 7)
            {
                Console.Write("Enter your carbon footprint (CO2e): ");
                if (double.TryParse(Console.ReadLine(), out double footprint))
                {
                    userLogs[Username][day - 1] = footprint;
                    Console.WriteLine("Carbon footprint logged successfully!");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a numeric value.");
                }
            }
            else
            {
                Console.WriteLine("Invalid day. Please enter a number between 1 and 7.");
            }
        }

        private void DisplayWeeklyFootprint()
        {
            Console.WriteLine("\nWeekly Carbon Footprint Bar Chart:");
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var logs = userLogs[Username];

            int maxChartWidth = 100;

            Console.WriteLine("+------------+------------------------------------+----------------+");
            Console.WriteLine("| Day        | Chart                              | Footprint (%)  |");
            Console.WriteLine("+------------+------------------------------------+----------------+");

            foreach (var (day, i) in days.Select((day, i) => (day, i)))
            {
                double footprint = logs[i];
                int barLength = (int)footprint; 

                string bar = new string('#', Math.Min(barLength, maxChartWidth));
                Console.WriteLine($"| {day,-10} | {bar,-35} | {footprint,14:F1}% |");
            }

            Console.WriteLine("+------------+------------------------------------+----------------+");
        }

        private void SaveDataAndExit()
        {
            Console.WriteLine("Saving your data...");
            SaveData();
            Console.WriteLine("Data saved successfully! Exiting...");
        }

        private void SaveData()
        {
            
            File.WriteAllLines("UserLogs.txt", userLogs.Select(kv => $"{kv.Key},{string.Join(",", kv.Value)}"));
        }
    }

    class Resource
    {
        public string Id { get; }
        public string Type { get; set; }
        public double Impact { get; set; }

        public Resource(string id, string type, double impact)
        {
            Id = id;
            Type = type;
            Impact = impact;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Type: {Type}, Environmental Impact (CO2e): {Impact}";
        }
    }

    class Program
    {
        static Dictionary<string, string> adminAccounts = new Dictionary<string, string>();
        static Dictionary<string, string> userAccounts = new Dictionary<string, string>();
        static Dictionary<string, List<double>> userLogs = new Dictionary<string, List<double>>();
        static UserBase currentUser;

        static void Main(string[] args)
        {
            LoadData();

            if (!adminAccounts.Any() && !userAccounts.Any())
            {
                SetupAccounts();
            }

            if (Login())
            {
                currentUser.Menu();
            }
        }

        static void SetupAccounts()
        {
            Console.Write("Enter Admin Username: ");
            string adminUsername = Console.ReadLine();
            Console.Write("Enter Admin Password: ");
            string adminPassword = Console.ReadLine();
            adminAccounts[adminUsername] = adminPassword;
            Console.Clear();
            Console.Write("Enter User Username: ");
            string userUsername = Console.ReadLine();
            Console.Write("Enter User Password: ");
            string userPassword = Console.ReadLine();
            userAccounts[userUsername] = userPassword;
            userLogs[userUsername] = new List<double>(new double[7]);
            Console.Clear(); 
            SaveData();
        }

        static bool Login()
        {
            Console.WriteLine("Welcome to Environmental Resource Management System!");
            while (true)
            {
                Console.Write("Enter admin/user: ");
                string username = Console.ReadLine();
                Console.Write("Enter password: ");
                string password = Console.ReadLine();
                Console.Clear();

                if (adminAccounts.ContainsKey(username) && adminAccounts[username] == password)
                {
                    currentUser = new Admin(username);
                    return true;
                }
                else if (userAccounts.ContainsKey(username) && userAccounts[username] == password)
                {
                    currentUser = new User(username, userLogs);
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid username or password. Try again.");
                }
            }
        }

        static void LoadData()
        {
            try
            {
                
                if (File.Exists("AdminAccounts.txt"))
                {
                    foreach (var line in File.ReadLines("AdminAccounts.txt"))
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 2)
                        {
                            adminAccounts[parts[0]] = parts[1];
                        }
                    }
                }

                
                if (File.Exists("UserAccounts.txt"))
                {
                    foreach (var line in File.ReadLines("UserAccounts.txt"))
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 2)
                        {
                            userAccounts[parts[0]] = parts[1];
                        }
                    }
                }

                
                if (File.Exists("UserLogs.txt"))
                {
                    foreach (var line in File.ReadLines("UserLogs.txt"))
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 8) 
                        {
                            string username = parts[0];
                            List<double> logs = parts.Skip(1).Select(double.Parse).ToList();
                            userLogs[username] = logs;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while loading data: {ex.Message}");
            }

            
            foreach (var username in userAccounts.Keys)
            {
                if (!userLogs.ContainsKey(username))
                {
                    userLogs[username] = new List<double>(new double[7]); 
                }
            }
        }

        static void SaveData()
        {
            
            File.WriteAllLines("UserAccounts.txt", userAccounts.Select(kv => $"{kv.Key},{kv.Value}"));

           
            File.WriteAllLines("UserLogs.txt", userLogs.Select(kv => $"{kv.Key},{string.Join(",", kv.Value)}"));
        }
    }
}
