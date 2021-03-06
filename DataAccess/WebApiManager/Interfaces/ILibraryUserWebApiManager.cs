﻿

namespace DataAccess.WebApiManager.Interfaces
{
    using Common.Models;
    using Common.Models.Api;
    using System.Collections.Generic;


    public interface ILibraryUserWebApiManager
    {
        /// <summary>
        /// Get a LibraryUser by it's id
        /// </summary>
        /// <param name="libraryUserCode"></param>
        /// <returns>The <see cref="LibraryUserApiModel"/> </returns>
        LibraryUserApiModel GetLibraryUserByLibraryUserCode(string libraryUserCode);


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
        /// <param name="libraryUser"></param>
        /// <param name="libraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int InsertLibraryUser(LibraryUserApiModel libraryUser, out string libraryUserCode, TransactionParam transactionParam = null);


        /// <summary>
        /// Updare LibraryUser
        /// </summary>
        /// <param name="libraryUser"></param>
        /// <returns>The<see cref="int"/> </returns>
        int UpdateLibraryUser(LibraryUserApiModel libraryUser, TransactionParam transactionParam = null);


        /// <summary>
        /// Delete LibraryUser
        /// </summary>
        /// <param name="libraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        int DeleteLibraryUser(string libraryUserCode);
    }
}
