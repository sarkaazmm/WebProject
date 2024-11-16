using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class Program
{
    static string _solutionPath = @"C:\\Halia\\University\\University_Y3\\WEB\\WebProject"; // подвоєні слеші
    static void Main(string[] args)
    {
        Console.WriteLine(_solutionPath);
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Service Manager ===");
            Console.WriteLine("1. Start and watch API");
            Console.WriteLine("2. Start Client");
            Console.WriteLine("3. Start PrimeCheckService (Instance 1)");
            Console.WriteLine("4. Start PrimeCheckService (Instance 2)");
            Console.WriteLine("5. Start All Services");
            Console.WriteLine("6. Close All Services");
            Console.WriteLine("0. Exit");
            
            var key = Console.ReadKey(true);
            
            switch (key.KeyChar)
            {
                case '1':
                    StartService("API", "dotnet watch run");
                    break;
                case '2':
                    StartService("client", "npm start");
                    break;
                case '3':
                    StartService("PrimeCheckService", "dotnet run", "Instance 1");
                    break;
                case '4':
                    StartService("PrimeCheckService", "dotnet run", "Instance 2");
                    break;
                case '5':
                    StartAllServices();
                    break;
                case '6':
                    CloseAllServices();
                    break;
                case '0':
                    return;
            }
        }
    }

    static void StartService(string folder, string command, string instanceName = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k cd \"{Path.Combine(_solutionPath, folder)}\" && {command}",
                UseShellExecute = true
            }
        };

        try
        {
            process.Start();
            Console.WriteLine($"\nStarted {folder} {instanceName ?? ""}");
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError starting {folder}: {ex.Message}");
            Thread.Sleep(2000);
        }
    }

    static void StartAllServices()
    {
        StartService("API", "dotnet run");
        StartService("client", "npm start");
        StartService("PrimeCheckService", "dotnet run", "Instance 1");
        StartService("PrimeCheckService", "dotnet run", "Instance 2");
        
        Console.WriteLine("\nAll services started!");
        Thread.Sleep(2000);
    }

    static void CloseAllServices()
    {
        foreach (var process in Process.GetProcessesByName("cmd"))
        {
            try
            {
                process.Kill();
                Console.WriteLine("\nClosed process: " + process.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError closing process: {ex.Message}");
            }
        }

        Thread.Sleep(2000); // Пауза перед поверненням до меню
    }
}
