using System.Collections.Generic;
using Duende.IdentityServer.Models;
using FluentAssertions;
using Ma.AdminAPI.Controllers;
using Ma.Contracts;
using Ma.Model;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Ma.AdminAPI.Tests.Controllers;

public class RobotsControllerTests
{
    private readonly IExtendedClientStore _store;
    private readonly RobotsController _controller;
    private static readonly List<string> s_scopes = new() {"test_scope"};
    private const string RobotName = "robot_name";
    private readonly RobotRecord _robot = new("robot_id", RobotName, s_scopes,new List<string>{"test_thumbprint"},3600);

    public RobotsControllerTests()
    {
        _store = Substitute.For<IExtendedClientStore>();
        _controller = new RobotsController(_store);
    }
    
    [Fact]
    public async void Save_RequestIsValid_StoreSaveCalled()
    {
        //arrange
        _store.AllScopeExist(s_scopes).Returns(true);
        
        //act
        _ = await _controller.Save(_robot);

        //assert 
        await _store.Received(1).Save(Arg.Is<Client>(client => client.ClientId == _robot.RobotId));
    }
    
    [Fact]
    public async void Save_RequestIsInvalid_BadRequestReturned()
    {
        //arrange
        _store.AllScopeExist(s_scopes).Returns(false);
        
        //act
        var result = await _controller.Save(_robot);

        //assert 
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async void Save_RequestIsValidRobotAlreadyExist_OkResultReturned()
    {
        //arrange
        _store.AllScopeExist(s_scopes).Returns(true);
        _store.Exist(Arg.Any<Client>()).Returns(true);
        
        //act
        var result = await _controller.Save(_robot);

        //assert 
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<RobotRecord>().Which.Name.Should().Be(RobotName);
    }
    
    [Fact]
    public async void Save_RequestIsValidRobotDoesNotExist_CreatedActionReturned()
    {
        //arrange
        _store.AllScopeExist(s_scopes).Returns(true);

        //act
        var result = await _controller.Save(_robot);

        //assert 
        result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().BeOfType<RobotRecord>().Which.Name.Should().Be(RobotName);
    }
}
