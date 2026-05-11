using ImageGallery.Db.Models;

namespace ImageGalleryApp.Services
{
    public class ImagesFileProvider : IImagesFileProvider
    {
        readonly IWebHostEnvironment _environment;
        readonly ILogger<ImagesFileProvider> _logger;

        public ImagesFileProvider(IWebHostEnvironment environment, ILogger<ImagesFileProvider> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Сохраняет файл в папку и возвращает путь к нему
        /// </summary>
        /// <param name="image">Модель изображения</param>
        /// <param name="file">Файл изображения</param>
        /// <returns>Путь к файлу на диске</returns>
        /// <exception cref="ArgumentNullException">Один и аргументов метода был null</exception>
        public async Task<string?> SaveAsync(ImageModel image, IFormFile file)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = file.FileName;

                // Если пользователь ввел заголовок, создаем файл с названием заголовка
                if (image.Title != null)
                    fileName = image.Title + Path.GetExtension(file.FileName);

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return "/uploads/" + fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения файла изображения");
                return null;
            }
        }

        public bool Delete(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException(nameof(path));

                var filePath = Path.Combine(_environment.WebRootPath, path.TrimStart('/'));

                if (!File.Exists(filePath))
                    throw new ArgumentException("Файл не существует", nameof(path));

                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления файла изображения");
                return false;
            }
        }
    }
}
