﻿using AbpTemplate.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace AbpTemplate.Data;

public class AbpTemplateDbContext : AbpDbContext<AbpTemplateDbContext>, IDataProtectionKeyContext, ITenantManagementDbContext
{

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; } = null!;

    public DbSet<Sample> EmpDetails { get; set; }

    public AbpTemplateDbContext(DbContextOptions<AbpTemplateDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own entities here */
    }
}
