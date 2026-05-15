using EventHub.Web.Models;

namespace EventHub.Web.Interfaces;

public interface IUnitOfWork
{
    IRepository<Event> Events { get; }
    IRepository<Category> Categories { get; }
    IRepository<Registration> Registrations { get; }
    Task<int> SaveChangesAsync();
}
