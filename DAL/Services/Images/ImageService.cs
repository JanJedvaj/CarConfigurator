using DAL.Models;
using DAL.Repositories.Images;
using System;
using System.Collections.Generic;

namespace DAL.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public IEnumerable<Image> GetAll()
            => _imageRepository.GetAll();

        public Image? GetById(int id)
            => _imageRepository.GetById(id);

        public int Create(Image image)
        {
            Validate(image);
            return _imageRepository.Add(image);
        }

        public void Delete(int id)
        {
            _imageRepository.Delete(id);
        }

        public IEnumerable<Image> SearchByFileName(string? query, int page, int pageSize)
            => _imageRepository.SearchByFileName(query, page, pageSize);

        public int Count(string? query)
            => _imageRepository.Count(query);

        private static void Validate(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (string.IsNullOrWhiteSpace(image.FileName))
                throw new InvalidOperationException("FileName is required.");

            if (string.IsNullOrWhiteSpace(image.ContentType))
                throw new InvalidOperationException("ContentType is required.");

            if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only image/* content types are allowed.");

            if (image.Length <= 0)
                throw new InvalidOperationException("Length must be > 0.");

            if (string.IsNullOrWhiteSpace(image.StoragePathOrUrl))
                throw new InvalidOperationException("StoragePathOrUrl is required.");

            if (image.Width.HasValue && image.Width.Value <= 0)
                throw new InvalidOperationException("Width must be > 0.");

            if (image.Height.HasValue && image.Height.Value <= 0)
                throw new InvalidOperationException("Height must be > 0.");
        }
    }
}
