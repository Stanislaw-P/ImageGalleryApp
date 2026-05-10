using ImageGallery.Db.Models;
using ImageGallery.Db.Repositories;
using ImageGalleryApp.Helpers;
using ImageGalleryApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageGalleryApp.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        readonly IImagesRepository _imagesRepository;
        readonly ImagesFileProvider _imagesFileProvider;

        public ImagesController(IImagesRepository imagesRepository, ImagesFileProvider imagesFileProvider)
        {
            _imagesRepository = imagesRepository;
            _imagesFileProvider = imagesFileProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var images = await _imagesRepository.GetAllAsync();
            if (images == null)
                return NotFound(images);

            return Ok(images);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(ImageUploadDto request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("Файл не выбран");

            var image = new ImageModel
            {
                Title = request.Title,
                Description = request.Description,
                FileName = request.File.FileName // Если title будет null, так можно будет определить название
            };

            var filePath = await _imagesFileProvider.SaveAsync(image, request.File);
            if (filePath == null)
                return BadRequest("Не удалось сохранить файл изображения");

            image.FilePath = filePath;

            var result = await _imagesRepository.AddAsync(image);
            if (result == false)
                return BadRequest("Не удалось сохранить запись в БД");

            return Ok(image);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id == 0)
                return BadRequest("id должен быть больше 0");

            var existingImage = await _imagesRepository.TryGetByIdAsync(id);
            if (existingImage == null)
                return NotFound();

            var result = _imagesFileProvider.Delete(existingImage.FilePath);
            if (result == false)
                return BadRequest("Не удалось удалить файл изображения с сервера");

            await _imagesRepository.DeleteAsync(existingImage);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, ImageUpdateDto request)
        {
            if (id == 0)
                return BadRequest("id должен быть больше 0");

            var existingImage = await _imagesRepository.TryGetByIdAsync(id);
            if (existingImage == null)
                return NotFound();

            existingImage.Title = request?.Title?.Trim();
            existingImage.Description = request?.Description?.Trim();

            await _imagesRepository.UpdateAsync(existingImage);

            return NoContent();
        }
    }
}
