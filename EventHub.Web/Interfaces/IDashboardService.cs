using EventHub.Web.ViewModels;

namespace EventHub.Web.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
