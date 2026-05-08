namespace ImageGalleryApp.Models
{
    public class ImageUploadDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}
