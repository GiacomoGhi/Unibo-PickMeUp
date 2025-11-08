namespace PickMeUp.Core.Common.Models;

public class DeleteEntityParams<T> : ParamsBase where T : struct
{
    /// <summary>
    /// Entity to be deleted identifier.
    /// </summary>
    public T EntityId { get; set; }
}
