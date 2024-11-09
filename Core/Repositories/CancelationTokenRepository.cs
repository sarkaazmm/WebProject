using Core.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Repositories
{
    public class CancelationTokenRepository
    {
        private AppDbContext _context;


        public CancelationTokenRepository(AppDbContext context)
        {
            _context = context;
        }


        public void Add(Models.CancellationToken cancelationToken)
        {
            lock (_context)
            {
                _context.CancellationToken.Add(cancelationToken);
                _context.SaveChanges();
            }
            //_context.CancellationToken.Add(cancelationToken);
            //_context.SaveChanges();
        }

        public Models.CancellationToken GetByPrimeCheckHistoryId(int PrimeCheckHistoryId)
        {
            lock (_context)
            {
                return _context.CancellationToken.FirstOrDefault(x => x.PrimeCheckHistoryId == PrimeCheckHistoryId)!;
            }
            return _context.CancellationToken.FirstOrDefault(x => x.PrimeCheckHistoryId == PrimeCheckHistoryId)!;
        }

        public EntityEntry<Models.CancellationToken> GetEntry(Models.CancellationToken cancellationToken)
        {
            lock (_context)
            {
                return _context.Entry(cancellationToken);
            }
        }


        public Models.CancellationToken Update(Models.CancellationToken cancellationToken)
        {
            lock (_context)
            {
                _context.CancellationToken.Update(cancellationToken);
                _context.SaveChanges();
            }
            return cancellationToken;
        }
    }
}
