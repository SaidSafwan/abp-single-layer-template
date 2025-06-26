using Volo.Abp.Reflection;

namespace AbpTemplate.Permissions;

public static class AbpTemplatePermissions
{
    public const string GroupName = "AbpTemplate";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
    public static class Tenant
    {
        public const string Default = GroupName + ".Tenant";
        public const string AddHost = Default + ".AddHost";
    }

    public static class Sample
    {
        public const string Default = GroupName + ".Sample";
        public const string Create = Default + ".User.Create";
        public const string Edit = Default + ".User.Manager.Edit";
        public const string Delete = Default + ".User.Delete";
        public const string Get = Default + ".User.Get";
        public const string GetAll = Default + ".User.Getall";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(
            typeof(AbpTemplatePermissions));
    }
}