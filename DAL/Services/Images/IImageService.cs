using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Images
{
    public interface IImageService
    {
        IEnumerable<Image> GetAll();
        Image? GetById(int id);

        int Create(Image image);
        void Delete(int id);

        IEnumerable<Image> SearchByFileName(string? query, int page, int pageSize);
        int Count(string? query);
    }
}
