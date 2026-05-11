using ImageGallery.Db.Models;

namespace ImageGalleryApp.Services
{
    public interface IImagesFileProvider
    {
        bool Delete(string path);
        Task<string?> SaveAsync(ImageModel image, IFormFile file);
    }
}