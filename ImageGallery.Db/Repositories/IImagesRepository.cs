using ImageGallery.Db.Models;

namespace ImageGallery.Db.Repositories
{
    public interface IImagesRepository
    {
        Task<List<Image>> GetAllAsync();
        Task<Image?> TryGetByIdAsync(int id);
        Task AddAsync(Image image);
        Task UpdateAsync(Image image);
        Task DeleteAsync(int id);
    }
}
