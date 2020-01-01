

namespace DataAccess.WebApiRepository.Interfaces
{
    using Common.Models;
    using Common.Models.Api;
    using System.Collections.Generic;

    public interface ILibraryUserRepository
    {
        /// <summary>
        /// Get a LibraryUser by it's id
        /// </summary>
        /// <param name="LibraryUserCode"></param>
        /// <returns>The <see cref="LibraryUserApiModel"/> </returns>
        LibraryUserApiModel GetLibraryUserByLibraryUserCode(string LibraryUserCode);


        ApiItemCollectionApiModel GetLibraryUsers(string search);

        /// <summary>
        /// GetLibraryUsersPaged for a sales location
        /// </summary>
        /// <param name="filterParameters"></param>
        /// <param name="searchResultCount"></param>
        /// <returns>The <see cref="LibraryUserPageApiModel"/> </returns>
        LibraryUserPageApiModel GetLibraryUsersPaged(PagedBase filterParameters, out int searchResultCount);
        

        /// <summary>
        /// Insert LibraryUser
        /// </summary>
        /// <param name="LibraryUser"></param>
        /// <param name="LibraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int InsertLibraryUser(LibraryUserApiModel LibraryUser, out string LibraryUserCode, TransactionParam transactionParam = null);
        

        /// <summary>
        /// Updare LibraryUser
        /// </summary>
        /// <param name="LibraryUser"></param>
        /// <returns>The<see cref="int"/> </returns>
        int UpdateLibraryUser(LibraryUserApiModel LibraryUser, TransactionParam transactionParam = null);
        

        /// <summary>
        /// Delete LibraryUser
        /// </summary>
        /// <param name="LibraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int DeleteLibraryUser(string LibraryUserCode);
        
    }
}
