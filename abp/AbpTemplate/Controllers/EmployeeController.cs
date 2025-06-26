using AbpTemplate.Entities;
using AbpTemplate.Permissions;
using AbpTemplate.Services;
using AbpTemplate.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace AbpTemplate.Controllers
{

    [Route("api/emp-management")]
    public class EmployeeController : AbpController
    {
        private readonly IRepository<Sample, Guid> SampleRepository;
        private readonly IDistributedCache<EmployeCachItemDto, Guid> _cache;
        private readonly RegistrationService _registrationService;

        public EmployeeController(RegistrationService registrationService,
            IRepository<Sample, Guid> _SampleRepository, IDistributedCache<EmployeCachItemDto, Guid> cache)
        {
            _registrationService = registrationService;
            this.SampleRepository = _SampleRepository;
            this._cache = cache; //added
        }


        [HttpPost("SubmitData")]
        [Authorize(AbpTemplatePermissions.Sample.Edit)]
        public async Task<ActionResult<Sample>> CreateAsync([FromBody] Samplepostdto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for existing employee with the same email or phone number
            var existingEmployee = await SampleRepository.FirstOrDefaultAsync(e =>
                e.Email.ToLower() == input.Email.ToLower() || e.PhoneNum == input.PhoneNum);

            if (existingEmployee != null)
            {
                return BadRequest("An employee with the same email or phone number already exists.");
            }

            // Map the input DTO to the Sample entity
            var newEmployee = ObjectMapper.Map<Samplepostdto, Sample>(input);

            // Save the new employee to the database
            await SampleRepository.InsertAsync(newEmployee);

            // Add the new employee data to the cache without setting an expiration time
            var employeeCacheItem = new EmployeCachItemDto
            {
                Department = newEmployee.Department,
                EmpName = newEmployee.EmpName,
                PhoneNum = newEmployee.PhoneNum,
                Role = newEmployee.Role,
                userId = newEmployee.Id,
                Email = newEmployee.Email
            };

            await _cache.SetAsync(newEmployee.Id, employeeCacheItem); // No expiration time set

            // (Optional) Invalidate any related cached lists if necessary
            // await _cache.RemoveAsync("EmployeeList");

            return Ok("Employee added successfully.");
        }


        [HttpGet("GetById")]
        public async Task<ActionResult<EmployeCachItemDto>> GetAsync(Guid id)
        {
            // Attempt to retrieve data from Redis cache
            var cachedEmployee = await _cache.GetAsync(id);
            if (cachedEmployee != null)
            {
                await _registrationService.RegisterAsync("userName", "emailAddress", "password");
                // Return the cached data if available
                return Ok(cachedEmployee);
            }

            // Fetch data from the database if not found in cache
            var employee = await SampleRepository.FindAsync(id);
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }

            // Map database entity to cache DTO
            var employeeCacheItem = new EmployeCachItemDto
            {
                Department = employee.Department,
                EmpName = employee.EmpName,
                PhoneNum = employee.PhoneNum,
                Role = employee.Role,
                userId = employee.Id,
                Email = employee.Email
            };

            // Store the fetched data in Redis cache for future requests
            await _cache.SetAsync(id, employeeCacheItem, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1) // Cache expiry
            });

            // Return the freshly fetched data
            return Ok(employeeCacheItem);
        }


        // Update data controller
        [HttpPut("Update")]
        [Authorize(AbpTemplatePermissions.Sample.Edit)]
        public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] Samplepostdto input)
        {
            var existingData = await SampleRepository.GetAsync(id);

            if (existingData == null)
            {
                return NotFound($"Data with ID {id} not found.");
            }

            // Check for duplicate email or phone number
            var duplicateEmployee = await SampleRepository.FirstOrDefaultAsync(e =>
                (e.Email == input.Email || e.PhoneNum == input.PhoneNum) && e.Id != id);

            if (duplicateEmployee != null)
            {
                return BadRequest("Another employee with the same email or phone number already exists.");
            }

            existingData.Department = input.Department;
            existingData.EmpName = input.EmpName;
            existingData.PhoneNum = input.PhoneNum;
            existingData.Role = input.Role;
            existingData.Email = input.Email;

            await SampleRepository.UpdateAsync(existingData);

            return Ok("Data updated successfully.");
        }

        //Delete Data controller

        [HttpDelete("Delete")]

        [Authorize(AbpTemplatePermissions.Sample.Create)]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            var existingData = await SampleRepository.FindAsync(id);

            if (existingData == null)
            {
                return NotFound($"Data with ID {id} not found.");
            }

            await SampleRepository.DeleteAsync(existingData);

            return Ok("Data Deleted Successfully.");

        }

        // Get all data controller

        [HttpGet("GetAll")]
        [Authorize(Roles = "User, admin")]
        public async Task<ActionResult<List<Sampledto>>> GetAllAsync()
        {
            var dataList = await SampleRepository.GetListAsync();

            if (dataList == null)
            {
                return NotFound($"Data is not found.");
            }

            var result = dataList.Select(client => new Sampledto
            {
                Department = client.Department,
                EmpName = client.EmpName,
                PhoneNum = client.PhoneNum,
                Role = client.Role,
                userId = client.Id,
                Email = client.Email
            }).ToList();

            return Ok(result);
        }

    }
}
