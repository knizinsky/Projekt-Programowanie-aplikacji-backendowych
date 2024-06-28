using ApplicationCore.Models;
using Infrastructure.EF.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Configuration;
using WebAPI.Controllers;
using WebAPI.Dto;
using WebAPI.Security;


namespace WebAPI.Tests
{
    [TestClass]
    public class AuthenticationControllerTests
    {
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private Mock<ILogger<AuthenticationController>> _loggerMock;
        private JwtSettings _jwtSettings;
        private AuthenticationController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            var store = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
            _loggerMock = new Mock<ILogger<AuthenticationController>>();

            // Create a configuration
            var myConfiguration = new Dictionary<string, string>
            {
                {"JwtSettings:Secret", "test_secret"},
                {"JwtSettings:Issuer", "test_issuer"},
                {"JwtSettings:Audience", "test_audience"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            _jwtSettings = new JwtSettings(configuration);
            _controller = new AuthenticationController(_userManagerMock.Object, _loggerMock.Object, configuration, _jwtSettings);
        }


        [TestMethod]
        public async Task Authenticate_InvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            var userDto = new LoginUserDto { LoginName = "testuser", Password = "wrongpassword" };
            var userEntity = new UserEntity { UserName = "testuser", Email = "test@example.com", Id = 1 };

            _userManagerMock.Setup(m => m.FindByNameAsync(userDto.LoginName)).ReturnsAsync(userEntity);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(userEntity, userDto.Password)).ReturnsAsync(false);

