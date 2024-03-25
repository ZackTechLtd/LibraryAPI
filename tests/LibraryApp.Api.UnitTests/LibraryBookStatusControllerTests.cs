using System.Text;
using Common.Models;
using Common.Models.Api;
using FluentAssertions;
using LibraryApp.Api.Controllers;
using LibraryApp.Infrastructure.Helper;
using LibraryApp.Infrastructure.WebApiManager.Interfaces;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace LibraryApp.Api.UnitTests;

public class LibraryBookStatusControllerTests : BaseTests
{
	
    [Fact]
    public void Get_ReturnsStatus_OkResult_When_Book_Exists()
	{
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBookStatusCode = "Current Status"
        };

        mockStatusManager.Setup(m => m.GetLibraryBookStatusByLibraryBookStatusCode(It.IsAny<string>())).Returns(model);

        //Act
        var result = controller.Get("1");

        //Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(model);
    }

    [Fact]
    public void Insert_Returns_OkResult_When_Successfully_Inserted()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };

        
        mockStatusManager.Setup(m => m.GetCountOfBookCurrentLent(It.IsAny<string>())).Returns(0);
        mockManager.Setup(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>())).Returns(libraryBook);
        mockStatusManager.Setup(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        mockStatusManager.Setup(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>())).Returns(1);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");


        //Act
        var result = controller.Insert(model);

        //Assert
        var okResult = result.Should().BeOfType<ContentResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Content.Should().Be(libraryBookStatus);

    }

    [Fact]
    public void Insert_Returns_BadRequest_When_BookIsLent_ExceedMax_Inserted()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };


        mockStatusManager.Setup(m => m.GetCountOfBookCurrentLent(It.IsAny<string>())).Returns(5);
        mockManager.Setup(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>())).Returns(libraryBook);
        mockStatusManager.Setup(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        mockStatusManager.Setup(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>())).Returns(1);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");


        //Act
        var result = controller.Insert(model);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        mockStatusManager.Verify(m => m.GetCountOfBookCurrentLent(It.IsAny<string>()), Times.Once());
        mockManager.Verify(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>()), Times.Never());
        mockStatusManager.Verify(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        mockStatusManager.Verify(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>()), Times.Never());


    }

    [Fact]
    public void Insert_Returns_BadRequest_When_BookCanNotBeFound_Inserted()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        LibraryBookApiModel nulllibraryBook = default!;
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };


        mockStatusManager.Setup(m => m.GetCountOfBookCurrentLent(It.IsAny<string>())).Returns(0);
        mockManager.Setup(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>())).Returns(nulllibraryBook);
        mockStatusManager.Setup(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        mockStatusManager.Setup(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>())).Returns(1);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");


        //Act
        var result = controller.Insert(model);


        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        mockStatusManager.Verify(m => m.GetCountOfBookCurrentLent(It.IsAny<string>()), Times.Once());
        mockManager.Verify(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>()), Times.Once());
        mockStatusManager.Verify(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        mockStatusManager.Verify(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>()), Times.Never());


    }

    [Fact]
    public void Insert_Returns_BadRequest_WhenUserAlreadyHasBook_Inserted()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };


        mockStatusManager.Setup(m => m.GetCountOfBookCurrentLent(It.IsAny<string>())).Returns(0);
        mockManager.Setup(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>())).Returns(libraryBook);
        mockStatusManager.Setup(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        mockStatusManager.Setup(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>())).Returns(1);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");


        //Act
        var result = controller.Insert(model);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        mockStatusManager.Verify(m => m.GetCountOfBookCurrentLent(It.IsAny<string>()), Times.Once());
        mockManager.Verify(m => m.GetLibraryBookByLibraryBookCode(It.IsAny<string>()), Times.Once());
        mockStatusManager.Verify(m => m.HasMoreThanOneBookWithSameISBN(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        mockStatusManager.Verify(m => m.InsertLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), out libraryBookStatus, It.IsAny<TransactionParam>()), Times.Never());
    }

    [Fact]
    public void Update_Returns_OkResult_When_Successfully_Updated()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };


        mockStatusManager.Setup(m => m.UpdateLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), It.IsAny<TransactionParam>())).Returns(1);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        //Act
        var result = controller.Update(model);

        //Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        mockStatusManager.Verify(m => m.UpdateLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), It.IsAny<TransactionParam>()), Times.Once());


    }

    [Fact]
    public void Update_Returns_BadRequest_When_FailsToUpdate()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };


        mockStatusManager.Setup(m => m.UpdateLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), It.IsAny<TransactionParam>())).Returns(0);
        HttpRequestMessage request = AddAuthorisation("role", controller);
        request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

        //Act
        var result = controller.Update(model);

        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var okResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        okResult.StatusCode.Should().Be(400);
        mockStatusManager.Verify(m => m.UpdateLibraryBookStatus(It.IsAny<LibraryBookStatusApiModel>(), It.IsAny<TransactionParam>()), Times.Once());


    }

    [Fact]
    public void GetLibraryUsersPaged_Gets_ListOfPagedUsers()
    {
        //Arrange
        var mockManager = new Mock<ILibraryBookWebApiManager>();
        var mockStatusManager = new Mock<ILibraryBookStatusWebApiManager>();
        var controller = new LibraryBookStatusController(mockStatusManager.Object, mockManager.Object);
        var libraryBook = new LibraryBookApiModel
        {
            // Initialize your model object here
            LibraryBookCode = "LibraryBookCode"
        };
        var libraryBookStatus = "Current Status";
        var model = new LibraryBookStatusApiModel
        {
            // Initialize your model object here
            LibraryBook = libraryBook,
            LibraryBookStatusCode = libraryBookStatus,
            LibraryUser = new LibraryUserApiModel
            {
                LibraryUserCode = "LibraryUserCode"
            }
        };
        List<LibraryBookStatusApiModel> list = new()
        {
            model
        };
        LibraryBookStatusPageApiModel pagelist = new()
        {
            Results = list,
            SearchResultCount = list.Count
        };
        PagedBase pagedBase = new PagedBase
        {

        };


        int searchResultsCount = 1;
        mockStatusManager.Setup(m => m.GetLibraryBookStatusPaged(It.IsAny<PagedBase>(), out searchResultsCount)).Returns(pagelist);

        //Act
        var result = controller.GetLibraryUsersPaged(pagedBase);

        //Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(pagelist);

    }
}

