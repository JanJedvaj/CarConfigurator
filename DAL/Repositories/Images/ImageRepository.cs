using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.Images
{
    public class ImageRepository : IImageRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public ImageRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }

        //CRUD

        public IEnumerable<Image> GetAll()
        {
            return _context.Images
                .OrderByDescending(i => i.UploadedAt)
                .ToList();
        }

        public Image? GetById(int id)
        {
            return _context.Images.Find(id);
        }

        public int Add(Image image)
        {
            if (image.UploadedAt == default)
                image.UploadedAt = DateTime.UtcNow;

            _context.Images.Add(image);
            _context.SaveChanges();
            return image.Id;
        }

        public void Delete(int id)
        {
            var image = _context.Images.Find(id);
            if (image == null)
                return;

            // Ako je slika vezana uz komponente, prvo makni vezu
            var componentsUsingImage = _context.Components
                .Where(c => c.ImageId == id)
                .ToList();

            foreach (var c in componentsUsingImage)
            {
                c.ImageId = null;
            }

            _context.Images.Remove(image);
            _context.SaveChanges();
        }

        //Search + paging

        public int Count(string? query)
        {
            var q = _context.Images.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(i => i.FileName.Contains(query));

            return q.Count();
        }

        public IEnumerable<Image> SearchByFileName(string? query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var q = _context.Images.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(i => i.FileName.Contains(query));

            return q
                .OrderByDescending(i => i.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
