using BookInformationService.DatabaseContext;
using BookInformationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookInformationService.DataAccessLayer
{
    public class BookInformationDL : IBookInformationDL
    {
        private readonly ILogger<BookInformationDL> _logger;
        private readonly AppDbContext _dbContext;

        public BookInformationDL(ILogger<BookInformationDL> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<BookInformation>?> GetBookInformations()
        {
            return await _dbContext.BookInformations.ToListAsync();
        }

        public async Task<BookInformation?> GetBookInformation(Guid id)
        {
            return await _dbContext.BookInformations.FindAsync(id);
        }

        public async Task<int> CreateBookInformation(BookInformation bookInformation)
        {
            _dbContext.BookInformations.Add(bookInformation);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateBookInformation(BookInformation bookInformation)
        {
            _dbContext.BookInformations.Update(bookInformation);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteBookInformation(BookInformation bookInformation)
        {
            _dbContext.BookInformations.Remove(bookInformation);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<List<BookInformation>?> SearchBookInformations(string searchTerm)
        {
            return await _dbContext.BookInformations
                .Where(b => b.Title.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }
    }
}
