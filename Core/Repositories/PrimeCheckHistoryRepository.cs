using Core.Data;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace Core.Repositories
{
    public class PrimeCheckHistoryRepository
    {
        private AppDbContext _context;


        public PrimeCheckHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(PrimeCheckHistory primeCheckHistory)
        {
            lock (_context)
            {
                _context.PrimeCheckHistory.Add(primeCheckHistory);
                _context.SaveChanges();
            }
        }

        public List<PrimeCheckHistory> GetByUserId(string userId)
        {
            lock (_context)
            {
                return _context.PrimeCheckHistory.Where(x => x.UserId == userId).ToList();
            }
            //return _context.PrimeCheckHistory.Where(x=>x.UserId == userId).ToList();
        }

        public PrimeCheckHistory GetById(int taskId)
        {
            lock (_context)
            {
                return _context.PrimeCheckHistory.FirstOrDefault(x => x.Id == taskId)!;
            }
            //return _context.PrimeCheckHistory.FirstOrDefault(x => x.Id == taskId)!;
        }

        public List<PrimeCheckHistory> GetAll(int progress =-100)
        {
            lock (_context)
            {
                if (progress != -100)
                {
                    return _context.PrimeCheckHistory.Where(x => x.Progress == progress).ToList();
                }
                return _context.PrimeCheckHistory.ToList();
            }
        }
        public List<PrimeCheckHistory> GetAllInRgogress()
        {
            lock (_context)
            {
                return _context.PrimeCheckHistory.Where(x => x.Progress > -1 && x.Progress < 100).ToList();
            }
            //return _context.PrimeCheckHistory
            //    .Where(x => x.Progress > -1 
            //                && x.Progress < 100)
            //    .ToList();
        }

        public PrimeCheckHistory GetNextTask()
        {
            lock(_context)
            {
                return _context.PrimeCheckHistory
                    .OrderBy(x => x.RequestDateTime)
                    .FirstOrDefault(x => x.Progress == -3)!;
            }
            return _context.PrimeCheckHistory
             .FirstOrDefault(x => x.Progress == -3)!;
        }

        public Task<int> GetActiveTaskCountAsync(string userId)
        {
            lock(_context)
            {
                return _context.PrimeCheckHistory
                    .CountAsync(x => x.UserId == userId
                        && x.Progress != 100
                        && x.Progress != -1);
            }
            return _context.PrimeCheckHistory
            .CountAsync(x => x.UserId == userId
                && x.Progress != 100
                && x.Progress != -1);
        }

        public PrimeCheckHistory UpdateProgress(PrimeCheckHistory primeCheckHistory)
        {
            lock(_context)
            {
                _context.PrimeCheckHistory.Update(primeCheckHistory);
                _context.SaveChanges();
            }
            //_context.PrimeCheckHistory.Update(primeCheckHistory);
            //_context.SaveChanges();

            return primeCheckHistory;
        }

        public PrimeCheckHistory UpdateProgress(int taskId, double progress)
        {
            lock (_context)
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

                return progressEntry;
            }
        }

        public bool Update(PrimeCheckHistory primeCheckHistory)
        {
            lock(_context)
            {
                _context.PrimeCheckHistory.Update(primeCheckHistory);
                _context.SaveChanges();
            }
            //_context.PrimeCheckHistory.Update(primeCheckHistory);
            //_context.SaveChanges();

            return true;
        }

    }
}
