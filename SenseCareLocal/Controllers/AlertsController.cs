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

    [HttpPost]
    public async Task<ActionResult<JSONResponse>> Create(Alert alert)
    {
        try
        {
            await _alertService.Create(alert);
            var response = new JSONResponse { Status = 0, Message = "Alert Succesfully Added", MessageType = MessageType.Success };
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new JSONResponse { Status = 1, Message = e.Message, MessageType = MessageType.Error });
        }
    }

}
