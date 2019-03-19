using System.Collections.Generic;
using System.Linq;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Models.Logger;

namespace MobileManager.Database.Repositories
{
    public class LoggerRepository : IRepository<LogMessage>
    {
        private readonly GeneralDbContext _context;

        public LoggerRepository(GeneralDbContext context)
        {
            _context = context;
        }

        public void Add(LogMessage entity)
        {
            _context.Logger.Add(entity);
            _context.SaveChanges();
        }

        public void Add(IEnumerable<LogMessage> entity)
        {
            _context.Logger.AddRange(entity);
            _context.SaveChanges();
        }

        public IEnumerable<LogMessage> GetAll()
        {
            return _context.Logger.OrderBy(d => d.Id).ToList();
        }

        public LogMessage Find(string id)
        {
            return _context.Logger.Single(d => d.Id == int.Parse(id));
        }

        public bool Remove(string id)
        {
            _context.Logger.Remove(Find(id));
            _context.SaveChanges();

            return true;
        }

        public void Update(LogMessage entity)
        {
            _context.Logger.Update(entity);
            _context.SaveChanges();
        }
    }
}
