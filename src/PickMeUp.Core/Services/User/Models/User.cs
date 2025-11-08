namespace PickMeUp.Core.Services.User;

public class User
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User first name.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// User last name.
    /// </summary>
    public string LastName { get; set; } = default!;
}
