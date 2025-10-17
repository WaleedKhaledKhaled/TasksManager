using TasksManager.Api.DTOs;

namespace TasksManager.Api.Services.Interfaces;

public interface IReportService
{
    Task<ReportSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
}
