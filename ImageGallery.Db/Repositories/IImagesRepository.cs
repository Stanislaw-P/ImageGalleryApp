using ImageGallery.Db.Models;

namespace ImageGallery.Db.Repositories
{
    public interface IImagesRepository
    {
        Task<bool> AddAsync(ImageModel image);
        Task<bool> DeleteAsync(int id);
        Task<List<ImageModel>?> GetAllAsync();
        Task<ImageModel?> TryGetByIdAsync(int id);
        Task<bool> UpdateAsync(ImageModel image);
    }
}