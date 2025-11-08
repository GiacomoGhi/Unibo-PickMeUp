using System.Threading.Tasks;

namespace PickMeUp.Web.SignalR;

public interface IPublishDomainEvents
{
    Task Publish(object evnt);
}
