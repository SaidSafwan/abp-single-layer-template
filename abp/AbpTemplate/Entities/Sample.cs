using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace AbpTemplate.Entities
{
    public class Sample : Entity<Guid>
    {
        public string EmpName { get; set; }
        public string Role { get; set; }
        public string PhoneNum { get; set; }
        public string Department { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
