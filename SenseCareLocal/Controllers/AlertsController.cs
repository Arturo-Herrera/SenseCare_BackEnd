using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.Services;

namespace SenseCareLocal.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{

    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Alert>>> GetAll()
    {
        var alerts = await _alertService.GetAll();
        return Ok(alerts);
    }
}
