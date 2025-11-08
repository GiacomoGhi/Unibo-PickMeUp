namespace PickMeUp.Core.Services.Email.Templates;

internal class PickUpRequestReceivedViewModel
{
    public required string TravelOwnerFirstName { get; set; }
    public required string RequesterFirstName { get; set; }
    public required string RequesterLastName { get; set; }
    public required string DepartureAddress { get; set; }
    public required string DestinationAddress { get; set; }
    public required string PickUpPointAddress { get; set; }
}
