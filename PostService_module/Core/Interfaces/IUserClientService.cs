
using PostService_module.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostService_module.Core.Interfaces
{
    public interface IUserClientService
    {
        Task<UserDTO> GetUserByIdAsync(int userId);
    }
}
