using EventHub.Web.Data;
using EventHub.Web.Interfaces;
using EventHub.Web.Models;

namespace EventHub.Web.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public IRepository<Event> Events { get; } = new Repository<Event>(context);
    public IRepository<Category> Categories { get; } = new Repository<Category>(context);
    public IRepository<Registration> Registrations { get; } = new Repository<Registration>(context);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
