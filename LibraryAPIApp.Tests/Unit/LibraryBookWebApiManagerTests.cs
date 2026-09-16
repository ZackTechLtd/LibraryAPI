using System.Collections.Generic;
using Common.Models;
using Common.Models.Api;
using DataAccess;
using DataAccess.WebApiManager.Interfaces;
using DataAccess.WebApiManager.Manager;
using DataAccess.WebApiRepository.Interfaces;
using Moq;

namespace LibraryAPIApp.Tests.Unit;

public class LibraryBookWebApiManagerTests
{
    private readonly Mock<ILibraryBookRepository> _repository;
    private readonly LibraryBookWebApiManager _manager;

    public LibraryBookWebApiManagerTests()
    {
        _repository = new Mock<ILibraryBookRepository>();
        _manager = new LibraryBookWebApiManager(_repository.Object);
    }

    [Fact]
    public void GetLibraryBookByLibraryBookCode_DelegatesToRepository()
    {
        var expected = new LibraryBookApiModel { LibraryBookCode = "ABC" };
        _repository.Setup(r => r.GetLibraryBookByLibraryBookCode("ABC")).Returns(expected);

        var result = _manager.GetLibraryBookByLibraryBookCode("ABC");

        Assert.Same(expected, result);
        _repository.Verify(r => r.GetLibraryBookByLibraryBookCode("ABC"), Times.Once);
    }

    [Fact]
    public void GetLibraryBooksPaged_DelegatesToRepositoryAndForwardsOutValue()
    {
        var filters = new PagedBase();
        var expected = new LibraryBookPageApiModel();
        int expectedCount = 12;

        _repository
            .Setup(r => r.GetLibraryBooksPaged(It.IsAny<PagedBase>(), It.IsAny<bool>(), out expectedCount))
            .Returns(expected);

        var result = _manager.GetLibraryBooksPaged(filters, false, out int actualCount);

        Assert.Same(expected, result);
        Assert.Equal(12, actualCount);
        _repository.Verify(r => r.GetLibraryBooksPaged(filters, false, out expectedCount), Times.Once);
    }

    [Fact]
    public void GetBooks_DelegatesToRepository()
    {
        var expected = new ApiItemCollectionApiModel();
        _repository.Setup(r => r.GetBooks("harry")).Returns(expected);

        var result = _manager.GetBooks("harry");

        Assert.Same(expected, result);
        _repository.Verify(r => r.GetBooks("harry"), Times.Once);
    }

    [Fact]
    public void InsertLibraryBook_DelegatesToRepositoryAndForwardsOutValue()
    {
        var libraryBook = new LibraryBookApiModel();
        string expectedCode = "ABC123";

        _repository
            .Setup(r => r.InsertLibraryBook(libraryBook, out expectedCode, It.IsAny<TransactionParam>()))
            .Returns(1);

        var result = _manager.InsertLibraryBook(libraryBook, out string actualCode);

        Assert.Equal(1, result);
        Assert.Equal("ABC123", actualCode);
        _repository.Verify(r => r.InsertLibraryBook(libraryBook, out expectedCode, It.IsAny<TransactionParam>()), Times.Once);
    }

    [Fact]
    public void UpdateLibraryBook_DelegatesToRepository()
    {
        var libraryBook = new LibraryBookApiModel();
        _repository.Setup(r => r.UpdateLibraryBook(libraryBook, It.IsAny<TransactionParam>())).Returns(1);

        var result = _manager.UpdateLibraryBook(libraryBook);

        Assert.Equal(1, result);
        _repository.Verify(r => r.UpdateLibraryBook(libraryBook, It.IsAny<TransactionParam>()), Times.Once);
    }

    [Fact]
    public void DeleteLibraryBook_DelegatesToRepository()
    {
        _repository.Setup(r => r.DeleteLibraryBook("ABC")).Returns(1);

        var result = _manager.DeleteLibraryBook("ABC");

        Assert.Equal(1, result);
        _repository.Verify(r => r.DeleteLibraryBook("ABC"), Times.Once);
    }
}