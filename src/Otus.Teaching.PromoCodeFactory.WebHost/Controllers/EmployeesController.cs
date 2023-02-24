using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Application.Common.Exceptions;
using Otus.Teaching.PromoCodeFactory.Core.Domain.Administration;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController
        : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _rolesRepository;

        public EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> rolesRepository)
        {
            _employeeRepository = employeeRepository;
            _rolesRepository = rolesRepository;
        }

        /// <summary>
        /// Получить список ролей по их идентификаторам
        /// </summary>
        /// <param name="roleIds">список идентификаторов</param>
        /// <returns>список ролей</returns>
        /// <exception cref="NotFoundException">не найдено</exception>
        private async Task<List<Role>> InitRoles(List<Guid> roleIds)
        {
            var roles = new List<Role>();
            foreach (var roleId in roleIds)
            {
                var role = await _rolesRepository.GetByIdAsync(roleId);
                if (role != null)
                    roles.Add(role);
                else
                    throw new NotFoundException(nameof(Role), roleId);
            }

            return roles;
        }

        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                }).ToList();

            return employeesModelList;
        }

        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }

        /// <summary>
        /// Добавить нового сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Guid>> Create(EmployeeRequest employeeRequest)
        {
            var roles = await InitRoles(employeeRequest.RoleIds);

            var id = await _employeeRepository.CreateAsync(new Employee
            {
                FirstName = employeeRequest.FirstName,
                LastName = employeeRequest.LastName,
                Email = employeeRequest.Email,
                Roles = roles
            });

            return Ok(id);
        }

        /// <summary>
        /// Отредактировать данные сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpPut("[action]/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, EmployeeRequest employeeRequest)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            var roles = await InitRoles(employeeRequest.RoleIds);

            await _employeeRepository.UpdateAsync(new Employee
            {
                Id = id,
                FirstName = employeeRequest.FirstName,
                LastName = employeeRequest.LastName,
                Email = employeeRequest.Email,
                Roles = roles,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            });

            return Ok();
        }

        /// <summary>
        /// Удалить сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpDelete("[action]/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _employeeRepository.DeleteAsync(id);

            return Ok();
        }
    }
}