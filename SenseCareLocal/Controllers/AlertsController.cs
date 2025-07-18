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

    [HttpGet("patient/{id}")]
    public async Task<ActionResult<List<Alert>>> GetByPatient(int id)
    {
        var alerts = await _alertService.GetByPatient(id);
        if (alerts == null || !alerts.Any())
        {
            return NotFound();
        }
        return Ok(alerts);
    }

    [HttpPost]
    public async Task<ActionResult<JSONResponse>> Create(Alert alert)
    {
        try
        {
            await _alertService.Create(alert);
            var response = new JSONResponse { Status = 0, Message = "Alert Successfully Added", MessageType = MessageType.Success };
            return Ok(response);
        }
        catch (Exception e)
        {
            var response = new JSONResponse { Status = 1, Message = "Failed Creating Alert", MessageType = MessageType.Error };
            return StatusCode(500, response);
        }
    }
}