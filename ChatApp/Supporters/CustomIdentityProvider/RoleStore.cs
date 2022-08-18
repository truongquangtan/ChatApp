using Microsoft.AspNetCore.Identity;

using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Supporters.CustomIdentityProvider
{
    public class RoleStore : IRoleStore<Role>
    {
        private readonly ChatAppImplementationContext _context;

        public RoleStore(ChatAppImplementationContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot insert data due to exception: " + ex.ToString() });
            }
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot remove data due to exception: " + ex.ToString() });
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return Task.FromResult<Role>(_context.Roles.Where(r => r.Name.Equals(normalizedRoleName)).FirstOrDefault());
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(role.Name);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(role.Id);
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            role.Name = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error due to: " + ex.ToString() });
            }
        }
    }
}
