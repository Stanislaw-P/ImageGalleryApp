using FluentAssertions;
using ImageGallery.Db.Models;
using ImageGalleryApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryApp.Tests.Services
{
    public class ImagesFileProviderTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<ImagesFileProvider>> _mockLogger;
        private readonly ImagesFileProvider _fileProvider;
        private readonly string _testUploadPath;

        public ImagesFileProviderTests()
        {
            _testUploadPath = Path.Combine(Path.GetTempPath(), "gallery_test");
            if (!Directory.Exists(_testUploadPath))
                Directory.CreateDirectory(_testUploadPath);

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<ImagesFileProvider>>();

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(_testUploadPath);

            _fileProvider = new ImagesFileProvider(_mockEnvironment.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveAsync_WithValidImage_ShouldReturnFilePath()
        {
            // Arrange
            var image = new ImageModel { Title = "testImage" };
            var file = CreateMockFormFile("photo.jpg", "fake image content");

            // Act
            var result = await _fileProvider.SaveAsync(image, file.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("/uploads/");
            result.Should().Contain("testImage.jpg");

            // Cleanup
            if (result != null)
                File.Delete(Path.Combine(_testUploadPath, result.TrimStart('/')));
        }

        [Fact]
        public async Task SaveAsync_WithNullImage_ShouldThrowArgumentNullException()
        {
            // Arrange
            var file = CreateMockFormFile("test.jpg", "content");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _fileProvider.SaveAsync(null!, file.Object)
            );
        }

        [Fact]
        public async Task SaveAsync_WithNullFile_ShouldThrowArgumentNullException()
        {
            // Arrange
            var image = new ImageModel { Title = "test" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _fileProvider.SaveAsync(image, null!)
            );
        }

        [Fact]
        public async Task SaveAsync_WithoutTitle_ShouldUseOriginalFileName()
        {
            // Arrange
            var image = new ImageModel { Title = null };
            var file = CreateMockFormFile("original_name.jpg", "content");

            // Act
            var result = await _fileProvider.SaveAsync(image, file.Object);

            // Assert
            result.Should().Contain("original_name.jpg");

            // Cleanup
            if (result != null)
                File.Delete(Path.Combine(_testUploadPath, result.TrimStart('/')));
        }

        [Fact]
        public async Task SaveAsync_WhenExceptionOccurs_ShouldReturnNullAndLogError()
        {
            // Arrange
            _mockEnvironment.Setup(x => x.WebRootPath).Throws(new IOException("Disk full"));
            var image = new ImageModel { Title = "test" };
            var file = CreateMockFormFile("test.jpg", "content");

            // Act
            var result = await _fileProvider.SaveAsync(image, file.Object);

            // Assert
            result.Should().BeNull();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Delete_WithValidPath_ShouldDeleteFileAndReturnTrue()
        {
            // Arrange
            var fileName = "delete_test.jpg";
            var fullPath = Path.Combine(_testUploadPath, "uploads", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, "test content");

            // Act
            var result = _fileProvider.Delete($"/uploads/{fileName}");

            // Assert
            result.Should().BeTrue();
            File.Exists(fullPath).Should().BeFalse();
        }

        [Fact]
        public void Delete_WithInvalidPath_ShouldReturnFalseAndLogError()
        {
            // Arrange
            var invalidPath = "/uploads/nonexistent.jpg";

            // Act
            var result = _fileProvider.Delete(invalidPath);

            // Assert
            result.Should().BeFalse();
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Delete_WithNullPath_ShouldReturnFalse()
        {
            // Act
            var result = _fileProvider.Delete(null!);

            // Assert
            result.Should().BeFalse();
        }

        private Mock<IFormFile> CreateMockFormFile(string fileName, string content)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                })
                .Returns(Task.CompletedTask);

            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            return fileMock;
        }
    }
}
