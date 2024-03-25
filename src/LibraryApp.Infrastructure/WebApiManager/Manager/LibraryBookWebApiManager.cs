using System;
using Common.Models;
using Common.Models.Api;
using LibraryApp.Infrastructure.Helper;
using LibraryApp.Infrastructure.Repository;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;

namespace LibraryApp.Infrastructure.WebApiManager.Manager;

public class LibraryBookWebApiManager : ILibraryBookWebApiManager
{
    /// <summary>
    /// The libraryBook service repository.
    /// </summary>
    private readonly ILibraryBookRepository _libraryBookRepository;

    /// <summary>
    /// Initialises a new instance of the <see cref="LibraryBookWebApiManager" /> class.
    /// </summary>
    /// <param name="libraryBookRepository">
    /// The Branch repository
    /// </param>
    public LibraryBookWebApiManager(
        ILibraryBookRepository libraryBookRepository)
    {
        this._libraryBookRepository = libraryBookRepository;
    }

    /// <summary>
    /// Get a LibraryBook by it's id
    /// </summary>
    /// <param name="libraryBookCode"></param>
    /// <returns>The <see cref="LibraryBookApiModel"/> </returns>
    public LibraryBookApiModel GetLibraryBookByLibraryBookCode(string libraryBookCode)
    {
        return this._libraryBookRepository.GetLibraryBookByLibraryBookCode(libraryBookCode);
    }

    /// <summary>
    /// GetLibraryBooksPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="userName"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryBookPageApiModel"/> </returns>
    public LibraryBookPageApiModel GetLibraryBooksPaged(PagedBase filterParameters, bool listLostAndStolen, out int searchResultCount)
    {
        return this._libraryBookRepository.GetLibraryBooksPaged(filterParameters, listLostAndStolen, out searchResultCount);
    }

    public ApiItemCollectionApiModel GetBooks(string search)
    {
        return this._libraryBookRepository.GetBooks(search);
    }


    /// <summary>
    /// Insert LibraryBook
    /// </summary>
    /// <param name="libraryBook"></param>
    /// <param name="libraryBookCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int InsertLibraryBook(LibraryBookApiModel libraryBook, out string libraryBookCode, TransactionParam transactionParam = default!)
    {
        return this._libraryBookRepository.InsertLibraryBook(libraryBook, out libraryBookCode, transactionParam);
    }


    /// <summary>
    /// Updare LibraryBook
    /// </summary>
    /// <param name="libraryBook"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int UpdateLibraryBook(LibraryBookApiModel libraryBook, TransactionParam transactionParam = default!)
    {
        return this._libraryBookRepository.UpdateLibraryBook(libraryBook, transactionParam);
    }


    /// <summary>
    /// Delete LibraryBook
    /// </summary>
    /// <param name="libraryBookCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int DeleteLibraryBook(string libraryBookCode)
    {
        return this._libraryBookRepository.DeleteLibraryBook(libraryBookCode);
    }
}

