namespace LibraryApp.Infrastructure.Identity;

public class PreviousPassword
{

    /// <summary>
    /// PaswordHash
    /// </summary>
    //[Key, Column(Order = 0)]
    public string PasswordHash { get; set; } = default!;

    /// <summary>
    /// CreateDate
    /// </summary>
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// UserId
    /// </summary>
    //[Key, Column(Order = 1)]
    public string UserId { get; set; } = default!;

    /// <summary>
    /// User
    /// </summary>
    public virtual ApplicationUser User { get; set; } = default!;

}
