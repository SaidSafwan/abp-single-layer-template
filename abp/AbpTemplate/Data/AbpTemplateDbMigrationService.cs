﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using AbpTemplate.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace AbpTemplate.Data;

public class AbpTemplateDbMigrationService : ITransientDependency
{
    public ILogger<AbpTemplateDbMigrationService> Logger { get; set; }

    private readonly IDataSeeder _dataSeeder;
    private readonly AbpTemplateEFCoreDbSchemaMigrator _dbSchemaMigrator;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionManager _permissionManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IdentityRoleManager _roleManager;

    public AbpTemplateDbMigrationService(
        IDataSeeder dataSeeder,
        AbpTemplateEFCoreDbSchemaMigrator dbSchemaMigrator,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IPermissionManager permissionManager,
        IUnitOfWorkManager unitOfWorkManager,
        IIdentityRoleRepository roleRepository,
        ILookupNormalizer lookupNormalizer,
        IdentityRoleManager roleManager)
    {
        _dataSeeder = dataSeeder;
        _dbSchemaMigrator = dbSchemaMigrator;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _permissionManager = permissionManager;
        _unitOfWorkManager = unitOfWorkManager;

        Logger = NullLogger<AbpTemplateDbMigrationService>.Instance;
        _roleRepository = roleRepository;
        _lookupNormalizer = lookupNormalizer;
        _roleManager = roleManager;
    }

    public async Task MigrateAsync()
    {
        var initialMigrationAdded = AddInitialMigrationIfNotExist();

        if (initialMigrationAdded)
        {
            return;
        }

        Logger.LogInformation("Started database migrations...");

        await MigrateDatabaseSchemaAsync();
        await SeedDataAsync();
        await AgentUserRoleSeedDataAsync();

        Logger.LogInformation($"Successfully completed host database migrations.");

        var tenants = await _tenantRepository.GetListAsync(includeDetails: true);

        var migratedDatabaseSchemas = new HashSet<string>();
        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                if (tenant.ConnectionStrings.Any())
                {
                    var tenantConnectionStrings = tenant.ConnectionStrings
                        .Select(x => x.Value)
                        .ToList();

                    if (!migratedDatabaseSchemas.IsSupersetOf(tenantConnectionStrings))
                    {
                        await MigrateDatabaseSchemaAsync(tenant);

                        migratedDatabaseSchemas.AddIfNotContains(tenantConnectionStrings);
                    }
                }

                await SeedDataAsync(tenant);
            }

            Logger.LogInformation($"Successfully completed {tenant.Name} tenant database migrations.");
        }

        Logger.LogInformation("Successfully completed all database migrations.");
        Logger.LogInformation("You can safely end this process...");
    }

    private async Task MigrateDatabaseSchemaAsync(Tenant tenant = null)
    {
        Logger.LogInformation($"Migrating schema for {(tenant == null ? "host" : tenant.Name + " tenant")} database...");
        await _dbSchemaMigrator.MigrateAsync();
    }

    private async Task SeedDataAsync(Tenant tenant = null)
    {
        Logger.LogInformation($"Executing {(tenant == null ? "host" : tenant.Name + " tenant")} database seed...");

        await _dataSeeder.SeedAsync(new DataSeedContext(tenant?.Id)
            .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName, IdentityDataSeedContributor.AdminEmailDefaultValue)
            .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName, IdentityDataSeedContributor.AdminPasswordDefaultValue)
        );
    }

    private bool AddInitialMigrationIfNotExist()
    {
        try
        {
            if (!DbMigrationsProjectExists())
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            if (!MigrationsFolderExists())
            {
                AddInitialMigration();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning("Couldn't determinate if any migrations exist : " + e.Message);
            return false;
        }
    }

    private bool DbMigrationsProjectExists()
    {
        return Directory.Exists(GetEntityFrameworkCoreProjectFolderPath());
    }

    private bool MigrationsFolderExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return Directory.Exists(Path.Combine(dbMigrationsProjectFolder, "Migrations"));
    }

    private void AddInitialMigration()
    {
        Logger.LogInformation("Creating initial migration...");

        string argumentPrefix;
        string fileName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            argumentPrefix = "-c";
            fileName = "/bin/bash";
        }
        else
        {
            argumentPrefix = "/C";
            fileName = "cmd.exe";
        }

        var procStartInfo = new ProcessStartInfo(fileName,
            $"{argumentPrefix} \"abp create-migration-and-run-migrator \"{GetEntityFrameworkCoreProjectFolderPath()}\" --nolayers\""
        );

        try
        {
            Process.Start(procStartInfo);
        }
        catch (Exception)
        {
            throw new Exception("Couldn't run ABP CLI...");
        }
    }

    private string GetEntityFrameworkCoreProjectFolderPath()
    {
        var slnDirectoryPath = GetSolutionDirectoryPath();

        if (slnDirectoryPath == null)
        {
            throw new Exception("Solution folder not found!");
        }

        return Path.Combine(slnDirectoryPath, "AbpTemplate");
    }

    private string GetSolutionDirectoryPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (Directory.GetParent(currentDirectory.FullName) != null)
        {
            currentDirectory = Directory.GetParent(currentDirectory.FullName);

            if (Directory.GetFiles(currentDirectory.FullName).FirstOrDefault(f => f.EndsWith(".sln")) != null)
            {
                return currentDirectory.FullName;
            }
        }

        return null;
    }

    private async Task AgentUserRoleSeedDataAsync()

    {

        using var uow = _unitOfWorkManager.Begin(true);
        const string roleName = "User";

        var existingAgentRole = await _roleRepository.FindByNormalizedNameAsync(_lookupNormalizer.NormalizeName(roleName));

        if (existingAgentRole == null)

        {

            var agentRole = new Volo.Abp.Identity.IdentityRole(

                Guid.NewGuid(),

                roleName)

            {

                IsStatic = false,

                IsPublic = true
            };

            await _roleManager.CreateAsync(agentRole);

            Logger.LogInformation("Seeding permission to agent role...");

            await AssignPermissions(roleName, ".User");

        }
    }


    private async Task AssignPermissions(string roleName, string role)
    {
        foreach (var permission in AbpTemplatePermissions.GetAll())
        {
            if (permission.Contains(role))
            {
                await _permissionManager.SetForRoleAsync(roleName, permission, true);
            }
        }
    }
}
