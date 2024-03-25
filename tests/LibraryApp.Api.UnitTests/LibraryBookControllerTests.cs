using System.Text;
using Common.Models.Api;
using FluentAssertions;
using LibraryApp.Api.Controllers;
using LibraryApp.Infrastructure.Helper;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace LibraryApp.Api.UnitTests;

public class LibraryBookControllerTests : BaseTests
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

        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        //Act
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

        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        //Act
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

}
