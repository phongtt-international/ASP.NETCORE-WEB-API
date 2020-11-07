using Entities.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetEmployees(Guid companyId,bool trackChanges);
    }
}
