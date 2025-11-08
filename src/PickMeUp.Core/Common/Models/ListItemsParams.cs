namespace PickMeUp.Core.Common.Models;

public class ListItemsParams : ParamsBase
{
    /// <summary>
    /// Number of items to skip (for pagination/virtual scroll).
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Number of items to take per request.
    /// </summary>
    public int Take { get; set; } = 20;

    /// <summary>
    /// Optional filter criteria.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field name.
    /// </summary>
    public int? OrderBy { get; set; }

    /// <summary>
    /// Sort direction.
    /// </summary>
    public bool OrderByDescending { get; set; } = false;
}
