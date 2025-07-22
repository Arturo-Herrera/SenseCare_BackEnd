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

    [HttpGet("getTotalsToday")]

    public async Task<ActionResult> GetTotalsToday()
    {
        var alerts = await _alertService.GetTotalsToday();

        return Ok(alerts);
    }

}
