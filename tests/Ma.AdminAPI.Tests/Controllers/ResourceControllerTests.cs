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

public class ResourceControllerTests
{
    private readonly IExtendedResourceStore _store;
    private readonly ResourcesController _controller;
    private static readonly List<string> s_scopes = new() {"test_scope"};
    private const string ResourceName = "resource_name";
    private readonly ApiResourceRecord _apiResourceRecord = new(ResourceName, s_scopes);

    public ResourceControllerTests()
    {
        _store = Substitute.For<IExtendedResourceStore>();
        _controller = new ResourcesController(_store);
    }
    
    [Fact]
    public async void Save_RequestIsValid_StoreSaveCalled()
    {
        //act
        _ = await _controller.Save(_apiResourceRecord);

        //assert 
        await _store.Received(1).Save(Arg.Is<ApiResource>(apiResource => apiResource.Name == _apiResourceRecord.Name));
    }

    [Fact]
    public async void Save_RequestIsValidRobotAlreadyExist_OkResultReturned()
    {
        //arrange
        _store.Exist(Arg.Any<ApiResource>()).Returns(true);
        
        //act
        var result = await _controller.Save(_apiResourceRecord);

        //assert 
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<ApiResourceRecord>().Which.Name.Should().Be(ResourceName);
    }
    
    [Fact]
    public async void Save_RequestIsValidRobotDoesNotExist_CreatedActionReturned()
    {
        //act
        var result = await _controller.Save(_apiResourceRecord);

        //assert 
        result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().BeOfType<ApiResourceRecord>().Which.Name.Should().Be(ResourceName);
    }
}
