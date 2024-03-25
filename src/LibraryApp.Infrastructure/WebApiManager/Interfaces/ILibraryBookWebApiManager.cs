using Common.Models;
using Common.Models.Api;
using LibraryApp.Infrastructure.Helper;

namespace LibraryApp.WebApiManager.Infrastructure.Interfaces;

public interface ILibraryBookWebApiManager
{
    /// <summary>
    /// Get a LibraryBook by it's id
    /// </summary>
    /// <param name="LibraryBookCode"></param>
    /// <returns>The <see cref="LibraryBookApiModel"/> </returns>
    LibraryBookApiModel GetLibraryBookByLibraryBookCode(string LibraryBookCode);

    ApiItemCollectionApiModel GetBooks(string search);

    /// <summary>
    /// GetLibraryBooksPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="listLostAndStolen"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryBookPageApiModel"/> </returns>
    LibraryBookPageApiModel GetLibraryBooksPaged(PagedBase filterParameters, bool listLostAndStolen, out int searchResultCount);

    /// <summary>
    /// Insert LibraryBook
    /// </summary>
    /// <param name="LibraryBook"></param>
    /// <param name="LibraryBookCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    int InsertLibraryBook(LibraryBookApiModel LibraryBook, out string LibraryBookCode, TransactionParam transactionParam = default!);


    /// <summary>
    /// Updare LibraryBook
    /// </summary>
    /// <param name="LibraryBook"></param>
    /// <returns>The<see cref="int"/> </returns>
    int UpdateLibraryBook(LibraryBookApiModel LibraryBook, TransactionParam transactionParam = default!);

    /// <summary>
    /// Delete LibraryBook
    /// </summary>
    /// <param name="LibraryBookCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    int DeleteLibraryBook(string LibraryBookCode);
}

