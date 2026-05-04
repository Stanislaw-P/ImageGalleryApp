using ImageGallery.Db.Models;
using Microsoft.EntityFrameworkCore;
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

        public ImagesInDbRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task AddAsync(Image image)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            var existingImg = await TryGetByIdAsync(id);
            if (existingImg == null)
            {
                return;
            }
        }

        public async Task<List<Image>> GetAllAsync()
        {
            return await _context.Images.ToListAsync();
        }

        public async Task<Image?> TryGetByIdAsync(int id)
        {
            return await _context.Images.FirstOrDefaultAsync(img => img.Id == id);
        }

        public async Task UpdateAsync(Image image)
        {
            _context.Images.Update(image);
            await _context.SaveChangesAsync();
        }
    }
}
