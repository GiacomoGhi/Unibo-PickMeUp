namespace PickMeUp.Core.Common.Models;

public class EditEntityResult<TIdentifier> where TIdentifier : struct
{
    /// <summary>
    /// Identifier of the edited entity.
    /// </summary>
    public TIdentifier EntityId { get; set; }
}