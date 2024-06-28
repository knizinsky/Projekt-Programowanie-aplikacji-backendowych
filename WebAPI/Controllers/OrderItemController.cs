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
    /// Controller for managing order items.
    /// </summary>
    [ApiController]
    [Route("api/orderitems")]
    public class OrderItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserEntity> _manager;

        public OrderItemController(ApplicationDbContext context, UserManager<UserEntity> manager)
        {
            _context = context;
            _manager = manager;
        }

        /// <summary>
        /// Retrieves all order items.
        /// </summary>
        /// <returns>A list of order items.</returns>
        [HttpGet("all")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> GetOrderItems()
        {
            var orderItems = await _context.OrderItems.ToListAsync();
            var orderItemDtos = orderItems.Select(MapToDto).ToList();
            return Ok(orderItemDtos);
        }

        /// <summary>
        /// Retrieves an order item by its ID.
        /// </summary>
        /// <param name="id">The ID of the order item.</param>
        /// <returns>The order item.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> GetOrderItem(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null)
            {
                return NotFound();
            }

            var orderItemDto = MapToDto(orderItem);
            return Ok(orderItem);
        }

        /// <summary>
        /// Creates a new order item.
        /// </summary>
        /// <param name="orderItemDto">The order item DTO containing the order item data.</param>
        /// <returns>The created order item.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> CreateOrderItem(OrderItemDto orderItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderItem = new OrderItem
            {
                Id = orderItemDto.Id,
                Name = orderItemDto.Name,
                Description = orderItemDto.Description,
                Stock = orderItemDto.Stock,
                OrderId = orderItemDto.OrderId
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            return Ok(orderItem);
        }

        /// <summary>
        /// Updates an existing order item.
        /// </summary>
        /// <param name="id">The ID of the order item to update.</param>
        /// <param name="orderItemDto">The order item DTO containing the updated order item data.</param>
        /// <returns>
        /// NoContent if the update is successful,
        /// NotFound if the order item is not found,
        /// BadRequest if the request is invalid,
        /// or NotFound if the order item is not found after the update.
        /// </returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> UpdateOrderItem(int id, OrderItemDto orderItemDto)
        {
            if (id != orderItemDto.Id)
            {
                return BadRequest();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null)
            {
                return NotFound();
            }

            orderItem.Name = orderItemDto.Name;
            orderItem.Description = orderItemDto.Description;
            orderItem.Stock = orderItemDto.Stock;
            orderItem.OrderId = orderItemDto.OrderId;

            _context.Entry(orderItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(id))
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
        /// Deletes an order item.
        /// </summary>
        /// <param name="id">The ID of the order item to delete.</param>
        /// <returns>
        /// Ok with the deleted order item if the deletion is successful,
        /// NotFound if the order item is not found,
        /// or Forbid if the user is not authorized to delete the order item.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Bearer")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null)
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

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            var orderItemDto = MapToDto(orderItem);
            return Ok(orderItem);
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }

        private OrderItemDto MapToDto(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                Name = orderItem.Name,
                Description = orderItem.Description,
                Stock = orderItem.Stock,
                OrderId = orderItem.OrderId
            };
        }
    }
}
