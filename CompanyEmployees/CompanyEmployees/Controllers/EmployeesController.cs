using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities;
using Entities.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogError($"The Company with id: {companyId} doesn't exits in the Database.");
                return NotFound();
            }
            var employees = _repositoryManager.Employee.GetEmployees(companyId, trackChanges: false);
            var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return Ok(employeeDto);
        }
    }
}