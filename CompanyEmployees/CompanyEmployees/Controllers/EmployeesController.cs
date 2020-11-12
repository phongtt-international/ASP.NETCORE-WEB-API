using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities;
using Entities.DTO;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        public EmployeesController(IRepositoryManager repositoryManager, 
                                    IMapper mapper,
                                    ILoggerManager logger)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be less than min age.");
            }
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogError($"The Company with id: {companyId} doesn't exits in the Database.");
                return NotFound();
            }
            var employees = await _repositoryManager.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employees.MetaData));

            var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return Ok(employeeDto);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges:false);
            if (company == null)
            {
                _logger.LogError($"The Company with id: {companyId} doesn't exits in the Database.");
                return NotFound();
            }
            var employee = await _repositoryManager.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
            if (employee == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if(employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object");
                return UnprocessableEntity(ModelState);
            }

            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee);
            _repositoryManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repositoryManager.SaveAsync();
            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            //if(company == null)
            //{
            //    _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            //    return NotFound();
            //}
            //var employeeForCompany = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: false);
            //if(employeeForCompany == null)
            //{
            //    _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
            _repositoryManager.Employee.DeleteEmployee(employeeForCompany);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id,[FromBody]EmployeeForUpdateDto employee)
        {
            //if (employee == null)
            //{
            //    _logger.LogError("EmployeeForUpdateDto object sent from client is null.");
            //    return BadRequest("EmployeeForUpdateDto object is null");
            //}
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
            //    return UnprocessableEntity(ModelState);
            //}
            //var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges:false);
            //if(company == null)
            //{
            //    _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            //    return NotFound();
            //}
            //var employeeEntity = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: true);
            //if(employeeEntity == null)
            //{
            //    _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeEntity);
            await _repositoryManager.SaveAsync();
            return NoContent();

        }
        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id,
           [FromBody]JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }
            //var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            //if (company == null)
            //{
            //    _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            //    return NotFound();
            //}
            //var employeeEntity =  _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: true);
            //if (employeeEntity == null)
            //{
            //    _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            //validating the patchDoc which is completely valid at this moment
            patchDoc.ApplyTo(employeeToPatch, ModelState);
            // trigger a validation when save if ModelState invalid then show err
            TryValidateModel(employeeToPatch);
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }
            _mapper.Map(employeeToPatch, employeeEntity);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }


    }
}