using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Supporters.CustomIdentityProvider
{
    public class UserStore : IUserStore<User>, IUserRoleStore<User>, IUserPasswordStore<User>
    {
         ChatAppImplementationContext _context;

        public UserStore(ChatAppImplementationContext context)
        {
            _context = context;
        }

        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = _context.Roles.Where(r => r.Name == roleName).FirstOrDefault();
            if(role == null)
            {
                throw new Exception("Cannot find role");
            }    
            _context.UserRoles.Add(new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id,
                RoleName = roleName,
            });
            _context.SaveChanges();
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot insert data due to exception: " + ex.ToString() });
            }
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _context.Users.Where(user => user.Username.Equals(normalizedUserName)).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.Username);
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.Password);
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            var roles = _context.UserRoles.AsNoTracking().Where(userRole => userRole.UserId == user.Id).Select(userRole => userRole.RoleName).ToList();
            return Task.FromResult<IList<string>>(roles);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username);
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var users = await _context.UserRoles.AsNoTracking()
                .Where(userRole => userRole.RoleName == roleName)
                .Include(userRole => userRole.User)
                .Select(userRole => userRole.User)
                .ToListAsync();
            return users;
        }

        public async Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return user != null && user.Password != null;
        }

        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var result = await _context.UserRoles.Where(userInList => userInList.Id == user.Id).ToListAsync();
            if(result == null)
            {
                return false;
            }
            foreach(var userRole in result)
            {
                if(userRole.RoleName == roleName)
                {
                    return true;
                }
            }
            return false;
        }

        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var userRole = _context.UserRoles.Where(userRole => userRole.UserId == user.Id && userRole.RoleName == roleName).FirstOrDefault();
            _context.UserRoles.Remove(userRole);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.Username = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError() { Description = "Error due to: " + ex.ToString() });
            }
        }
    }
}
