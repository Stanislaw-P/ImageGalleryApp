using FluentAssertions;
using ImageGallery.Db.Models;
using ImageGallery.Db.Repositories;
using ImageGalleryApp.Controllers;
using ImageGalleryApp.Models;
using ImageGalleryApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryApp.Tests.Controllers
{
    public class ImagesControllerTests
    {
        private readonly Mock<IImagesRepository> _mockRepository;
        private readonly Mock<IImagesFileProvider> _mockFileProvider;
        private readonly ImagesController _controller;

        public ImagesControllerTests()
        {
            _mockRepository = new Mock<IImagesRepository>();

            _mockFileProvider = new Mock<IImagesFileProvider>();

            _controller = new ImagesController(_mockRepository.Object, _mockFileProvider.Object);
        }

        [Fact]
        public async Task GetAllAsync_WhenImagesExist_ShouldReturnOkWithImages()
        {
            // Arrange
            var expectedImages = new List<ImageModel>
        {
            new() { Id = 1, FileName = "img1.jpg", Title = "Image 1" },
            new() { Id = 2, FileName = "img2.jpg", Title = "Image 2" }
        };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedImages);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);
            okResult?.Value.Should().BeEquivalentTo(expectedImages);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoImages_ShouldReturnNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ImageModel>?)null);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnOkWithImage()
        {
            // Arrange
            var request = new ImageUploadDto
            {
                Title = "Test Image",
                Description = "Test Description",
                File = CreateMockFormFile("test.jpg")
            };

            var expectedPath = "/uploads/Test Image.jpg";
            _mockFileProvider.Setup(f => f.SaveAsync(It.IsAny<ImageModel>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedPath);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ImageModel>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult?.StatusCode.Should().Be(200);

            var image = okResult?.Value as ImageModel;
            image.Should().NotBeNull();
            image?.Title.Should().Be("Test Image");
            image?.Description.Should().Be("Test Description");
            image?.FilePath.Should().Be(expectedPath);
        }

        [Fact]
        public async Task CreateAsync_WithoutFile_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ImageUploadDto { Title = "Test", File = null };

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Файл не выбран");
        }

        [Fact]
        public async Task CreateAsync_WhenFileSaveFails_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ImageUploadDto
            {
                Title = "Test",
                File = CreateMockFormFile("test.jpg")
            };

            _mockFileProvider.Setup(f => f.SaveAsync(It.IsAny<ImageModel>(), It.IsAny<IFormFile>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Не удалось сохранить файл изображения");
        }

        [Fact]
        public async Task CreateAsync_WhenDbSaveFails_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ImageUploadDto
            {
                Title = "Test",
                File = CreateMockFormFile("test.jpg")
            };

            _mockFileProvider.Setup(f => f.SaveAsync(It.IsAny<ImageModel>(), It.IsAny<IFormFile>()))
                .ReturnsAsync("/uploads/test.jpg");
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ImageModel>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Не удалось сохранить запись в БД");
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var existingImage = new ImageModel { Id = 1, FilePath = "/uploads/test.jpg" };
            _mockRepository.Setup(r => r.TryGetByIdAsync(1)).ReturnsAsync(existingImage);
            _mockFileProvider.Setup(f => f.Delete(existingImage.FilePath)).Returns(true);
            _mockRepository.Setup(r => r.DeleteAsync(existingImage)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepository.Verify(r => r.DeleteAsync(existingImage), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithZeroId_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.DeleteAsync(0);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("id должен быть больше 0");
        }

        [Fact]
        public async Task DeleteAsync_WithNonexistentId_ShouldReturnNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.TryGetByIdAsync(999)).ReturnsAsync((ImageModel?)null);

            // Act
            var result = await _controller.DeleteAsync(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteAsync_WhenFileDeleteFails_ShouldReturnBadRequest()
        {
            // Arrange
            var existingImage = new ImageModel { Id = 1, FilePath = "/uploads/test.jpg" };
            _mockRepository.Setup(r => r.TryGetByIdAsync(1)).ReturnsAsync(existingImage);
            _mockFileProvider.Setup(f => f.Delete(existingImage.FilePath)).Returns(false);

            // Act
            var result = await _controller.DeleteAsync(1);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("Не удалось удалить файл изображения с сервера");
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var existingImage = new ImageModel
            {
                Id = 1,
                FileName = "old.jpg",
                Title = "Old Title",
                Description = "Old Description"
            };

            var request = new ImageUpdateDto
            {
                Title = "  New Title  ",
                Description = "  New Description  "
            };

            _mockRepository.Setup(r => r.TryGetByIdAsync(1)).ReturnsAsync(existingImage);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ImageModel>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAsync(1, request);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            // Проверяем, что значения обновились и были обрезаны
            existingImage.Title.Should().Be("New Title");
            existingImage.Description.Should().Be("New Description");
        }

        [Fact]
        public async Task UpdateAsync_WithZeroId_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ImageUpdateDto { Title = "New Title" };

            // Act
            var result = await _controller.UpdateAsync(0, request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest?.Value.Should().Be("id должен быть больше 0");
        }

        [Fact]
        public async Task UpdateAsync_WithNonexistentId_ShouldReturnNotFound()
        {
            // Arrange
            var request = new ImageUpdateDto { Title = "New Title" };
            _mockRepository.Setup(r => r.TryGetByIdAsync(999)).ReturnsAsync((ImageModel?)null);

            // Act
            var result = await _controller.UpdateAsync(999, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithNullRequest_ShouldNotUpdateValues()
        {
            // Arrange
            var existingImage = new ImageModel
            {
                Id = 1,
                Title = "Original Title",
                Description = "Original Description"
            };

            _mockRepository.Setup(r => r.TryGetByIdAsync(1)).ReturnsAsync(existingImage);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ImageModel>())).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAsync(1, null!);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            existingImage.Title.Should().BeNullOrEmpty();
            existingImage.Description.Should().BeNullOrEmpty();
        }

        private IFormFile CreateMockFormFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(100);
            return fileMock.Object;
        }
    }
}
