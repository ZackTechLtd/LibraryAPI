using System;
using Common.Configuration;
using System.Data;
using Common.Models;
using Common.Models.Api;
using Common.Util;
using Dapper;
using LibraryApp.Infrastructure.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LibraryApp.Infrastructure.Repository;

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
    int InsertLibraryUser(LibraryUserApiModel LibraryUser, out string LibraryUserCode, TransactionParam transactionParam = default!);


    /// <summary>
    /// Updare LibraryUser
    /// </summary>
    /// <param name="LibraryUser"></param>
    /// <returns>The<see cref="int"/> </returns>
    int UpdateLibraryUser(LibraryUserApiModel LibraryUser, TransactionParam transactionParam = default!);


    /// <summary>
    /// Delete LibraryUser
    /// </summary>
    /// <param name="LibraryUserCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    int DeleteLibraryUser(string LibraryUserCode);

}

public class LibraryUserRepository : RepositoryBase, ILibraryUserRepository
{
    protected readonly IRandomKeyGenerator _randomKeyGenerator;
    protected readonly IOptions<ApiConfiguration> _apiConfiguration;

    /// <summary>
    /// Initializes a new instance of the<see cref="LibraryUserRepository"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="apiConfiguration"></param>
    /// <param name="randomKeyGenerator"></param>
    public LibraryUserRepository(IConfiguration config, IOptions<ApiConfiguration> apiConfiguration, IRandomKeyGenerator randomKeyGenerator) : base(config, apiConfiguration)
    {
        _apiConfiguration = apiConfiguration;
        _randomKeyGenerator = randomKeyGenerator;
    }


#pragma warning disable CS8600

