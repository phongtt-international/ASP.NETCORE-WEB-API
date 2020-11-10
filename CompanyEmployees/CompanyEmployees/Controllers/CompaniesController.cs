using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DTO;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog.Fluent;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompaniesController(IRepositoryManager repositoryManager,
                                    ILoggerManager logger,
                                    IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _repositoryManager.Company.GetAllCompaniesAsync(trackChanges: false);
                var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
                return Ok(companiesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(GetCompanies)} action { ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(id, trackChanges: false);
            if(company == null)
            {
                return NotFound();
            }
            var companyDto = _mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if(company == null)
            {
                _logger.LogError("CompanyForCreationDto object sent from client is null.");
                return BadRequest("CompanyForCreationDto object is null");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the CompanyForCreationDto object");
                return UnprocessableEntity(ModelState);
            }
            var companyEntity = _mapper.Map<Company>(company);
            _repositoryManager.Company.CreateCompany(companyEntity);
            await _repositoryManager.SaveAsync();
            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companiesEntity = await _repositoryManager.Company.GetByIdsAsync(ids, trackChanges: false);
            if (ids.Count() != companiesEntity.Count())
            {
                _logger.LogError("Some ids are not valid in collection");
                return NotFound();
            }
            var companiesReturn = _mapper.Map<IEnumerable<CompanyDto>>(companiesEntity);
            return Ok(companiesReturn);
        }
        [HttpPost("Collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(company);
            }
            await _repositoryManager.SaveAsync();
            var companyCollectionReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionReturn.Select(c => c.Id));
            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            //var company = await _repositoryManager.Company.GetCompanyAsync(id, trackChanges: false);
            //if (company == null)
            //{
            //    _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}
            var company = HttpContext.Items["company"] as Company;
            _repositoryManager.Company.DeleteCompany(company);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody]CompanyForUpdateDto company)
        {
            var companyEntity = HttpContext.Items["company"] as Company;
            _mapper.Map(company, companyEntity);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
    }
}