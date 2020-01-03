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
    public class LibraryBookController : BaseController
    {
        /// <summary>
        /// The Library Book API Manager (Dependency Injection)
        /// </summary>
        private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

        public LibraryBookController(ILibraryBookWebApiManager libraryBookWebApiManager)
        {
            _libraryBookWebApiManager = libraryBookWebApiManager;
        }

        
        [HttpGet("ID/{id}", Name = "GetLibraryBook")]
        public IActionResult Get(string id)
        {
            var librarybook = _libraryBookWebApiManager.GetLibraryBookByLibraryBookCode(id);

            if (librarybook == null)
            {
                ModelState.AddModelError(string.Empty, "Unknown book code");
                return BadRequest(ModelState);
            }

            return Ok(librarybook);
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrSeniorLibrarian")]
        [HttpPost]
        public IActionResult Insert([FromBody] LibraryBookApiModel model)
        {
            int result = 0;
            var libraryBookCode = "";

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

                result = _libraryBookWebApiManager.InsertLibraryBook(model, out libraryBookCode);
            }

            switch (result)
            {
                case 1:
                    return new ContentResult
                    {
                        Content = libraryBookCode,
                        ContentType = "text/plain",
                        StatusCode = 200
                    };
                default:
                    ModelState.AddModelError(string.Empty, "Failed to insert record");
                    break;
            }

            return BadRequest(ModelState);
        }

        
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Administrator")]
        [HttpPut]
        public IActionResult Update([FromBody] LibraryBookApiModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.ModifiedBy = GetCurrentUser();

            var result = _libraryBookWebApiManager.UpdateLibraryBook(model);

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

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrSeniorLibrarian")]
        [HttpDelete]
        public IActionResult Delete(string id)
        {
            var result = _libraryBookWebApiManager.DeleteLibraryBook(id);
            if (result == 1)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Unable To Delete Book");
                return BadRequest(ModelState);
            }
        }

        [Route("Paged")]
        [HttpGet]
        public IActionResult GetLibraryBooksPaged(PagedBase filterParameters, bool listLostAndStolen)
        {
            var booksPagedListApiModel = _libraryBookWebApiManager.GetLibraryBooksPaged(filterParameters, listLostAndStolen,  out int searchResultCount);

            return Ok(booksPagedListApiModel);
        }

        [Route("List")]
        [HttpGet]
        public IActionResult GetLibraryBooks(string search)
        {
            var booksApiModel = _libraryBookWebApiManager.GetBooks(search);
            return Ok(booksApiModel);
        }
    }
}
