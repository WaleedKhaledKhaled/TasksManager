using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs;

namespace TasksManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<HealthStatusResponse>>> GetHealth(CancellationToken cancellationToken)
    {
        // Report whether the API and database are ready to serve requests
        var databaseHealthy = await dbContext.Database.CanConnectAsync(cancellationToken);
        var response = new HealthStatusResponse("Healthy", databaseHealthy ? "Healthy" : "Unreachable");
        return Ok(ApiResponse.Success(response));
    }
}
