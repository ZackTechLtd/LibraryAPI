﻿using System.Security.Claims;
using Common.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers;

//[AllowAnonymous]
//[Authorize(Policy = "Member")]
//[Authorize(AuthenticationSchemes = "Bearer", Policy = "Member")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // GET: api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/values
    //[Authorize(AuthenticationSchemes = "Bearer", Policy = "Administrator")]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost]
    public IActionResult Post([FromBody] LibraryBookApiModel model)
    {
        ClaimsIdentity claimsIdentity = User.Identity as ClaimsIdentity ?? default!;

        var claims = claimsIdentity.Claims.Select(x => new { type = x.Type, value = x.Value });

        //var rolesClaim = this.HttpContext.User.Claims;
        if (model != null)
            return Ok("Model OK");
        else
            return Ok("Model Null");
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
