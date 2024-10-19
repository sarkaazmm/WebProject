using API.Data;
using API.Models;
using System;

namespace API.Services
{
    public class PrimeCheckService
    {
        private readonly AppDbContext _context;

        public PrimeCheckService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsPrime(int number, string userId)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));
            int progressStep = boundary / 20; // 5% of the range
            int nextProgressUpdate = progressStep;

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;

                if (i >= nextProgressUpdate)
                {
                    System.Threading.Thread.Sleep(2000);
                    int progress = (int)(((double)i / boundary) * 100);
                    UpdateProgress(userId, number, progress);
                    nextProgressUpdate += progressStep;
                }
            }

            return true;
        }

        private void UpdateProgress(string userId, int number, int progress)
        {
            var progressEntry = _context.PrimeCheckHistory
                .FirstOrDefault(p => p.UserId == userId && p.Number == number);

            if (progressEntry == null)
            {
                progressEntry = new PrimeCheckHistory
                {
                    UserId = userId,
                    Number = number,
                    Progress = progress,
                };
                _context.PrimeCheckHistory.Add(progressEntry);
            }
            else
            {
                progressEntry.Progress = progress;
                _context.PrimeCheckHistory.Update(progressEntry);
            }

            _context.SaveChanges();
        }
    }
}

