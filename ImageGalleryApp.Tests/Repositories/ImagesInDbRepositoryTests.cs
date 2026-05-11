using Castle.Core.Logging;
using FluentAssertions;
using ImageGallery.Db;
using ImageGallery.Db.Models;
using ImageGallery.Db.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryApp.Tests.Repositories
{
    public class ImagesInDbRepositoryTests
    {
        private async Task<DatabaseContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new DatabaseContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return dbContext;
        }

        [Fact]
        public async Task AddAsync_WithValidImage_ShouldReturnTrue()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            var image = new ImageModel
            {
                FileName = "test.jpg",
                Title = "Test Image",
                FilePath = "/uploads/test.jpg"
            };

            // Act
            var result = await repository.AddAsync(image); 

            // Assert
            result.Should().BeTrue();
            var savedImage = await context.Images.FirstOrDefaultAsync();
            savedImage.Should().NotBeNull();
            savedImage.Title.Should().Be("Test Image");
            savedImage.Description.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var context = await GetDatabaseContext();
            await context.DisposeAsync(); // Закрываем контекст для вызова ошибки

            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            var image = new ImageModel();

            // Act
            var result = await repository.AddAsync(image);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_WithImages_ShouldReturnAllImages()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            await context.Images.AddRangeAsync(
                new ImageModel { FileName = "img1.jpg", FilePath = "/img1.jpg" },
                new ImageModel { FileName = "img2.jpg", FilePath = "/img2.jpg" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result?.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoImages_ShouldReturnEmptyList()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result?.Should().BeEmpty();
        }

        [Fact]
        public async Task TryGetByIdAsync_WithValidId_ShouldReturnImage()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            var image = new ImageModel { FileName = "test.jpg", FilePath = "/test.jpg" };
            await context.Images.AddAsync(image);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.TryGetByIdAsync(image.Id);

            // Assert
            result.Should().NotBeNull();
            result?.Id.Should().Be(image.Id);
        }

        [Fact]
        public async Task TryGetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            // Act
            var result = await repository.TryGetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateImage()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            var image = new ImageModel
            {
                FileName = "test.jpg",
                FilePath = "/test.jpg",
                Title = "Old Title",
                Description = "Old Description"
            };
            await context.Images.AddAsync(image);
            await context.SaveChangesAsync();

            image.Title = "New Title";
            image.Description = "New Description";

            // Act
            var result = await repository.UpdateAsync(image);

            // Assert
            result.Should().BeTrue();

            var updated = await context.Images.FirstOrDefaultAsync();
            updated?.Title.Should().Be("New Title");
            updated?.Description.Should().Be("New Description");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveImage()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            var image = new ImageModel { FileName = "test.jpg", FilePath = "/test.jpg" };
            await context.Images.AddAsync(image);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.DeleteAsync(image);

            // Assert
            result.Should().BeTrue();
            var count = await context.Images.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_WithNullImage_ShouldReturnFalse()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var logger = new Mock<ILogger<ImagesInDbRepository>>().Object;
            var repository = new ImagesInDbRepository(context, logger);

            // Act
            var result = await repository.DeleteAsync(null!);

            // Assert
            result.Should().BeFalse();
        }
    }
}
