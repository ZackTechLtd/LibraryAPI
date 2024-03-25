using Common.Models;
using Common.Models.Api;
using LibraryApp.Infrastructure.WebApiManager.Interfaces;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers;

[Produces("application/json")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Member")]
[Route("api/[controller]")]
public class LibraryBookStatusController : BaseController
{
    /// <summary>
    /// The Library API Managers (Dependency Injection)
    /// </summary>
    private readonly ILibraryBookStatusWebApiManager _libraryBookStatusWebApiManager;
    private readonly ILibraryBookWebApiManager _libraryBookWebApiManager;

    public LibraryBookStatusController(ILibraryBookStatusWebApiManager libraryBookStatusWebApiManager, ILibraryBookWebApiManager libraryBookWebApiManager)
    {
        _libraryBookStatusWebApiManager = libraryBookStatusWebApiManager;
        _libraryBookWebApiManager = libraryBookWebApiManager;
    }

    [HttpGet("ID/{id}", Name = "GetLibraryBookStatus")]
    public IActionResult Get(string id)
    {
        var librarbookstatus = _libraryBookStatusWebApiManager.GetLibraryBookStatusByLibraryBookStatusCode(id);

        if (librarbookstatus == null)
        {
            ModelState.AddModelError(string.Empty, "Unknown code");
            return BadRequest(ModelState);
        }

        return Ok(librarbookstatus);
    }

    [HttpPost]
    public IActionResult Insert([FromBody] LibraryBookStatusApiModel model)
    {
        int result = 0;
        var libraryBookStatusCode = "";

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        if (model != null)
        {
            int? retVal = _libraryBookStatusWebApiManager.GetCountOfBookCurrentLent(model!.LibraryUser.LibraryUserCode);
            if (retVal != null && retVal.Value > 4)
            {
                ModelState.AddModelError(string.Empty, "Maximum number of books exceeded");
                return BadRequest(ModelState);
            }

            LibraryBookApiModel libraryBookApiModel = _libraryBookWebApiManager.GetLibraryBookByLibraryBookCode(model!.LibraryBook!.LibraryBookCode);
            if (libraryBookApiModel == null)
            {
                ModelState.AddModelError(string.Empty, "Library book not found");
                return BadRequest(ModelState);
            }

            if (_libraryBookStatusWebApiManager.HasMoreThanOneBookWithSameISBN(libraryBookApiModel.ISBN, model!.LibraryUser.LibraryUserCode))
            {
                ModelState.AddModelError(string.Empty, "Library User already has that book");
                return BadRequest(ModelState);
            }


            model.CreatedBy = GetCurrentUser();
            model.DateCreated = DateTime.Now;
            model.ModifiedBy = GetCurrentUser();
            model.DateModified = DateTime.Now;

            result = _libraryBookStatusWebApiManager.InsertLibraryBookStatus(model, out libraryBookStatusCode);
        }

        switch (result)
        {
            case 1:
                return new ContentResult
                {
                    Content = libraryBookStatusCode,
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
    public IActionResult Update([FromBody] LibraryBookStatusApiModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        model.ModifiedBy = GetCurrentUser();

        var result = _libraryBookStatusWebApiManager.UpdateLibraryBookStatus(model);

        switch (result)
        {
            case 1:
                return Ok();

            default:
                ModelState.AddModelError(string.Empty, "Unable to Update book status");
                break;
        }

        return BadRequest(ModelState);
    }

    [Route("Paged")]
    [HttpGet]
    public IActionResult GetLibraryUsersPaged(PagedBase filterParameters)
    {
        var contactPagedListApiModel = _libraryBookStatusWebApiManager.GetLibraryBookStatusPaged(filterParameters, out int searchResultCount);

        return Ok(contactPagedListApiModel);
    }
}

