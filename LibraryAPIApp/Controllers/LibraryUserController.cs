using Common.Models;
using Common.Models.Api;
using DataAccess.WebApiManager.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LibraryAPIApp.Controllers
{
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Member")]
    [Route("api/[controller]")]
    public class LibraryUserController : BaseController
    {
        /// <summary>
        /// The Library User API Manager (Dependency Injection)
        /// </summary>
        private readonly ILibraryUserWebApiManager _libraryUserWebApiManager;

        public LibraryUserController(ILibraryUserWebApiManager libraryUserWebApiManager)
        {
            _libraryUserWebApiManager = libraryUserWebApiManager;
        }

        [HttpGet("ID/{id}", Name = "GetLibraryUser")]
        public IActionResult Get(string id)
        {
            var libraryuser = _libraryUserWebApiManager.GetLibraryUserByLibraryUserCode(id);

            if (libraryuser == null)
            {
                ModelState.AddModelError(string.Empty, "Unknown user code");
                return BadRequest(ModelState);
            }

            return Ok(libraryuser);
        }

        [HttpPost]
        public IActionResult Insert([FromBody] LibraryUserApiModel model)
        {
            int result = 0;
            var libraryUserCode = "";

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model != null)
            {

                model.CreatedBy = GetCurrentUser();
                model.DateCreated = DateTime.Now;
                model.ModifiedBy = GetCurrentUser();
                model.DateModified = DateTime.Now;

                result = _libraryUserWebApiManager.InsertLibraryUser(model, out libraryUserCode);
            }

            switch (result)
            {
                case 1:
                    return new ContentResult
                    {
                        Content = libraryUserCode,
                        ContentType = "text/plain",
                        StatusCode = 200
                    };
                default:
                    ModelState.AddModelError(string.Empty, "Failed to insert record");
                    break;
            }

            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult Update([FromBody] LibraryUserApiModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.ModifiedBy = GetCurrentUser();

            var result = _libraryUserWebApiManager.UpdateLibraryUser(model);

            switch (result)
            {
                case 1:
                    return Ok();

                default:
                    ModelState.AddModelError(string.Empty, "Unable to Update user");
                    break;
            }

            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            var result = _libraryUserWebApiManager.DeleteLibraryUser(id);
            if (result == 1)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable To Delete User");
                return BadRequest(ModelState);
            }
        }

        [Route("Paged")]
        [HttpGet]
        public IActionResult GetLibraryUsersPaged(PagedBase filterParameters)
        {
            var libraryusersPagedListApiModel = _libraryUserWebApiManager.GetLibraryUsersPaged(filterParameters, out int searchResultCount);

            return Ok(libraryusersPagedListApiModel);
        }

        [Route("List")]
        [HttpGet]
        public IActionResult GetLibraryUsers(string search)
        {
            var usersApiModel = _libraryUserWebApiManager.GetLibraryUsers(search);
            
            return Ok(usersApiModel);
        }
    }
}