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

    [HttpGet("getByPatient/{idCaregiver}")]
    public async Task<ActionResult> GetAlertsByPatient(int idCaregiver)
    {
        try
        {
            var alerts = await _alertService.GetByPatient(idCaregiver);

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }

    [HttpGet("getTotalsToday")]

    public async Task<ActionResult> GetTotalsToday(int idMedic)
    {
        var alerts = await _alertService.GetTotalsToday(idMedic);

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
