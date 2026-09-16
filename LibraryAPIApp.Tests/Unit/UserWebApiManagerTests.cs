using System.Collections.Generic;
using Common.Models;
using Common.Models.Api;
using DataAccess.IdentityModels;
using DataAccess.WebApiManager.Interfaces;
using DataAccess.WebApiManager.Manager;
using DataAccess.WebApiRepository.Interfaces;
using Moq;

namespace LibraryAPIApp.Tests.Unit;

public class UserWebApiManagerTests
{
    private readonly Mock<IUserRepository> _repository;
    private readonly UserWebApiManager _manager;

    public UserWebApiManagerTests()
    {
        _repository = new Mock<IUserRepository>();
        _manager = new UserWebApiManager(_repository.Object);
    }

    [Fact]
    public void GetUserByUserName_DelegatesToRepository()
    {
        var expected = new ApplicationUser { UserName = "alice" };
        _repository.Setup(r => r.GetUserByUserName("alice")).Returns(expected);

        var result = _manager.GetUserByUserName("alice");

        Assert.Same(expected, result);
        _repository.Verify(r => r.GetUserByUserName("alice"), Times.Once);
    }

    [Fact]
    public void GetUsersPaged_DelegatesToRepositoryAndForwardsOutValue()
    {
        var filters = new PagedBase();
        var expected = new List<User> { new User { UserName = "alice" } };
        int expectedCount = 7;

        _repository
            .Setup(r => r.GetUsersPaged(It.IsAny<PagedBase>(), It.IsAny<string>(), It.IsAny<bool>(), out expectedCount))
            .Returns(expected);

        var result = _manager.GetUsersPaged(filters, "alice", true, out int actualCount);

        Assert.Same(expected, result);
        Assert.Equal(7, actualCount);
        _repository.Verify(r => r.GetUsersPaged(filters, "alice", true, out expectedCount), Times.Once);
    }

    [Fact]
    public void GetUserAndRolesByUserName_DelegatesToRepository()
    {
        var expected = new List<Role> { new Role { RoleName = "Administrator" } };
        _repository.Setup(r => r.GetUserAndRolesByUserName("alice", "webapi")).Returns(expected);

        var result = _manager.GetUserAndRolesByUserName("alice", "webapi");

        Assert.Same(expected, result);
        _repository.Verify(r => r.GetUserAndRolesByUserName("alice", "webapi"), Times.Once);
    }

    [Fact]
    public void GetAllRoles_DelegatesToRepository()
    {
        var expected = new List<Role> { new Role { RoleName = "Librarian" } };
        _repository.Setup(r => r.GetAllRoles()).Returns(expected);

        var result = _manager.GetAllRoles();

        Assert.Same(expected, result);
        _repository.Verify(r => r.GetAllRoles(), Times.Once);
    }

    [Fact]
    public void UpdateUser_DelegatesToRepository()
    {
        var user = new User { UserName = "alice" };
        _repository.Setup(r => r.UpdateUser(user)).Returns(1);

        var result = _manager.UpdateUser(user);

        Assert.Equal(1, result);
        _repository.Verify(r => r.UpdateUser(user), Times.Once);
    }

    [Fact]
    public void DeleteUser_DelegatesToRepository()
    {
        _repository.Setup(r => r.DeleteUser("alice")).Returns(1);

        var result = _manager.DeleteUser("alice");

        Assert.Equal(1, result);
        _repository.Verify(r => r.DeleteUser("alice"), Times.Once);
    }

    [Fact]
    public void IsUserInRole_DelegatesToRepository()
    {
        _repository.Setup(r => r.IsUserInRole("Administrator", "alice")).Returns(true);

        var result = _manager.IsUserInRole("Administrator", "alice");

        Assert.True(result);
        _repository.Verify(r => r.IsUserInRole("Administrator", "alice"), Times.Once);
    }

    [Fact]
    public void GetNumberOfCompanyUsersByBranchCode_DelegatesToRepository()
    {
        _repository.Setup(r => r.GetNumberOfCompanyUsersByBranchCode("MAIN", true)).Returns(3);

        var result = _manager.GetNumberOfCompanyUsersByBranchCode("MAIN");

        Assert.Equal(3, result);
        _repository.Verify(r => r.GetNumberOfCompanyUsersByBranchCode("MAIN", true), Times.Once);
    }
}