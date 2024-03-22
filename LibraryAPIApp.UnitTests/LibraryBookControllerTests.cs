using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Common.Models.Api;
using DataAccess;
using DataAccess.WebApiManager.Interfaces;
using FluentAssertions;
using LibraryAPIApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json;

namespace LibraryAPIApp.UnitTests;

public class LibraryBookControllerTests
{
    [Fact]
    public void Get_Returns_OkResult_When_Book_Exists()
    {
        // Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var controller = new LibraryBookController(mockManager.Object);
        var fakeBookId = "fakeId";
        var fakeBook = new LibraryBookApiModel { /* Initialize fake book object */ };
        mockManager.Setup(m => m.GetLibraryBookByLibraryBookCode(fakeBookId)).Returns(fakeBook);

        // Act
        var result = controller.Get(fakeBookId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(fakeBook);
    }

    [Fact]
    public void Insert_Returns_OkResult_When_Successfully_Inserted()
    {
        // Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var controller = new LibraryBookController(mockManager.Object);
        var model = new LibraryBookApiModel
        {
            // Initialize your model object here
        };
        var fakeLibraryBookCode = "fakeCode";
        mockManager.Setup(m => m.InsertLibraryBook(It.IsAny<LibraryBookApiModel>(), out fakeLibraryBookCode, It.IsAny<TransactionParam>())).Returns(1);

        // Creating Bearer Token
        var token = CreateBearerToken("role");

        // Set up HttpClient with Bearer token
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "your_api_endpoint_here");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        // Set Authorization header in controller context
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + token;

        var result = controller.Insert(model);

        // Assert
        var okResult = result.Should().BeOfType<ContentResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Content.Should().Be(fakeLibraryBookCode);
    }

    [Fact]
    public void Insert_Returns_BadRequestResult_When_ModelState_Invalid()
    {
        // Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var controller = new LibraryBookController(mockManager.Object);
        controller.ModelState.AddModelError("TestError", "TestErrorMessage");
        var model = new LibraryBookApiModel(); // Create an invalid model

        // Act
        var result = controller.Insert(model);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Update_Returns_OkResult_When_Successfully_Inserted()
    {
        // Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var controller = new LibraryBookController(mockManager.Object);
        var model = new LibraryBookApiModel
        {
            // Initialize your model object here
            Title = "Title",
            Author = "Author"
        };
        
        mockManager.Setup(m => m.UpdateLibraryBook(It.IsAny<LibraryBookApiModel>(), It.IsAny<TransactionParam>())).Returns(1);

        // Creating Bearer Token
        var token = CreateBearerToken("role");

        // Set up HttpClient with Bearer token
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, "your_api_endpoint_here");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        // Set Authorization header in controller context
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + token;

        OkResult result = (OkResult)controller.Update(model);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Update_Returns_BadRequestResult_When_ModelState_Invalid()
    {
        // Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var controller = new LibraryBookController(mockManager.Object);
        controller.ModelState.AddModelError("TestError", "TestErrorMessage");
        var model = new LibraryBookApiModel(); // Create an invalid model

        // Act
        var result = controller.Update(model);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    public static string CreateBearerToken(string role)
    {
        var BearerTokenSigningKey = "StubPrivateKey123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("role", role) };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(BearerTokenSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: user.Claims,
            signingCredentials: creds,
            expires: DateTime.UtcNow.AddMinutes(5)
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}