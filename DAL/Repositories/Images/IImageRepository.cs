using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.Images
{
    public interface IImageRepository
    {
        // CRUD
        IEnumerable<Image> GetAll();
        Image? GetById(int id);
        int Add(Image image);
        void Delete(int id);

        // Helper metode (admin)
        IEnumerable<Image> SearchByFileName(string? query, int page, int pageSize);
        int Count(string? query);
    }
}
