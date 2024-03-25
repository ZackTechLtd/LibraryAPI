using System;
using Common.Configuration;
using Common.Models;
using Common.Models.Api;
using Common.Util;
using Dapper;
using LibraryApp.Infrastructure.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;

namespace LibraryApp.Infrastructure.Repository;

public interface ILibraryBookStatusRepository
{
    int? GetCountOfBookCurrentLent(string libraryUserCode);

    bool HasMoreThanOneBookWithSameISBN(string isbn, string libraryUserCode);


    /// <summary>
    /// Get a LibraryBookStatus by it's id
    /// </summary>
    /// <param name="LibraryBookStatusCode"></param>
    /// <returns>The <see cref="LibraryBookStatusApiModel"/> </returns>
    LibraryBookStatusApiModel GetLibraryBookStatusByLibraryBookStatusCode(string libraryBookStatusCode);




    /// <summary>
    /// GetLibraryBookStatussPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="userName"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryBookStatusPageApiModel"/> </returns>
    LibraryBookStatusPageApiModel GetLibraryBookStatusPaged(PagedBase filterParameters, out int searchResultCount);


    /// <summary>
    /// Insert LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatus"></param>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    int InsertLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, out string libraryBookStatusCode, TransactionParam transactionParam = default!);


    /// <summary>
    /// Updare LibraryBookStatus
    /// </summary>
    /// <param name="LibraryBookStatus"></param>
    /// <returns>The<see cref="int"/> </returns>
    int UpdateLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, TransactionParam transactionParam = default!);

    /// <summary>
    /// Delete LibraryBookStatus
    /// </summary>
    /// <param name="LibraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    int DeleteLibraryBookStatus(string LibraryBookStatusCode);

}

public class LibraryBookStatusRepository : RepositoryBase, ILibraryBookStatusRepository
{
    protected readonly IRandomKeyGenerator _randomKeyGenerator;
    protected readonly IOptions<ApiConfiguration> _apiConfiguration;

    /// <summary>
    /// Initializes a new instance of the<see cref="LibraryBookStatusRepository"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="apiConfiguration"></param>
    /// <param name="randomKeyGenerator"></param>
    public LibraryBookStatusRepository(IConfiguration config, IOptions<ApiConfiguration> apiConfiguration, IRandomKeyGenerator randomKeyGenerator) : base(config, apiConfiguration)
    {
        _apiConfiguration = apiConfiguration;
        _randomKeyGenerator = randomKeyGenerator;
    }

