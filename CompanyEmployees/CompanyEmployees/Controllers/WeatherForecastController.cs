using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private ILoggerManager _logger;
        private IRepositoryManager _repositoryManager;
        public WeatherForecastController(ILoggerManager logger, IRepositoryManager repositoryManager)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
        }
        //[HttpGet]
        //public IEnumerable<string> GetLog()
        //{
        //    _logger.LogInfo("Here is info message from our values controller.");
        //    _logger.LogDebug("Here is debug message from our values controller.");
        //    _logger.LogWarn("Here is Warn message from our values controller.");
        //    _logger.LogError("Here is Error message from our values controller.");

        //    return new string[] { "value 1", "value 2" };
        //}
        [HttpGet]
        public IEnumerable<string> Get()
        {
            //_repositoryManager.Company.AnyMethodFromCompanyRepository();
            //_repository.Employee.AnyMethodFromEmployeeRepository();
            return new string[] { "value1", "value2" };
        }
    }
}
