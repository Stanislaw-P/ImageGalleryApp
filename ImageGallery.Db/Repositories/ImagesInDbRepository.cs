using ImageGallery.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGallery.Db.Repositories
{
    public class ImagesInDbRepository : IImagesRepository
    {
        readonly DatabaseContext _context;
        readonly ILogger<ImagesInDbRepository> _logger;

        public ImagesInDbRepository(DatabaseContext context, ILogger<ImagesInDbRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddAsync(ImageModel image)
        {
            try
            {
                await _context.Images.AddAsync(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения изображения");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var existingImg = await TryGetByIdAsync(id);
                if (existingImg == null)
                    return false;

                _context.Remove(existingImg);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка удаления изображения с id={id}");
                return false;
            }
        }

        public async Task<List<ImageModel>?> GetAllAsync()
        {
            try
            {
                return await _context.Images.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка изображений");
                return null;
            }
        }

        public async Task<ImageModel?> TryGetByIdAsync(int id)
        {
            try
            {
                return await _context.Images.FirstOrDefaultAsync(img => img.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения изображения по id");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(ImageModel image)
        {
            try
            {
                _context.Images.Update(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка обновления изображения с id={image.Id}");
                return false;
            }
        }
    }
}
