using Common.Models;
using Common.Models.Api;
using LibraryApp.Infrastructure.Helper;
using LibraryApp.Infrastructure.Repository;
using LibraryApp.Infrastructure.WebApiManager.Interfaces;

namespace LibraryApp.Infrastructure.WebApiManager.Manager;

public class LibraryBookStatusWebApiManager : ILibraryBookStatusWebApiManager
{
    /// <summary>
    /// The libraryBook service repository.
    /// </summary>
    private readonly ILibraryBookStatusRepository _libraryBookStatusRepository;

    /// <summary>
    /// Initialises a new instance of the <see cref="LibraryBookWebApiManager" /> class.
    /// </summary>
    /// <param name="libraryBookRepository">
    /// The Branch repository
    /// </param>
    public LibraryBookStatusWebApiManager(
        ILibraryBookStatusRepository libraryBookStatusRepository)
    {
        this._libraryBookStatusRepository = libraryBookStatusRepository;
    }

    /// <summary>
    /// Get a LibraryBookStatus by it's id
    /// </summary>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The <see cref="LibraryBookStatusApiModel"/> </returns>
    public LibraryBookStatusApiModel GetLibraryBookStatusByLibraryBookStatusCode(string libraryBookStatusCode)
    {
        return _libraryBookStatusRepository.GetLibraryBookStatusByLibraryBookStatusCode(libraryBookStatusCode);
    }


    public int? GetCountOfBookCurrentLent(string libraryUserCode)
    {
        return _libraryBookStatusRepository.GetCountOfBookCurrentLent(libraryUserCode);
    }

    public bool HasMoreThanOneBookWithSameISBN(string isbn, string libraryUserCode)
    {
        return _libraryBookStatusRepository.HasMoreThanOneBookWithSameISBN(isbn, libraryUserCode);
    }

    /// <summary>
    /// GetLibraryBookStatussPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryBookStatusPageApiModel"/> </returns>
    public LibraryBookStatusPageApiModel GetLibraryBookStatusPaged(PagedBase filterParameters, out int searchResultCount)
    {
        return _libraryBookStatusRepository.GetLibraryBookStatusPaged(filterParameters, out searchResultCount);
    }


    /// <summary>
    /// Insert LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatus"></param>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int InsertLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, out string libraryBookStatusCode, TransactionParam transactionParam = default!)
    {
        return _libraryBookStatusRepository.InsertLibraryBookStatus(libraryBookStatus, out libraryBookStatusCode, transactionParam);
    }


    /// <summary>
    /// Updare LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatus"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int UpdateLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, TransactionParam transactionParam = default!)
    {
        return _libraryBookStatusRepository.UpdateLibraryBookStatus(libraryBookStatus, transactionParam);
    }

    /// <summary>
    /// Delete LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int DeleteLibraryBookStatus(string libraryBookStatusCode)
    {
        return _libraryBookStatusRepository.DeleteLibraryBookStatus(libraryBookStatusCode);
    }
}