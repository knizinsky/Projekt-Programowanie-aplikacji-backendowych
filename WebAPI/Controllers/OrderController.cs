using ApplicationCore.Models;
using Infrastructure.EF.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Dto;
using WebAPI.Security;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing orders.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserEntity> _manager;

        public OrderController(ApplicationDbContext context, UserManager<UserEntity> manager)
        {
            _context = context;
            _manager = manager;
        }

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>A list of order DTOs.</returns>
        [HttpGet("all")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.ToListAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The order DTO.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = MapToDto(order);
            return Ok(orderDto);
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="orderDto">The order DTO containing the order data.</param>
        /// <returns>The created order DTO.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = new Order
            {
                Id = orderDto.Id,
                Name = orderDto.Name,
                Description = orderDto.Description,
                CustomerId = orderDto.CustomerId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="orderDto">The order DTO containing the updated order data.</param>
        /// <returns>No content if the update is successful, NotFound if the order is not found, or BadRequest if the ID in the URL does not match the ID in the DTO.</returns>

        [HttpPut("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> UpdateOrder(int id, OrderDto orderDto)
        {
            if (id != orderDto.Id)
            {
                return BadRequest();
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.Name = orderDto.Name;
            order.Description = orderDto.Description;
            order.CustomerId = orderDto.CustomerId;

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes an order.
        /// </summary>
        /// <param name="id">The ID of the order to delete.</param>
        /// <returns>
        /// Ok with the deleted order DTO if the deletion is successful,
        /// NotFound if the order is not found,
        /// Forbid if the current user is not authorized to delete,
        /// or BadRequest if the deletion fails.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }
            var authorizationHeader = HttpContext.Request.Headers["Authorization"];
            var token = authorizationHeader.ToString().Replace("Bearer ", "");

            var currentUserId = JwtTokenHelper.GetUserIdFromToken(token);
            bool isAdmin = await JwtTokenHelper.IsAdminUserAsync(currentUserId, _manager);
            if (!isAdmin)
            {
                return Forbid(); // User is not authorized to delete
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            var orderDto = MapToDto(order);
            return Ok(order);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Name = order.Name,
                Description = order.Description,
                CustomerId = order.CustomerId
            };
        }
    }
}