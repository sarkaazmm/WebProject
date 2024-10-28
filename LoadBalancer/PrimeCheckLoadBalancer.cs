using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace LoadBalancer;

public class PrimeCheckPolicy : ILoadBalancingPolicy
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, int> _serverTaskCounts = new();
    private int _roundRobinCounter = 0;

    public PrimeCheckPolicy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name => "PrimeCheckPolicy";

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        Console.WriteLine("Picking destination");
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        // Check if this is a prime check creation request
        if (context.Request.Path.StartsWithSegments("/api/primechackhistory/create") && 
            context.Request.Method == "POST")
        {
            Console.WriteLine("Picking destination for prime check creation request");
            var dist = PickLeastBusyDestination(availableDestinations);
            Console.WriteLine($"Selected server {dist.DestinationId}");
            return dist;
        }
        else
        {
            Console.WriteLine("Picking destination for prime check creation request");
            var dist = PickRoundRobinDestination(availableDestinations);
            Console.WriteLine($"Selected server {dist.DestinationId}");
            return dist;
        }
    }

    private DestinationState PickLeastBusyDestination(IReadOnlyList<DestinationState> availableDestinations)
    {
        DestinationState leastBusyDestination = availableDestinations[0];
        int minTasks = int.MaxValue;

        foreach (var destination in availableDestinations)
        {
            var tasksCount = GetRunningTasksCount(destination.Model.Config.Address).Result;
            _serverTaskCounts.AddOrUpdate(destination.Model.Config.Address, tasksCount, (_, _) => tasksCount);

            if (tasksCount < minTasks)
            {
                minTasks = tasksCount;
                leastBusyDestination = destination;
            }
        }

        Console.WriteLine($"Selected server {leastBusyDestination.DestinationId} with {minTasks} running tasks");
        return leastBusyDestination;
    }

    private DestinationState PickRoundRobinDestination(IReadOnlyList<DestinationState> availableDestinations)
    {
        var index = Interlocked.Increment(ref _roundRobinCounter) % availableDestinations.Count;
        return availableDestinations[index];
    }

    private async Task<int> GetRunningTasksCount(string serverAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{serverAddress}/api/primechackhistory/running-tasks-count");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Running tasks count from {serverAddress}: {content}");
                
                var result = JsonSerializer.Deserialize<TaskCountResponse>(content);
                if (result != null)
                {
                    return result.count;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting running tasks count from {serverAddress}: {ex.Message}");
        }

        // Return 0 in case of any errors to make the server eligible for new tasks
        return 0;
    }

    private class TaskCountResponse
    {
        public int count { get; set; }
    }
}
