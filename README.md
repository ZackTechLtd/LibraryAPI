# LibraryAPI
This is a sample Net Core API

It uses JWT Token Authorization

Currently it is configured to use MariaDB (MySQL) but has an 
abstracted Data Access Layer that can be reconfigured for other databases.
Access to the data access layer is provided via dependency injection.

Authentication uses the built in Entity Framework System.

To access the other tables the lightweight ORM Dapper is used.

SOLID Principals have been used when creating the API.

