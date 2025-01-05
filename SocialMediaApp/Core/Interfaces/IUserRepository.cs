using UserService.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> FindByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByIdAsync(int id);  
    }
}
