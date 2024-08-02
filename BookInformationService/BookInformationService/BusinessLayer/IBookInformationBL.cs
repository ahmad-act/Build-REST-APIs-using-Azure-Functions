using BookInformationService.DTOs;
using BookInformationService.Models;

namespace BookInformationService.BusinessLayer
{
    public interface IBookInformationBL
    {
        Task<BookInformationDisplayDto?> CreateBookInformation(BookInformationUpdateDto bookInformationUpdateDto);
        Task<BookInformationDisplayDto> DeleteBookInformation(Guid id);
        Task<BookInformationDisplayDto?> GetBookInformation(Guid id);
        Task<List<BookInformationDisplayDto>?> GetBookInformations();
        Task<List<BookInformationDisplayDto>?> SearchBookInformations(string searchTerm);
        Task<BookInformationDisplayDto?> UpdateBookInformation(Guid id, BookInformationUpdateDto bookInformationUpdateDto);
    }
}