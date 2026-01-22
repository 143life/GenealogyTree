using System.Security.Claims;
using Genealogy.API.Dtos;

namespace Genealogy.API.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(UserCreateDto userCreateDto);
    Task<TokenResponseDto> LoginAsync(UserLoginDto userLoginDto);
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> UpdateUserAsync(int id, UserUpdateDto userUpdateDto);
    Task<bool> DeleteUserAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal user);
}