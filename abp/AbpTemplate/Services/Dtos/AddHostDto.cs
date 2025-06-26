using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace AbpTemplate.Services.Dtos;

public class AddHostDto
{
    public Guid Id { get; set; }
    public string Host { get; set; }
}

public class Sampledto
{
    public Guid userId { get; set; }
    public string EmpName { get; set; }
    public string Role { get; set; }
    public string PhoneNum { get; set; }
    public string Department { get; set; }
    public string Email { get; set; }
}

public class Samplepostdto: Entity<Guid>
{
    public string EmpName { get; set; }
    public string Role { get; set; }
    public string PhoneNum { get; set; }
    public string Department { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
