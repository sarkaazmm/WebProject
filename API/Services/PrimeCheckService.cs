using API.Data;
using API.Models;
using System;

namespace API.Services;
public static class PrimeCheckService
{

    public static async System.Threading.Tasks.Task IsPrime(int taskId, AppDbContext _context)
    {
        int count = 0;
        var task = _context.PrimeCheckHistory
            .FirstOrDefault(p => p.Id == taskId);

        if (task == null) throw new Exception("Task not found.");

        int number = task.Number;
        int progressStep = number / 100 * 5;
        int currentProgress = progressStep;

        var cancellationToken = _context.CancellationToken
            .FirstOrDefault(p => p.PrimeCheckHistoryId == taskId);

        for (int i = 1; i <= number; i += 1)
        {
            if (cancellationToken == null) throw new Exception("Task not found.");
            
            await _context.Entry(cancellationToken).ReloadAsync();

            if (cancellationToken.IsCanceled)
            { 
                task.Progress = -1;
                throw new OperationCanceledException();
            }
            
            if (number % i == 0) count++;
            await System.Threading.Tasks.Task.Delay(100);  

            if (i == currentProgress)
            {
                UpdateProgress(taskId, (double)i / number * 100, _context);
                currentProgress += progressStep;
            }
        }

        UpdateProgress(taskId, 100, _context);

        task.IsPrime = count == 2;
        _context.PrimeCheckHistory.Update(task);
        await _context.SaveChangesAsync();
    }

    private static void UpdateProgress(int taskId, double progress, AppDbContext _context)
    {
        var progressEntry = _context.PrimeCheckHistory
            .FirstOrDefault(p => p.Id == taskId);

        if (progressEntry == null)
        {
            throw new Exception("Task not found.");
        }

        progressEntry.Progress = (int)progress;
        _context.PrimeCheckHistory.Update(progressEntry);
        _context.SaveChanges();
    }
}
