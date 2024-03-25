using Microsoft.AspNetCore.Authorization;

namespace LibraryApp.Api;

public static class Policies
{
    public const string Admin = "Administrator";
    public const string Librarian = "Librarian";
    public const string User = "User";

    public static AuthorizationPolicy AdminPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin).Build();
    }

    public static AuthorizationPolicy LibrarianPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Librarian).Build();
    }

    public static AuthorizationPolicy UserPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(User).Build();
    }
}