            // Act
            var result = await _controller.Authenticate(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Register_ValidUser_ReturnsOk()
        {
            // Arrange
            var userDto = new RegisterUserDto { Username = "newuser", Email = "newuser@example.com", Password = "password" };
            var identityResult = IdentityResult.Success;

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<UserEntity>(), userDto.Password)).ReturnsAsync(identityResult);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<UserEntity>(), "USER")).ReturnsAsync(identityResult);

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task Register_InvalidUser_ReturnsBadRequest()
        {
            // Arrange
            var userDto = new RegisterUserDto { Username = "newuser", Email = "newuser@example.com", Password = "password" };
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error" });

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<UserEntity>(), userDto.Password)).ReturnsAsync(identityResult);

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "1";
            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((UserEntity)null);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }

    [TestClass]
    public class CustomerControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private ApplicationDbContext _context;
        private CustomerController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(_dbContextOptions);

            var store = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new CustomerController(_context, _userManagerMock.Object);
        }

        [TestMethod]
        public async Task AddCustomer_ValidCustomer_ReturnsOk()
        {
            // Arrange
            var customerDto = new CustomerDto { Name = "Customer1", Description = "Description1" };

            // Act
            var result = await _controller.AddCustomer(customerDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var returnedCustomer = result.Value as Customer;
            Assert.AreEqual(customerDto.Name, returnedCustomer.Name);
        }

        [TestMethod]
        public async Task UpdateCustomer_CustomerExists_ReturnsOk()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "Customer1", Description = "Description1" };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            var updatedCustomerDto = new CustomerDto { Name = "UpdatedName", Description = "UpdatedDescription" };

            // Act
            var result = await _controller.UpdateCustomer(1, updatedCustomerDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var updatedCustomer = result.Value as Customer;
            Assert.AreEqual(updatedCustomerDto.Name, updatedCustomer.Name);
        }
    }




    [TestClass]
    public class OrderControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private ApplicationDbContext _context;
        private OrderController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(_dbContextOptions);

            var store = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new OrderController(_context, _userManagerMock.Object);

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer dummy_token";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task GetOrders_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, Name = "Order1", Description = "Description1", CustomerId = 1 },
                new Order { Id = 2, Name = "Order2", Description = "Description2", CustomerId = 2 }
            };
            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrders() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var returnOrders = result.Value as List<Order>;
            Assert.AreEqual(2, returnOrders.Count);
        }

        [TestMethod]
        public async Task GetOrder_ValidId_ReturnsOrderDto()
        {
            // Arrange
            var order = new Order { Id = 1, Name = "Order1", Description = "Description1", CustomerId = 1 };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrder(1) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var orderDto = result.Value as OrderDto;
            Assert.AreEqual(order.Name, orderDto.Name);
            Assert.AreEqual(order.Description, orderDto.Description);
            Assert.AreEqual(order.CustomerId, orderDto.CustomerId);
        }

        [TestMethod]
        public async Task GetOrder_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetOrder(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CreateOrder_ValidOrderDto_ReturnsCreatedOrderDto()
        {
            // Arrange
            var orderDto = new OrderDto { Name = "Order1", Description = "Description1", CustomerId = 1 };

            // Act
            var result = await _controller.CreateOrder(orderDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var createdOrder = result.Value as Order;
            Assert.AreEqual(1, createdOrder.Id);
            Assert.AreEqual(orderDto.Name, createdOrder.Name);
            Assert.AreEqual(orderDto.Description, createdOrder.Description);
            Assert.AreEqual(orderDto.CustomerId, createdOrder.CustomerId);
        }


        [TestMethod]
        public async Task UpdateOrder_InvalidIdInDto_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order { Id = 1, Name = "Order1", Description = "Description1", CustomerId = 1 };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var updatedOrderDto = new OrderDto  { Name = "UpdatedOrder1", Description = "UpdatedDescription1", CustomerId = 2 };

            // Act
            var result = await _controller.UpdateOrder(1, updatedOrderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task UpdateOrder_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var updatedOrderDto = new OrderDto { Name = "UpdatedOrder1", Description = "UpdatedDescription1", CustomerId = 2 };

            // Act
            var result = await _controller.UpdateOrder(1, updatedOrderDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task DeleteOrder_OrderNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteOrder(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }


    [TestClass]
    public class OrderItemControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private ApplicationDbContext _context;
        private OrderItemController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(_dbContextOptions);

            var store = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new OrderItemController(_context, _userManagerMock.Object);

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer dummy_token";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task GetOrderItems_ReturnsAllOrderItems()
        {
            // Arrange
            var orderItems = new List<OrderItem>
            {
                new OrderItem { Id = 1, Name = "Item1", Description = "Description1", Stock = Stock.High, OrderId = 1 },
                new OrderItem { Id = 2, Name = "Item2", Description = "Description2", Stock = Stock.High, OrderId = 2 }
            };
            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrderItems() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var returnOrderItems = result.Value as List<OrderItemDto>;
            Assert.AreEqual(2, returnOrderItems.Count);
        }

        [TestMethod]
        public async Task GetOrderItem_ValidId_ReturnsOrderItemDto()
        {
            // Arrange
            var orderItem = new OrderItem { Id = 1, Name = "Item1", Description = "Description1", Stock = Stock.High, OrderId = 1 };
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrderItem(1) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var orderItemDto = result.Value as OrderItemDto;
            Assert.AreEqual(orderItem.Name, orderItemDto.Name);
            Assert.AreEqual(orderItem.Description, orderItemDto.Description);
            Assert.AreEqual(orderItem.Stock, orderItemDto.Stock);
            Assert.AreEqual(orderItem.OrderId, orderItemDto.OrderId);
        }

        [TestMethod]
        public async Task GetOrderItem_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetOrderItem(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task CreateOrderItem_ValidOrderItemDto_ReturnsCreatedOrderItemDto()
        {
            // Arrange
            var orderItemDto = new OrderItemDto { Name = "Item1", Description = "Description1", Stock = Stock.High, OrderId = 1 };

            // Act
            var result = await _controller.CreateOrderItem(orderItemDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var createdOrderItem = result.Value as OrderItem;
            Assert.AreEqual(1, createdOrderItem.Id);
            Assert.AreEqual(orderItemDto.Name, createdOrderItem.Name);
            Assert.AreEqual(orderItemDto.Description, createdOrderItem.Description);
            Assert.AreEqual(orderItemDto.Stock, createdOrderItem.Stock);
            Assert.AreEqual(orderItemDto.OrderId, createdOrderItem.OrderId);
        }

        [TestMethod]
        public async Task UpdateOrderItem_InvalidIdInDto_ReturnsBadRequest()
        {
            // Arrange
            var orderItem = new OrderItem { Id = 1, Name = "Item1", Description = "Description1", Stock = Stock.High, OrderId = 1 };
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();

            var updatedOrderItemDto = new OrderItemDto { Name = "UpdatedItem1", Description = "UpdatedDescription1", Stock = Stock.High, OrderId = 2 };

            // Act
            var result = await _controller.UpdateOrderItem(1, updatedOrderItemDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task UpdateOrderItem_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var updatedOrderItemDto = new OrderItemDto { Name = "UpdatedItem1", Description = "UpdatedDescription1", Stock = Stock.High, OrderId = 2 };

            // Act
            var result = await _controller.UpdateOrderItem(1, updatedOrderItemDto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteOrderItem_OrderNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteOrderItem(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

    }
}
