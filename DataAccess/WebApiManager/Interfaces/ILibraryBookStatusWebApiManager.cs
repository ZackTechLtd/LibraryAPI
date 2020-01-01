

namespace DataAccess.WebApiManager.Interfaces
{
    using Common.Models;
    using Common.Models.Api;
    public interface ILibraryBookStatusWebApiManager
    {
        /// <summary>
        /// Get a LibraryBookStatus by it's id
        /// </summary>
        /// <param name="libraryBookStatusCode"></param>
        /// <returns>The <see cref="LibraryBookStatusApiModel"/> </returns>
        LibraryBookStatusApiModel GetLibraryBookStatusByLibraryBookStatusCode(string libraryBookStatusCode);

        int? GetCountOfBookCurrentLent(string libraryUserCode);


        bool HasMoreThanOneBookWithSameISBN(string isbn, string libraryUserCode);


        /// <summary>
        /// GetLibraryBookStatussPaged for a sales location
        /// </summary>
        /// <param name="filterParameters"></param>
        /// <param name="searchResultCount"></param>
        /// <returns>The <see cref="LibraryBookStatusPageApiModel"/> </returns>
        LibraryBookStatusPageApiModel GetLibraryBookStatusPaged(PagedBase filterParameters, out int searchResultCount);


        /// <summary>
        /// Insert LibraryBookStatus
        /// </summary>
        /// <param name="libraryBookStatus"></param>
        /// <param name="libraryBookStatusCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int InsertLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, out string libraryBookStatusCode, TransactionParam transactionParam = null);


        /// <summary>
        /// Updare LibraryBookStatus
        /// </summary>
        /// <param name="libraryBookStatus"></param>
        /// <returns>The<see cref="int"/> </returns>
        int UpdateLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, TransactionParam transactionParam = null);

        /// <summary>
        /// Delete LibraryBookStatus
        /// </summary>
        /// <param name="LibraryBookStatusCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int DeleteLibraryBookStatus(string LibraryBookStatusCode);
    }
}