    /// <summary>
    /// Get a LibraryUser by it's id
    /// </summary>
    /// <param name="LibraryUserCode"></param>
    /// <returns>The <see cref="LibraryUserApiModel"/> </returns>
    public LibraryUserApiModel GetLibraryUserByLibraryUserCode(string LibraryUserCode)
    {
        LibraryUserApiModel LibraryUserApiModel = default!;
        using (var connection = this.OpenConnection())
        {
            LibraryUserApiModel = connection.Query<LibraryUserApiModel>(@"SELECT C.LibraryUserCode, C.Title, C.Name, C.PhoneNumber, C.MobilePhoneNumber, C.Email, C.AlternativePhoneNumber, C.AlternativeEmail, 
                         C.AddressLine1, C.AddressLine2, C.AddressLine3, C.City, C.County, C.Country, C.Postcode, 
                         C.GDPRInformedDate,C.GDPRInformedBy,C.GDPRHowInformed,C.GDPRNotes,C.LibraryUserByPost,C.LibraryUserByPostConsentDate,C.LibraryUserByEmail,
                         C.LibraryUserByEmailConsentDate,C.LibraryUserByPhone,C.LibraryUserByPhoneConsentDate,C.LibraryUserBySMS,C.LibraryUserBySMSConsentDate,
                         C.DateCreated, C.CreatedBy, C.DateModified, C.ModifiedBy
                         FROM LibraryUser C
	                     WHERE C.LibraryUserCode = @LibraryUserCode", new { LibraryUserCode }).SingleOrDefault();
        }

        return LibraryUserApiModel ?? default!;

    }
#pragma warning restore CS8600

    public ApiItemCollectionApiModel GetLibraryUsers(string search)
    {
        ApiItemCollectionApiModel apiItemCollectionApiModel = new ApiItemCollectionApiModel();

        using (var connection = this.OpenConnection())
        {
            apiItemCollectionApiModel.Results = connection.Query<ApiItem>(@"SELECT C.LibraryUserCode As Code, CONCAT(C.Title, ' ', C.Name) As Name
                         FROM LibraryUser C
	                     WHERE C.Name LIKE @search LIMIT 20", new { search });
        }

        return apiItemCollectionApiModel;

    }


    /// <summary>
    /// GetLibraryUsersPaged for a sales location
    /// </summary>
    /// <param name="filterParameters"></param>
    /// <param name="searchResultCount"></param>
    /// <returns>The <see cref="LibraryUserPageApiModel"/> </returns>
    public virtual LibraryUserPageApiModel GetLibraryUsersPaged(PagedBase filterParameters, out int searchResultCount)
    {
        LibraryUserPageApiModel LibraryUserPageApiModel = new LibraryUserPageApiModel();
        searchResultCount = 0;
        IEnumerable<LibraryUserListItem> LibraryUsers = default!;

        string sql = @"SELECT C.LibraryUserCode, C.Title, C.Name, C.PhoneNumber, C.MobilePhoneNumber, C.Email,  C.AlternativePhoneNumber, C.AlternativeEmail,  C.AddressLine1, C.AddressLine2, C.AddressLine3, C.City, C.County, C.Country, C.Postcode, C.ModifiedBy, C.DateModified,
            ( SELECT COUNT(*) FROM LibraryUser) AS 'TotalRows' FROM LibraryUser C
	        WHERE 
		        (
                    CONCAT(C.Title, ' ', C.Name) LIKE @searchText
			        OR C.Name LIKE @searchText
                    OR C.AddressLine1 LIKE @searchText
                    OR C.City LIKE @searchText
                    OR C.County LIKE @searchText
                    OR C.PostCode LIKE @searchText
                    OR C.PhoneNumber LIKE @searchText
                    OR C.MobilePhoneNumber LIKE @searchText
                    OR C.Email LIKE @searchText
                    OR C.ModifiedBy LIKE @searchText
		            OR C.DateModified LIKE @searchText
		        )
            order by
                CASE WHEN @orderBy = 0 THEN C.Name END ASC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 0 THEN C.Name END DESC,
		         CASE WHEN @orderBy = 1 AND @sortOrder = 1 THEN C.Name END ASC,

                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.AddressLine1 END DESC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.AddressLine2 END DESC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.AddressLine3 END DESC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.City END DESC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.County END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 0 THEN C.PostCode END DESC,

                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.AddressLine1 END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.AddressLine2 END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.AddressLine3 END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.City END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.County END ASC,
                 CASE WHEN @orderBy = 2 AND @sortOrder = 1 THEN C.PostCode END ASC,


		         CASE WHEN @orderBy = 3 AND @sortOrder = 0 THEN C.PhoneNumber END DESC,
		         CASE WHEN @orderBy = 3 AND @sortOrder = 1 THEN C.PhoneNumber END ASC,
                 CASE WHEN @orderBy = 4 AND @sortOrder = 0 THEN C.MobilePhoneNumber END DESC,
		         CASE WHEN @orderBy = 4 AND @sortOrder = 1 THEN C.MobilePhoneNumber END ASC,
                 CASE WHEN @orderBy = 5 AND @sortOrder = 0 THEN C.Email END DESC,
		         CASE WHEN @orderBy = 5 AND @sortOrder = 1 THEN C.Email END ASC,
                 CASE WHEN @orderBy = 6 AND @sortOrder = 0 THEN C.ModifiedBy END DESC,
		         CASE WHEN @orderBy = 6 AND @sortOrder = 1 THEN C.ModifiedBy END ASC,
		         CASE WHEN @orderBy = 7 AND @sortOrder = 0 THEN C.DateModified END DESC,
		         CASE WHEN @orderBy = 7 AND @sortOrder = 1 THEN C.DateModified END ASC 
                 LIMIT @pageSize OFFSET @pageOffset";


        var pageOffset = (filterParameters.PageSize * (filterParameters.PageNum - 1));


        using (var connection = this.OpenConnection())
        {
            LibraryUsers = connection.Query<LibraryUserListItem>(sql, new
            {
                searchText = "%" + filterParameters.SearchText + "%",
                pageOffset,
                pageSize = filterParameters.PageSize,
                orderBy = filterParameters.OrderBy,
                sortOrder = filterParameters.SortOrder
            });
        }



        LibraryUserPageApiModel.Results = LibraryUsers.Select(x => new LibraryUserApiModel
        {
            LibraryUserCode = x.LibraryUserCode,
            Title = x.Title,
            Name = x.Name,
            AddressLine1 = x.AddressLine1,
            AddressLine2 = x.AddressLine2,
            AddressLine3 = x.AddressLine3,
            City = x.City,
            County = x.County,
            Country = x.Country,
            Postcode = x.Postcode,
            PhoneNumber = x.PhoneNumber,
            MobilePhoneNumber = x.MobilePhoneNumber,
            Email = x.Email,
            AlternativePhoneNumber = x.AlternativePhoneNumber,
            AlternativeEmail = x.AlternativeEmail,
            ModifiedBy = x.ModifiedBy,
            DateModified = x.DateModified,

        }).ToList();

        if (LibraryUserPageApiModel.Results != null && LibraryUserPageApiModel.Results.Count() > 0)
        {
            searchResultCount = LibraryUsers.First().TotalRows;
            LibraryUserPageApiModel.SearchResultCount = searchResultCount;
        }

        return LibraryUserPageApiModel;
    }

    /// <summary>
    /// Insert LibraryUser
    /// </summary>
    /// <param name="LibraryUser"></param>
    /// <param name="LibraryUserCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public virtual int InsertLibraryUser(LibraryUserApiModel LibraryUser, out string LibraryUserCode, TransactionParam transactionParam = default!)
    {
        LibraryUserCode = _randomKeyGenerator.GetUniqueKey(9);
        LibraryUser.LibraryUserCode = LibraryUserCode;

        var parameters = new DynamicParameters();

        parameters.Add(name: "LibraryUserCode", value: LibraryUserCode);
        parameters.Add(name: "title", value: LibraryUser.Title);
        parameters.Add(name: "name", value: LibraryUser.Name);
        parameters.Add(name: "phoneNumber", value: LibraryUser.PhoneNumber);
        parameters.Add(name: "mobilePhoneNumber", value: LibraryUser.MobilePhoneNumber);
        parameters.Add(name: "email", value: LibraryUser.Email);

        parameters.Add(name: "alternativePhoneNumber", value: LibraryUser.AlternativePhoneNumber);
        parameters.Add(name: "alternativeEmail", value: LibraryUser.AlternativeEmail);

        parameters.Add(name: "addressLine1", value: LibraryUser.AddressLine1);
        parameters.Add(name: "addressLine2", value: LibraryUser.AddressLine2);
        parameters.Add(name: "addressLine3", value: LibraryUser.AddressLine3);
        parameters.Add(name: "city", value: LibraryUser.City);
        parameters.Add(name: "county", value: LibraryUser.County);
        parameters.Add(name: "country", value: LibraryUser.Country);
        parameters.Add(name: "postcode", value: LibraryUser.Postcode);

        parameters.Add(name: "GDPRInformedDate", value: LibraryUser.GDPRInformedDate);
        parameters.Add(name: "GDPRInformedBy", value: LibraryUser.GDPRInformedBy);
        parameters.Add(name: "GDPRHowInformed", value: LibraryUser.GDPRHowInformed);
        parameters.Add(name: "GDPRNotes", value: LibraryUser.GDPRNotes);
        parameters.Add(name: "LibraryUserByPost", value: LibraryUser.LibraryUserByPost);
        parameters.Add(name: "LibraryUserByPostConsentDate", value: LibraryUser.LibraryUserByPostConsentDate);
        parameters.Add(name: "LibraryUserByEmail", value: LibraryUser.LibraryUserByEmail);
        parameters.Add(name: "LibraryUserByEmailConsentDate", value: LibraryUser.LibraryUserByEmailConsentDate);
        parameters.Add(name: "LibraryUserByPhone", value: LibraryUser.LibraryUserByPhone);
        parameters.Add(name: "LibraryUserByPhoneConsentDate", value: LibraryUser.LibraryUserByPhoneConsentDate);
        parameters.Add(name: "LibraryUserBySMS", value: LibraryUser.LibraryUserBySMS);
        parameters.Add(name: "LibraryUserBySMSConsentDate", value: LibraryUser.LibraryUserBySMSConsentDate);

        parameters.Add(name: "createdBy", value: LibraryUser.CreatedBy);
        parameters.Add(name: "dateCreated", value: LibraryUser.DateCreated);
        parameters.Add(name: "modifiedBy", value: LibraryUser.ModifiedBy);
        parameters.Add(name: "dateModified", value: LibraryUser.DateModified);

        int rowaffected = 0;

        string sql = @"INSERT INTO LibraryUser (
				LibraryUserCode,Title,Name,PhoneNumber,MobilePhoneNumber,Email, AlternativePhoneNumber, AlternativeEmail,
                AddressLine1, AddressLine2, AddressLine3, City, County, Country, Postcode,
                GDPRInformedDate,GDPRInformedBy,GDPRHowInformed,GDPRNotes,LibraryUserByPost,LibraryUserByPostConsentDate,LibraryUserByEmail,
                LibraryUserByEmailConsentDate,LibraryUserByPhone,LibraryUserByPhoneConsentDate,LibraryUserBySMS,LibraryUserBySMSConsentDate,
                CreatedBy, DateCreated, ModifiedBy, DateModified
				)
			VALUES(
				@LibraryUserCode, @title, @name, @phoneNumber, @mobilePhoneNumber, @email, @alternativePhoneNumber, @alternativeEmail,
                @addressLine1, @addressLine2, @addressLine3, @city, @county, @country, @postcode,
                @GDPRInformedDate,@GDPRInformedBy,@GDPRHowInformed,@GDPRNotes,@LibraryUserByPost,@LibraryUserByPostConsentDate,@LibraryUserByEmail,
                @LibraryUserByEmailConsentDate,@LibraryUserByPhone,@LibraryUserByPhoneConsentDate,@LibraryUserBySMS,@LibraryUserBySMSConsentDate,
				@createdBy, NOW(), @modifiedBy, NOW())";


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
    /// Updare LibraryUser
    /// </summary>
    /// <param name="LibraryUser"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int UpdateLibraryUser(LibraryUserApiModel LibraryUser, TransactionParam transactionParam = default!)
    {

        var parameters = new DynamicParameters();
        parameters.Add(name: "LibraryUserCode", value: LibraryUser.LibraryUserCode);
        parameters.Add(name: "title", value: LibraryUser.Title);
        parameters.Add(name: "name", value: LibraryUser.Name);
        parameters.Add(name: "phoneNumber", value: LibraryUser.PhoneNumber);
        parameters.Add(name: "mobilePhoneNumber", value: LibraryUser.MobilePhoneNumber);
        parameters.Add(name: "email", value: LibraryUser.Email);
        parameters.Add(name: "alternativePhoneNumber", value: LibraryUser.AlternativePhoneNumber);
        parameters.Add(name: "alternativeEmail", value: LibraryUser.AlternativeEmail);
        parameters.Add(name: "addressLine1", value: LibraryUser.AddressLine1);
        parameters.Add(name: "addressLine2", value: LibraryUser.AddressLine2);
        parameters.Add(name: "addressLine3", value: LibraryUser.AddressLine3);
        parameters.Add(name: "city", value: LibraryUser.City);
        parameters.Add(name: "county", value: LibraryUser.County);
        parameters.Add(name: "country", value: LibraryUser.Country);
        parameters.Add(name: "postcode", value: LibraryUser.Postcode);
        parameters.Add(name: "GDPRInformedDate", value: LibraryUser.GDPRInformedDate);
        parameters.Add(name: "GDPRInformedBy", value: LibraryUser.GDPRInformedBy);
        parameters.Add(name: "GDPRHowInformed", value: LibraryUser.GDPRHowInformed);
        parameters.Add(name: "GDPRNotes", value: LibraryUser.GDPRNotes);
        parameters.Add(name: "LibraryUserByPost", value: LibraryUser.LibraryUserByPost);
        parameters.Add(name: "LibraryUserByPostConsentDate", value: LibraryUser.LibraryUserByPostConsentDate);
        parameters.Add(name: "LibraryUserByEmail", value: LibraryUser.LibraryUserByEmail);
        parameters.Add(name: "LibraryUserByEmailConsentDate", value: LibraryUser.LibraryUserByEmailConsentDate);
        parameters.Add(name: "LibraryUserByPhone", value: LibraryUser.LibraryUserByPhone);
        parameters.Add(name: "LibraryUserByPhoneConsentDate", value: LibraryUser.LibraryUserByPhoneConsentDate);
        parameters.Add(name: "LibraryUserBySMS", value: LibraryUser.LibraryUserBySMS);
        parameters.Add(name: "LibraryUserBySMSConsentDate", value: LibraryUser.LibraryUserBySMSConsentDate);
        parameters.Add(name: "modifiedBy", value: LibraryUser.ModifiedBy);
        parameters.Add(name: "modifiedDate", value: DateTime.Now);

        const string sql = @"UPDATE LibraryUser
			SET 
               Title = @title
              ,Name = @name
              ,PhoneNumber = @phoneNumber
              ,MobilePhoneNumber = @mobilePhoneNumber
              ,Email = @email
              ,AlternativePhoneNumber = @alternativePhoneNumber
              ,AlternativeEmail = @alternativeEmail
              ,AddressLine1 = @addressLine1
              ,AddressLine2 = @addressLine2
              ,AddressLine3 = @addressLine3
              ,City = @city
              ,County = @county
              ,Country = @country
              ,Postcode = @postcode
              ,GDPRInformedDate = @GDPRInformedDate
              ,GDPRInformedBy = @GDPRInformedBy
              ,GDPRHowInformed = @GDPRHowInformed
              ,GDPRNotes = @GDPRNotes
              ,LibraryUserByPost = @LibraryUserByPost
              ,LibraryUserByPostConsentDate = @LibraryUserByPostConsentDate
              ,LibraryUserByEmail = @LibraryUserByEmail
              ,LibraryUserByEmailConsentDate = @LibraryUserByEmailConsentDate
              ,LibraryUserByPhone = @LibraryUserByPhone
              ,LibraryUserByPhoneConsentDate = @LibraryUserByPhoneConsentDate
              ,LibraryUserBySMS = @LibraryUserBySMS
              ,LibraryUserBySMSConsentDate = @LibraryUserBySMSConsentDate
              ,ModifiedBy = @modifiedBy
			  ,DateModified = @modifiedDate
			    WHERE LibraryUserCode = @LibraryUserCode";

        LibraryUser.DateModified = DateTime.Now;
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
    /// Delete LibraryUser
    /// </summary>
    /// <param name="LibraryUserCode"></param>
    /// <returns>The<see cref="int"/> </returns>
    public int DeleteLibraryUser(string LibraryUserCode)
    {
        int rowsAffected = 0;
        try
        {

            using (var connection = this.OpenConnection())
            {
                rowsAffected = connection.Execute("DELETE FROM LibraryUser WHERE LibraryUserCode = @LibraryUserCode", new { LibraryUserCode });
            }

        }
        catch
        {
            rowsAffected = -1;
        }

        return rowsAffected;
    }
}

