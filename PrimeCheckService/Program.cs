using Core.Data;
using Core.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using PrimeCheckService.Services;

namespace PrimeCheckService;

class Program
{
    private volatile static PrimeCheckHistoryRepository _primeCheckHistoryRepository = default!;
    private volatile static CancelationTokenRepository _cancelationTokenRepository = default!;
    private volatile static AppDbContext _context;
    private static readonly object _lockObject = new object();

    static int MaxThreadCount = 2;
    static int ThreadTrotling = 10000;

    static void Main(string[] args)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(@"Server=(localdb)\simple-db;Database=simple-db;Trusted_Connection=True;");
            })
            .AddScoped<PrimeCheckHistoryRepository>()
            .AddScoped<CancelationTokenRepository>()
            .BuildServiceProvider();

        _context = serviceProvider.GetService<AppDbContext>()!;

        _primeCheckHistoryRepository = new PrimeCheckHistoryRepository(_context);
        _cancelationTokenRepository = new CancelationTokenRepository(_context);

        Thread.Sleep(5000);

        var threads = new List<Thread>();
        for (int i = 0; i < MaxThreadCount; i++)
        {
            var thread = new Thread(ProcessQueue);
            thread.Start();
            threads.Add(thread);
            Thread.Sleep(1000);
        }

        HandleUserInput();
    }

    static void HandleUserInput()
    {
        while (true)
        {
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Enter)
            {
                var list = _primeCheckHistoryRepository.GetAllInRgogress();
                Console.WriteLine("\nAll tasks in progress:");
                foreach (var item in list)
                {
                    Console.WriteLine($"TaskId: {item.Id}\t Number: {item.Number}\t Progress: {item.Progress}");
                }
            }
            else if (key.Key == ConsoleKey.Spacebar)
            {
                var list = _primeCheckHistoryRepository.GetAll(-3);
                Console.WriteLine("\nAll tasks in Queue:");
                foreach (var item in list)
                {
                    Console.WriteLine($"TaskId: {item.Id}\t Number: {item.Number}\t Progress: InQueue");
                }
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                var list = _primeCheckHistoryRepository.GetAll(-1);
                Console.WriteLine("\nAll tasks are Canceled:");
                foreach (var item in list)
                {
                    Console.WriteLine($"TaskId: {item.Id}\t Number: {item.Number}\t Progress: Canceled");
                }
            }
        }
    }

    static void ProcessQueue()
    {
        var service = new CheckService(_primeCheckHistoryRepository, _cancelationTokenRepository);
        
        while (true)
        {
            var nextTask = GetNextTaskThreadSafe();

            if (nextTask != null)
            {
                try
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:\t Processing Task: {nextTask.Id}");
                    _primeCheckHistoryRepository.UpdateProgress(nextTask.Id, 0);
                    service.IsPrime(nextTask.Id).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing task {nextTask.Id}: {ex.Message}");
                    // Optionally update task status to failed
                    // _primeCheckHistoryRepository.UpdateTaskStatus(nextTask.Id, "Failed");
                }
            }
            else
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:\t Queue is empty sleep {ThreadTrotling / 1000.0} sec");
                Thread.Sleep(ThreadTrotling);
            }
        }
    }

    static Core.Models.PrimeCheckHistory? GetNextTaskThreadSafe()
    {
        lock (_lockObject)
        {
            var task = _primeCheckHistoryRepository.GetNextTask();
            // if (task != null)
            // {
            //     // Optionally mark the task as "InProgress" here to prevent other threads from picking it up
            //     // _primeCheckHistoryRepository.UpdateTaskStatus(task.Id, "InProgress");
            // }
            return task;
        }
    }
}
