using AbpTemplate.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AbpTemplate.Permissions;

public class AbpTemplatePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        //var abpTemplateGroup = context.AddGroup(AbpTemplatePermissions.GroupName, L("Permission:AbpTemplate"));
        //var tenantPermission = abpTemplateGroup.AddPermission(AbpTemplatePermissions.Tenant.Default, L("Permission:AbpTemplate.Tenant"));
        //tenantPermission.AddChild(AbpTemplatePermissions.Tenant.AddHost, L("Permission:AbpTemplate.Tenant.AddHost"));

        PermissionGroupDefinition AbpTemplate = context.AddGroup(AbpTemplatePermissions.GroupName, L("Permission:AbpTemplate"));

        PermissionDefinition adminPanelPermission = AbpTemplate.AddPermission(AbpTemplatePermissions.Sample.Default, L("Permission:AbpTemplate"));

        _ = adminPanelPermission.AddChild(AbpTemplatePermissions.Sample.Get, L("Permission:Sample.Get"));

        _ = adminPanelPermission.AddChild(AbpTemplatePermissions.Sample.GetAll, L("Permission:Sample.GetAll"));

        _ = adminPanelPermission.AddChild(AbpTemplatePermissions.Sample.Create, L("Permission:Sample.Create"));

        _ = adminPanelPermission.AddChild(AbpTemplatePermissions.Sample.Edit, L("Permission:Sample.Update"));

        _ = adminPanelPermission.AddChild(AbpTemplatePermissions.Sample.Delete, L("Permission:Sample.Delete"));

  
 
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpTemplateResource>(name);
    }
}