using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Dto;
using Infrastructure.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebAPI.Security;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing customers.
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserEntity> _manager;

        public CustomerController(ApplicationDbContext context, UserManager<UserEntity> manager)
        {
            _context = context;
            _manager = manager;
        }

        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <returns>A list of customers.</returns>
        [HttpGet]
        [Authorize(Policy = "Bearer")]
        [Route("all")]
        public async Task<IActionResult> GetCustomers()
        {
            return Ok(await _context.Customers.ToListAsync());
        }

        /// <summary>
        /// Retrieves a customer by its ID.
        /// </summary>
        /// <param name="id">The ID of the customer.</param>
        /// <returns>The customer.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                return Ok(customer);
            }
            return NotFound();
        }

        /// <summary>
        /// Adds a new customer.
        /// </summary>
        /// <param name="customerDto">The customer DTO containing the customer data.</param>
        /// <returns>The created customer.</returns>
        [HttpPost]
        [Authorize(Policy = "Bearer")]
        [Route("add")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = new Customer
            {
                Id = customerDto.Id,
                Name = customerDto.Name,
                Description = customerDto.Description
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="customerDto">The customer DTO containing the updated customer data.</param>
        /// <returns>
        /// The updated customer if the update is successful,
        /// NotFound if the customer is not found,
        /// or BadRequest if the update fails.
        /// </returns>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                customer.Name = customerDto.Name;
                customer.Description = customerDto.Description;
                await _context.SaveChangesAsync();
                return Ok(customer);
            }
            return NotFound();
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The ID of the customer to delete.</param>
        /// <returns>
        /// Ok with a success message if the deletion is successful,
        /// NotFound if the customer is not found,
        /// Forbid if the current user is not authorized to delete,
        /// or BadRequest if the deletion fails.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorizationHeader = HttpContext.Request.Headers["Authorization"];
            var token = authorizationHeader.ToString().Replace("Bearer ", "");

            var currentUserId = JwtTokenHelper.GetUserIdFromToken(token);
            bool isAdmin = await JwtTokenHelper.IsAdminUserAsync(currentUserId, _manager);
            if (!isAdmin)
            {
                return Forbid(); // User is not authorized to delete
            }
            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Ok("Customer has been removed from database");
            }
            return NotFound();
        }
    }
}
