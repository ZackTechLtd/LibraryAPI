

namespace DataAccess.WebApiRepository.Repository
{

    using Common.Configuration;
    using Common.Models;
    using Common.Models.Api;
    using Common.Util;
    using Dapper;
    using DataAccess.WebApiRepository.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    public class LibraryBookRepository : RepositoryBase, ILibraryBookRepository
    {
        protected readonly IRandomKeyGenerator _randomKeyGenerator;
        protected readonly IOptions<ApiConfiguration> _apiConfiguration;

        /// <summary>
        /// Initializes a new instance of the<see cref="LibraryBookRepository"/> class.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="apiConfiguration"></param>
        /// <param name="randomKeyGenerator"></param>
        public LibraryBookRepository(IConfiguration config, IOptions<ApiConfiguration> apiConfiguration, IRandomKeyGenerator randomKeyGenerator) : base(config, apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
            _randomKeyGenerator = randomKeyGenerator;
        }


        /// <summary>
        /// Get a LibraryBook by it's id
        /// </summary>
        /// <param name="LibraryBookCode"></param>
        /// <returns>The <see cref="LibraryBookApiModel"/> </returns>
        public LibraryBookApiModel GetLibraryBookByLibraryBookCode(string LibraryBookCode)
        {
            LibraryBookApiModel LibraryBookApiModel = null;
            using (var connection = this.OpenConnection())
            {
                LibraryBookApiModel = connection.Query<LibraryBookApiModel>(@"SELECT C.LibraryBookCode, C.ISBN, C.Title, C.Author, C.IsStolen, C.IsLost, C.CopyNumber,
                         C.DateCreated, C.CreatedBy, C.DateModified, C.ModifiedBy
                         FROM LibraryBook C
	                     WHERE C.LibraryBookCode = @LibraryBookCode", new { LibraryBookCode }).SingleOrDefault();
            }

            return LibraryBookApiModel;

        }


        public ApiItemCollectionApiModel GetBooks(string search)
        {
            ApiItemCollectionApiModel apiItemCollectionApiModel = new ApiItemCollectionApiModel();

            using (var connection = this.OpenConnection())
            {
                apiItemCollectionApiModel.Results = connection.Query<ApiItem>(@"SELECT C.LibraryBookCode AS Code, C.Title As Name
                         FROM LibraryBook C
	                     WHERE C.Title LIKE @search LIMIT 20", new { search });
            }

            return apiItemCollectionApiModel;

        }


        /// <summary>
        /// GetLibraryBooksPaged for a sales location
        /// </summary>
        /// <param name="filterParameters"></param>
        /// <param name="listLostAndStolen"></param>
        /// <param name="searchResultCount"></param>
        /// <returns>The <see cref="LibraryBookPageApiModel"/> </returns>
        public virtual LibraryBookPageApiModel GetLibraryBooksPaged(PagedBase filterParameters, bool listLostAndStolen, out int searchResultCount)
        {
            LibraryBookPageApiModel LibraryBookPageApiModel = new LibraryBookPageApiModel();
            searchResultCount = 0;
            IEnumerable<LibraryBookListItem> LibraryBooks = null;

            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT C.LibraryBookCode, C.ISBN, C.Title, C.Author, C.IsStolen, C.IsLost, C.CopyNumber, C.DateCreated, C.CreatedBy, C.DateModified, C.ModifiedBy,
            ( SELECT COUNT(*) FROM LibraryBook ) AS 'TotalRows' FROM LibraryBook C
	        WHERE ");

            if (!listLostAndStolen)
            {
                sb.Append(" C.IsStolen = 0 AND C.IsLost = 0 ");
                sb.Append(@" AND ");
            }

            sb.Append(@"
                (
                    
			        C.ISBN LIKE @searchText
                    OR C.Title LIKE @searchText
                    OR C.Author LIKE @searchText
                    OR C.IsStolen LIKE @searchText
                    OR C.IsLost LIKE @searchText
                    OR C.CopyNumber LIKE @searchText
                    OR C.ModifiedBy LIKE @searchText
		            OR C.DateModified LIKE @searchText
		        )
            order by
                CASE WHEN @orderBy = 0 THEN C.DateModified END ASC,
                 CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN C.ISBN END DESC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN C.ISBN END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.Title END DESC,
		         CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.Title END ASC,
		         CASE WHEN @orderBy = 3 AND @sortOrder = 0 THEN C.Author END DESC,
		         CASE WHEN @orderBy = 3 AND @sortOrder = 1 THEN C.Author END ASC,
                 CASE WHEN @orderBy = 4 AND @sortOrder = 0 THEN C.IsStolen END DESC,
		         CASE WHEN @orderBy = 4 AND @sortOrder = 1 THEN C.IsStolen END ASC,
                 CASE WHEN @orderBy = 5 AND @sortOrder = 0 THEN C.IsLost END DESC,
		         CASE WHEN @orderBy = 5 AND @sortOrder = 1 THEN C.IsLost END ASC,
                 CASE WHEN @orderBy = 6 AND @sortOrder = 0 THEN C.CopyNumber END DESC,
		         CASE WHEN @orderBy = 6 AND @sortOrder = 1 THEN C.CopyNumber END ASC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN C.ModifiedBy END DESC,
		         CASE WHEN @orderBy = 7 AND @sortOrder = 1 THEN C.ModifiedBy END ASC,
		         CASE WHEN @orderBy = 8 AND @sortOrder = 0 THEN C.DateModified END DESC,
		         CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN C.DateModified END ASC
                 LIMIT @pageSize OFFSET @pageOffset ");


            var pageOffset = (filterParameters.PageSize * (filterParameters.PageNum - 1));

            
            using (var connection = this.OpenConnection())
            {
                LibraryBooks = connection.Query<LibraryBookListItem>(sb.ToString(), new
                {
                    searchText = "%" + filterParameters.SearchText + "%",
                    pageOffset,
                    pageSize = filterParameters.PageSize,
                    orderBy = filterParameters.OrderBy,
                    sortOrder = filterParameters.SortOrder
                });
            }

            LibraryBookPageApiModel.Results = LibraryBooks.Select(x => new LibraryBookApiModel
            {
                LibraryBookCode = x.LibraryBookCode,
                ISBN = x.ISBN,
                Title = x.Title,
                Author = x.Author,
                IsStolen = x.IsStolen,
                IsLost = x.IsLost,
                CopyNumber = x.CopyNumber,
                CreatedBy = x.CreatedBy,
                DateCreated = x.DateCreated,
                ModifiedBy = x.ModifiedBy,
                DateModified = x.DateModified,

            }).ToList();

            if (LibraryBookPageApiModel.Results != null && LibraryBookPageApiModel.Results.Count() > 0)
            {
                searchResultCount = LibraryBooks.First().TotalRows;
                LibraryBookPageApiModel.SearchResultCount = searchResultCount;
            }

            return LibraryBookPageApiModel;
        }

        /// <summary>
        /// Insert LibraryBook
        /// </summary>
        /// <param name="LibraryBook"></param>
        /// <param name="LibraryBookCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        public virtual int InsertLibraryBook(LibraryBookApiModel LibraryBook, out string LibraryBookCode, TransactionParam transactionParam = null)
        {
            LibraryBookCode = _randomKeyGenerator.GetUniqueKey(9);
            LibraryBook.LibraryBookCode = LibraryBookCode;

            var parameters = new DynamicParameters();

            parameters.Add(name: "LibraryBookCode", value: LibraryBookCode);
            parameters.Add(name: "isbn", value: LibraryBook.ISBN);
            parameters.Add(name: "title", value: LibraryBook.Title);
            parameters.Add(name: "author", value: LibraryBook.Author);
            parameters.Add(name: "isStolen", value: LibraryBook.IsStolen);
            parameters.Add(name: "isLost", value: LibraryBook.IsLost);
            parameters.Add(name: "copyNumber", value: LibraryBook.CopyNumber);
            parameters.Add(name: "createdBy", value: LibraryBook.CreatedBy);
            //parameters.Add(name: "dateCreated", value: LibraryBook.DateCreated.ToString("yyyy-MM-dd H:mm:ss"));
            parameters.Add(name: "modifiedBy", value: LibraryBook.ModifiedBy);
            //parameters.Add(name: "dateModified", value: LibraryBook.DateModified.ToString("yyyy-MM-dd H:mm:ss"));

            int rowaffected = 0;

            string sql = @"INSERT INTO LibraryBook (
				LibraryBookCode, ISBN, Title, Author, IsStolen, IsLost, CopyNumber, CreatedBy, ModifiedBy)
			VALUES (@libraryBookCode, @isbn, @title, @author, @isStolen, @isLost, @copyNumber,
				@createdBy, @modifiedBy)";

            if (transactionParam != null)
            {
                rowaffected = transactionParam.Connection.Execute(
                    sql,
                    param: parameters,
                    commandType: CommandType.Text,
                    transaction: transactionParam.Transaction);
            }
            else
            {
                using (var connection = this.OpenConnection())
                {
                    rowaffected = connection.Execute(
                    sql,
                    param: parameters,
                    commandType: CommandType.Text);
                }
            }

            return rowaffected;
        }

        /// <summary>
        /// Updare LibraryBook
        /// </summary>
        /// <param name="LibraryBook"></param>
        /// <returns>The<see cref="int"/> </returns>
        public int UpdateLibraryBook(LibraryBookApiModel LibraryBook, TransactionParam transactionParam = null)
        {

            var parameters = new DynamicParameters();
            parameters.Add(name: "LibraryBookCode", value: LibraryBook.LibraryBookCode);
            parameters.Add(name: "isbn", value: LibraryBook.ISBN);
            parameters.Add(name: "title", value: LibraryBook.Title);
            parameters.Add(name: "author", value: LibraryBook.Author);
            parameters.Add(name: "isStolen", value: LibraryBook.IsStolen);
            parameters.Add(name: "isLost", value: LibraryBook.IsLost);
            parameters.Add(name: "copyNumber", value: LibraryBook.CopyNumber);
            parameters.Add(name: "modifiedBy", value: LibraryBook.ModifiedBy);
            parameters.Add(name: "modifiedDate", value: DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"));

            const string sql = @"UPDATE LibraryBook
			SET 
               Title = @title
              ,ISBN = @isbn
              ,Title = @title
              ,Author = @author
              ,IsStolen = @isStolen
              ,IsLost = @isLost
              ,CopyNumber = @copyNumber
              ,ModifiedBy = @modifiedBy
			  ,DateModified = @modifiedDate
			    WHERE LibraryBookCode = @LibraryBookCode";

            LibraryBook.DateModified = DateTime.Now;
            int rowaffected = 0;

            if (transactionParam != null)
            {
                rowaffected = transactionParam.Connection.Execute(
                    sql,
                    param: parameters,
                    commandType: CommandType.Text,
                    transaction: transactionParam.Transaction);
            }
            else
            {
                using (var connection = this.OpenConnection())
                {
                    rowaffected = connection.Execute(
                    sql,
                    param: parameters,
                    commandType: CommandType.Text);
                }
            }

            return rowaffected;

        }

        /// <summary>
        /// Delete LibraryBook
        /// </summary>
        /// <param name="LibraryBookCode"></param>
        /// <returns>The<see cref="int"/> </returns>
        public int DeleteLibraryBook(string LibraryBookCode)
        {
            int rowsAffected = 0;
            try
            {

                using (var connection = this.OpenConnection())
                {
                    rowsAffected = connection.Execute("DELETE FROM LibraryBook WHERE LibraryBookCode = @LibraryBookCode", new { LibraryBookCode });
                }

            }
            catch
            {
                rowsAffected = -1;
            }

            return rowsAffected;
        }
    }
}
