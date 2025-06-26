using AbpTemplate.Permissions;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.PermissionManagement;

namespace AbpTemplate.Controllers
{
    [Route("api/clients-management")]
    public class MyServiceController : AbpController
    {
        private readonly IPermissionManager _permissionManager;

        public MyServiceController(IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }
        [HttpPost]
        public async Task<string> GrantRolePermissionDemoAsync(string roleName, string role)
        {
            foreach (var permission in AbpTemplatePermissions.GetAll())
            {
                if (permission.Contains(role))
                {
                    await _permissionManager.SetForRoleAsync(roleName, permission, true);
                }
            }
            return "ok";
        }

        [HttpPost("permission")]
        public async Task GrantUserPermissionDemoAsync(
            Guid userId, string roleName, string permission)
        {
            await _permissionManager
                .SetForUserAsync(userId, permission, true);
        }
    }
}

