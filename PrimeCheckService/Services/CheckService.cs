using Core.Repositories;
using System;
using System.Threading.Tasks;

namespace PrimeCheckService.Services;

public class CheckService
{
    private readonly PrimeCheckHistoryRepository _primeCheckHistoryRepository;
    private readonly CancelationTokenRepository _cancelationTokenRepository;
    private const int DelayMilliseconds = 100;
    private const double MinProgressUpdate = 1.0; // Minimum progress update percentage

    public CheckService(PrimeCheckHistoryRepository primeCheckHistoryRepository, CancelationTokenRepository cancelationTokenRepository)
    {
        _primeCheckHistoryRepository = primeCheckHistoryRepository ?? throw new ArgumentNullException(nameof(primeCheckHistoryRepository));
        _cancelationTokenRepository = cancelationTokenRepository ?? throw new ArgumentNullException(nameof(cancelationTokenRepository));
    }

    public async Task IsPrime(int taskId)
    {
        var task = _primeCheckHistoryRepository.GetById(taskId);
        if (task == null) 
            throw new Exception($"Task with ID {taskId} not found.");

        var cancellationToken = _cancelationTokenRepository.GetByPrimeCheckHistoryId(taskId);
        if (cancellationToken == null)
            throw new Exception($"Cancellation token for task ID {taskId} not found.");

        try
        {
            int number = task.Number;
            int count = 0;
            double lastProgressUpdate = 0;
            
            // Check initial cancellation state
            await CheckCancellation(taskId, cancellationToken);

            for (int i = 1; i <= number; i++)
            {
                // Only check cancellation every few iterations to reduce database calls
                if (i % 10 == 0)
                {
                    await CheckCancellation(taskId, cancellationToken);
                }

                if (number % i == 0)
                    count++;

                // Calculate current progress
                double currentProgress = ((double)i / number) * 100;

                // Update progress if we've made significant progress
                if (currentProgress - lastProgressUpdate >= MinProgressUpdate)
                {
                    await UpdateProgress(taskId, currentProgress);
                    lastProgressUpdate = currentProgress;
                }

                await Task.Delay(DelayMilliseconds);
            }

            // Update final state
            await UpdateProgress(taskId, 100);
            task.IsPrime = count == 2;
            _primeCheckHistoryRepository.Update(task);
        }
        catch (OperationCanceledException)
        {
            await HandleCancellation(taskId);
            throw; // Rethrow to let caller handle cancellation
        }
        catch (Exception)
        {
            // Mark task as failed
            task.Progress = -2; // Assuming -2 represents failed state
            _primeCheckHistoryRepository.Update(task);
            throw;
        }
    }

    private async Task CheckCancellation(int taskId, Core.Models.CancellationToken cancellationToken)
    {
        // Refresh cancellation token state
        var currentToken = _cancelationTokenRepository.GetByPrimeCheckHistoryId(taskId);
        if (currentToken != null && currentToken.IsCanceled)
        {
            throw new OperationCanceledException($"Task {taskId} was cancelled.");
        }
    }

    private async Task HandleCancellation(int taskId)
    {
        var task = _primeCheckHistoryRepository.GetById(taskId);
        if (task != null)
        {
            task.Progress = -1; // Cancelled state
            _primeCheckHistoryRepository.Update(task);
        }
    }

    private async Task UpdateProgress(int taskId, double progress)
    {
        _primeCheckHistoryRepository.UpdateProgress(taskId, progress);
    }
}
