

namespace DataAccess.WebApiManager.Manager
{
    using Common.Models;
    using Common.Models.Api;
    using DataAccess.WebApiManager.Interfaces;
    using DataAccess.WebApiRepository.Interfaces;

    public class LibraryUserWebApiManager : ILibraryUserWebApiManager
    {
        /// <summary>
        /// The libraryUser service repository.
        /// </summary>
        private readonly ILibraryUserRepository _libraryUserRepository;

        /// <summary>
        /// Initialises a new instance of the <see cref="LibraryUserWebApiManager" /> class.
        /// </summary>
        /// <param name="libraryUserRepository">
        /// The Branch repository
        /// </param>
        public LibraryUserWebApiManager(
            ILibraryUserRepository libraryUserRepository)
        {
            this._libraryUserRepository = libraryUserRepository;
        }

        /// <summary>
        /// Get a LibraryUser by it's id
        /// </summary>
        /// <param name="libraryUserCode"></param>
        /// <returns>The <see cref="LibraryUserApiModel"/> </returns>
        public LibraryUserApiModel GetLibraryUserByLibraryUserCode(string libraryUserCode)
        {
            return this._libraryUserRepository.GetLibraryUserByLibraryUserCode(libraryUserCode);
        }

        public ApiItemCollectionApiModel GetLibraryUsers(string search)
        {
            return this._libraryUserRepository.GetLibraryUsers(search);
        }

        /// <summary>
        /// GetLibraryUsersPaged for a sales location
        /// </summary>
        /// <param name="filterParameters"></param>
        /// <param name="searchResultCount"></param>
        /// <returns>The <see cref="LibraryUserPageApiModel"/> </returns>
        public LibraryUserPageApiModel GetLibraryUsersPaged(PagedBase filterParameters, out int searchResultCount)
        {
            return this._libraryUserRepository.GetLibraryUsersPaged(filterParameters, out searchResultCount);
        }


        /// <summary>
        /// Insert LibraryUser
        /// </summary>
        /// <param name="libraryUser"></param>
        /// <param name="libraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        public int InsertLibraryUser(LibraryUserApiModel libraryUser, out string libraryUserCode, TransactionParam transactionParam = null)
        {
            return this._libraryUserRepository.InsertLibraryUser(libraryUser, out libraryUserCode, transactionParam);
        }


        /// <summary>
        /// Updare LibraryUser
        /// </summary>
        /// <param name="libraryUser"></param>
        /// <returns>The<see cref="int"/> </returns>
        public int UpdateLibraryUser(LibraryUserApiModel libraryUser, TransactionParam transactionParam = null)
        {
            return this._libraryUserRepository.UpdateLibraryUser(libraryUser, transactionParam);
        }


        /// <summary>
        /// Delete LibraryUser
        /// </summary>
        /// <param name="libraryUserCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        public int DeleteLibraryUser(string libraryUserCode)
        {
            return this._libraryUserRepository.DeleteLibraryUser(libraryUserCode);
        }
    }
}