    public int? GetCountOfBookCurrentLent(string libraryUserCode)
    {
        using (var connection = this.OpenConnection())
        {
            return connection.Query<int>(@"SELECT Count(*) FROM LibraryBookStatus LBS
                INNER JOIN LibraryUser LU ON LU.LibraryUserId = LBS.LibraryBookUserId
                WHERE LU.LibraryUserCode = @libraryUserCode AND LBS.DateReturned IS NULL", new { libraryUserCode }).SingleOrDefault();
        }

    }

    public bool HasMoreThanOneBookWithSameISBN(string isbn, string libraryUserCode)
    {
        using (var connection = this.OpenConnection())
        {

            int? retVal = connection.Query<int>(@"SELECT Count(*) FROM LibraryBookStatus LBS
                INNER JOIN LibraryUser LU ON LU.LibraryUserId = LBS.LibraryBookUserId
                INNER JOIN LibraryBook LB ON LB.LibraryBookId = LBS.LibraryBookId
                WHERE LU.LibraryUserCode = @libraryUserCode AND LBS.DateReturned IS NULL AND LB.ISBN = @isbn", new { isbn, libraryUserCode }).SingleOrDefault();

            if (retVal != null && retVal.Value > 0)
                return true;
            else
                return false;

        }

    }

#pragma warning disable CS8600 
    /// <summary>
    /// Get a LibraryBookStatus by it's id
    /// </summary>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The <see cref="LibraryBookStatusApiModel"/> </returns>
    public LibraryBookStatusApiModel GetLibraryBookStatusByLibraryBookStatusCode(string libraryBookStatusCode)
    {
        var parameters = new { libraryBookStatusCode };
        LibraryBookStatusApiModel libraryBookStatusApiModel = default!;
        using (var connection = this.OpenConnection())
        {
            libraryBookStatusApiModel = connection.Query<LibraryBookStatusApiModel, LibraryBookApiModel, LibraryUserApiModel, LibraryBookStatusApiModel>(
                @"SELECT C.LibraryBookStatusCode, C.DateCheckedOut, C.DateReturned, 
                         C.DateCreated, C.CreatedBy, C.DateModified, C.ModifiedBy,
                         LB.LibraryBookCode, LB.ISBN, LB.Title, LB.Author, LB.IsStolen, LB.IsLost, LB.CopyNumber,
                         LB.DateCreated, LB.CreatedBy, LB.DateModified, LB.ModifiedBy,
                         LU.LibraryUserCode, LU.Title, LU.Name, LU.PhoneNumber, LU.MobilePhoneNumber, LU.Email, LU.AlternativePhoneNumber, LU.AlternativeEmail, 
                         LU.AddressLine1, LU.AddressLine2, LU.AddressLine3, LU.City, LU.County, LU.Country, LU.Postcode, 
                         LU.GDPRInformedDate,LU.GDPRInformedBy,LU.GDPRHowInformed,LU.GDPRNotes,LU.LibraryUserByPost,LU.LibraryUserByPostConsentDate,LU.LibraryUserByEmail,
                         LU.LibraryUserByEmailConsentDate,LU.LibraryUserByPhone,LU.LibraryUserByPhoneConsentDate,LU.LibraryUserBySMS,LU.LibraryUserBySMSConsentDate,
                         LU.DateCreated, LU.CreatedBy, LU.DateModified, LU.ModifiedBy
                         FROM LibraryBookStatus C
                         INNER JOIN LibraryBook LB ON LB.LibraryBookId = c.LibraryBookId
                         INNER JOIN LibraryUser LU ON LU.LibraryUserId = C.LibraryBookUserId
	                     WHERE C.LibraryBookStatusCode = @LibraryBookStatusCode",
                    (lbs, lb, lu) =>
                    {
                        lbs.LibraryBook = lb;
                        lbs.LibraryUser = lu;
                        return lbs;
                    },
                    splitOn: "LibraryBookCode,LibraryUserCode",
                    param: parameters,
                    commandType: CommandType.Text).SingleOrDefault();
        }

        return libraryBookStatusApiModel ?? default!;

    }

#pragma warning restore CS8600



    /// <summary>
    /// GetLibraryBookStatussPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryBookStatusPageApiModel"/> </returns>
    public virtual LibraryBookStatusPageApiModel GetLibraryBookStatusPaged(PagedBase filterParameters, out int searchResultCount)
    {
        LibraryBookStatusPageApiModel LibraryBookStatusPageApiModel = new LibraryBookStatusPageApiModel();
        searchResultCount = 0;
        IEnumerable<LibraryBookStatusListItem> LibraryBookStatuss = default!;

        StringBuilder sb = new StringBuilder();
        sb.Append(@"SELECT C.LibraryBookStatusCode, C.DateCheckedOut, C.DateReturned, 
                         C.DateCreated, C.CreatedBy, C.DateModified, C.ModifiedBy,
                         LB.LibraryBookCode, LB.ISBN, LB.Title, LB.Author, LB.IsStolen, LB.IsLost, LB.CopyNumber,
                         LB.DateCreated, LB.CreatedBy, LB.DateModified, LB.ModifiedBy,
                         LU.LibraryUserCode, LU.Title, LU.Name, LU.PhoneNumber, LU.MobilePhoneNumber, LU.Email, LU.AlternativePhoneNumber, LU.AlternativeEmail, 
                         LU.AddressLine1, LU.AddressLine2, LU.AddressLine3, LU.City, LU.County, LU.Country, LU.Postcode, 
                         LU.GDPRInformedDate,LU.GDPRInformedBy,LU.GDPRHowInformed,LU.GDPRNotes,LU.LibraryUserByPost,LU.LibraryUserByPostConsentDate,LU.LibraryUserByEmail,
                         LU.LibraryUserByEmailConsentDate,LU.LibraryUserByPhone,LU.LibraryUserByPhoneConsentDate,LU.LibraryUserBySMS,LU.LibraryUserBySMSConsentDate,
                         LU.DateCreated, LU.CreatedBy, LU.DateModified, LU.ModifiedBy,
                         
            ( SELECT COUNT(*) FROM LibraryBookStatus ) AS 'TotalRows' FROM LibraryBookStatus C
            INNER JOIN LibraryBook LB ON LB.LibraryBookId = C.LibraryBookId
            INNER JOIN LibraryUser LU ON LU.LibraryUserId = C.LibraryBookUserId
	        WHERE
		        (
                    
			        C.DateCheckedOut LIKE @searchText
                    OR C.DateReturned LIKE @searchText
                    OR LB.ISBN LIKE @searchText
                    OR LB.Title LIKE @searchText
                    OR LB.CopyNumber LIKE @searchText
                    OR LU.Name LIKE @searchText
                    OR LU.AddressLine1 LIKE @searchText
                    OR LU.Postcode  LIKE @searchText
                    OR C.ModifiedBy LIKE @searchText
		            OR C.DateModified LIKE @searchText
		        )
            order by
                CASE WHEN @orderBy = 0 THEN C.DateCheckedOut END ASC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN C.DateCheckedOut END DESC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN C.DateCheckedOut END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.DateReturned END DESC,
		         CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.DateReturned END ASC,
		         CASE WHEN @orderBy = 3 AND @sortOrder = 0 THEN LB.ISBN END DESC,
		         CASE WHEN @orderBy = 3 AND @sortOrder = 1 THEN LB.ISBN END ASC,
                 CASE WHEN @orderBy = 4 AND @sortOrder = 0 THEN LB.Title END DESC,
		         CASE WHEN @orderBy = 4 AND @sortOrder = 1 THEN LB.Title END ASC,
                 CASE WHEN @orderBy = 5 AND @sortOrder = 0 THEN LB.CopyNumber END DESC,
		         CASE WHEN @orderBy = 5 AND @sortOrder = 1 THEN LB.CopyNumber END ASC,
                 CASE WHEN @orderBy = 6 AND @sortOrder = 0 THEN LU.Name END DESC,
		         CASE WHEN @orderBy = 6 AND @sortOrder = 1 THEN LU.Name END ASC,

                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.AddressLine1 END DESC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.AddressLine2 END DESC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.AddressLine3 END DESC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.City END DESC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.County END ASC,
                 CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN LU.PostCode END DESC,

                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.AddressLine1 END ASC,
                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.AddressLine2 END ASC,
                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.AddressLine3 END ASC,
                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.City END ASC,
                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.County END ASC,
                 CASE WHEN @orderBy = 8 AND @sortOrder = 1 THEN LU.PostCode END ASC,
  
		         CASE WHEN @orderBy = 9 AND @sortOrder = 0 THEN C.DateModified END DESC,
		         CASE WHEN @orderBy = 9 AND @sortOrder = 1 THEN C.DateModified END ASC 
                 LIMIT @pageSize OFFSET @pageOffset");


        var pageOffset = (filterParameters.PageSize * (filterParameters.PageNum - 1));

        var parameters = new
        {
            searchText = "%" + filterParameters.SearchText + "%",
            pageOffset,
            pageSize = filterParameters.PageSize,
            orderBy = filterParameters.OrderBy,
            sortOrder = filterParameters.SortOrder
        };

        try
        {
            using (var connection = this.OpenConnection())
            {
                LibraryBookStatuss = connection.Query<LibraryBookStatusListItem, LibraryBookApiModel, LibraryUserApiModel, LibraryBookStatusListItem>(sb.ToString(),
                    (lbs, lb, lu) =>
                    {
                        lbs.LibraryBook = lb;
                        lbs.LibraryUser = lu;
                        return lbs;
                    },
                    splitOn: "LibraryBookCode,LibraryUserCode",
                        param: parameters,
                        commandType: CommandType.Text);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }




        LibraryBookStatusPageApiModel.Results = LibraryBookStatuss.Select(x => new LibraryBookStatusApiModel
        {
            LibraryBookStatusCode = x.LibraryBookStatusCode,
            DateCheckedOut = x.DateCheckedOut,
            DateReturned = x.DateReturned,
            LibraryBook = x.LibraryBook,
            LibraryUser = x.LibraryUser,
            CreatedBy = x.CreatedBy,
            DateCreated = x.DateCreated,
            ModifiedBy = x.ModifiedBy,
            DateModified = x.DateModified,

        }).ToList();

        if (LibraryBookStatusPageApiModel.Results != null && LibraryBookStatusPageApiModel.Results.Count() > 0)
        {
            searchResultCount = LibraryBookStatuss.First().TotalRows;
            LibraryBookStatusPageApiModel.SearchResultCount = searchResultCount;
        }

        return LibraryBookStatusPageApiModel;
    }

    /// <summary>
    /// Insert LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatus"></param>
    /// <param name="libraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public virtual int InsertLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, out string libraryBookStatusCode, TransactionParam transactionParam = default!)
    {
        libraryBookStatusCode = _randomKeyGenerator.GetUniqueKey(9);
        libraryBookStatus.LibraryBookStatusCode = libraryBookStatusCode;

        var parameters = new DynamicParameters();

        parameters.Add(name: "libraryBookStatusCode", value: libraryBookStatusCode);
        parameters.Add(name: "dateCheckedOut", value: libraryBookStatus.DateCheckedOut);
        parameters.Add(name: "dateReturned", value: libraryBookStatus.DateReturned);
        parameters.Add(name: "libraryUserCode", value: libraryBookStatus.LibraryUser.LibraryUserCode);
        parameters.Add(name: "createdBy", value: libraryBookStatus.CreatedBy);
        //parameters.Add(name: "dateCreated", value: libraryBookStatus.DateCreated);
        parameters.Add(name: "modifiedBy", value: libraryBookStatus.ModifiedBy);
        //parameters.Add(name: "dateModified", value: libraryBookStatus.DateModified);

        int rowaffected = 0;



        string sql = @"INSERT INTO LibraryBookStatus (
				LibraryBookStatusCode, DateCheckedOut, DateReturned, DateCreated, CreatedBy, DateModified, ModifiedBy,
                LibraryBookId, LibraryBookUserId
				)
			SELECT @libraryBookStatusCode, @dateCheckedOut, @dateReturned, NOW(), @createdBy, NOW(), @modifiedBy, 
				@libraryBookId, LU.LibraryUserId
                FROM LibraryUser LU
                WHERE LU.LibraryUserCode = @libraryUserCode";


        if (transactionParam != null)
        {
            int bookId = transactionParam.Connection.Query<int>(@"SELECT LibraryBookId FROM LibraryBook WHERE LibraryBookCode =  @libraryBookCode",
                new { libraryBookStatus.LibraryBook.LibraryBookCode }
                ).SingleOrDefault();

            parameters.Add(name: "@libraryBookId", value: bookId);

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

                int bookId = connection.Query<int>(@"SELECT LibraryBookId FROM LibraryBook WHERE LibraryBookCode =  @libraryBookCode",
                new { libraryBookStatus.LibraryBook.LibraryBookCode }
                ).SingleOrDefault();

                parameters.Add(name: "@libraryBookId", value: bookId);

                rowaffected = connection.Execute(
                sql,
                param: parameters,
                commandType: CommandType.Text);
            }
        }

        return rowaffected;
    }

    /// <summary>
    /// Updare LibraryBookStatus
    /// </summary>
    /// <param name="libraryBookStatus"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int UpdateLibraryBookStatus(LibraryBookStatusApiModel libraryBookStatus, TransactionParam transactionParam = default!)
    {

        var parameters = new DynamicParameters();
        parameters.Add(name: "LibraryBookStatusCode", value: libraryBookStatus.LibraryBookStatusCode);
        parameters.Add(name: "dateCheckedOut", value: libraryBookStatus.DateCheckedOut);
        parameters.Add(name: "dateReturned", value: libraryBookStatus.DateReturned);
        parameters.Add(name: "modifiedBy", value: libraryBookStatus.ModifiedBy);
        parameters.Add(name: "modifiedDate", value: DateTime.Now);

        const string sql = @"UPDATE LibraryBookStatus
			SET 
               DateCheckedOut = @dateCheckedOut
              ,DateReturned = @dateReturned
              
              ,ModifiedBy = @modifiedBy
			  ,DateModified = @modifiedDate
			    WHERE LibraryBookStatusCode = @LibraryBookStatusCode";

        libraryBookStatus.DateModified = DateTime.Now;
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
    /// Delete LibraryBookStatus
    /// </summary>
    /// <param name="LibraryBookStatusCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int DeleteLibraryBookStatus(string LibraryBookStatusCode)
    {
        int rowsAffected = 0;
        try
        {

            using (var connection = this.OpenConnection())
            {
                rowsAffected = connection.Execute("DELETE FROM LibraryBookStatus WHERE LibraryBookStatusCode = @LibraryBookStatusCode", new { LibraryBookStatusCode });
            }

        }
        catch
        {
            rowsAffected = -1;
        }

        return rowsAffected;
    }
}

