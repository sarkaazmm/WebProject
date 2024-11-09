using Core.Repositories;

namespace PrimeCheckService.Services
{
    public class CheckService
    {
        private readonly PrimeCheckHistoryRepository _primeCheckHistoryRepository;
        private readonly CancelationTokenRepository _cancelationTokenRepository;


        public CheckService(PrimeCheckHistoryRepository primeCheckHistoryRepository, CancelationTokenRepository cancelationTokenRepository)
        {
            _primeCheckHistoryRepository = primeCheckHistoryRepository;
            _cancelationTokenRepository = cancelationTokenRepository;
        }

        public async Task IsPrime(int taskId)
        {
            int count = 0;
            var task = _primeCheckHistoryRepository.GetById(taskId);

            if (task == null) throw new Exception("Task not found.");

            int number = task.Number;
            int progressStep = number / 100;// * 5/5;
            int currentProgress = progressStep;

            var cancellationToken = _cancelationTokenRepository.GetByPrimeCheckHistoryId(taskId);

            for (int i = 1; i <= number; i += 1)
            {
                if (cancellationToken == null) throw new Exception("Task not found.");
                
                cancellationToken = _cancelationTokenRepository.GetByPrimeCheckHistoryId(taskId);


                if (cancellationToken.IsCanceled)
                {
                    task.Progress = -1;
                    throw new OperationCanceledException();
                }

                if (number % i == 0) count++;
                await System.Threading.Tasks.Task.Delay(100);

                if (i == currentProgress)
                {
                    _primeCheckHistoryRepository.UpdateProgress(taskId, (double)i / number * 100);
                    //UpdateProgress(taskId, (double)i / number * 100, _context);
                    currentProgress += progressStep;
                }
            }

            _primeCheckHistoryRepository.UpdateProgress(taskId, 100);
            //UpdateProgress(taskId, 100, _context);

            task.IsPrime = count == 2;

            _primeCheckHistoryRepository.Update(task);

            //await _context.SaveChangesAsync();
        }
    }
}
