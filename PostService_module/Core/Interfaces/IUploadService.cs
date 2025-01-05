
using PostService_module.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostService_module.Core.Interfaces
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(IFormFile file);
        void DeleteImage(string imageUrl);
    }
}
