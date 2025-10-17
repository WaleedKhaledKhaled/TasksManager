using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TasksManager.Api.DTOs;
using TasksManager.Api.Extensions;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController(IReportService reportService, IMemoryCache memoryCache, ILogger<ReportsController> logger) : ControllerBase
{
    private const string CachePrefix = "report-summary-";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    [HttpGet("summary")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<ApiResponse<ReportSummaryResponse>>> GetSummary(CancellationToken cancellationToken)
    {
        // Serve a quick snapshot of task progress while caching the heavy work
        var userId = User.GetUserId();
        var cacheKey = $"{CachePrefix}{userId}";

        if (!memoryCache.TryGetValue(cacheKey, out ReportSummaryResponse? summary))
        {
            logger.LogInformation("Cache miss for {CacheKey}. Generating summary.", cacheKey);
            summary = await reportService.GetSummaryAsync(userId, cancellationToken);
            memoryCache.Set(cacheKey, summary, CacheDuration);
        }

        summary ??= new ReportSummaryResponse();
        Response.Headers["Cache-Control"] = "public,max-age=300";
        return Ok(ApiResponse.Success(summary));
    }
}
