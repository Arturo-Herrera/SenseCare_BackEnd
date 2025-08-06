using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[ApiController]
[Route("api/[controller]")]


public class TriggerDeviceController : ControllerBase
{
    private readonly TriggerDevice _trigger;
    private readonly PatientService _patients;

    public TriggerDeviceController(TriggerDevice triggerStore , PatientService patients)
    {
        _trigger = triggerStore;
        _patients = patients;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerRead([FromBody] int idCuidador)
    {
        var deviceId = await _patients.GetDeviceIdByCuidadorAsync(idCuidador);

        if (deviceId == null)
        {
            return NotFound(new { message = "No se encontró un paciente asignado a este cuidador." });
        }

        _trigger.SetTrigger(deviceId.Value);
        return Ok(new { message = $"Trigger enviado al dispositivo {deviceId.Value}" });
    }

    [HttpGet("trigger/{id}")]
    public IActionResult CheckTrigger(int id)
    {
        bool shouldRead = _trigger.ConsumeTrigger(id);
        return Ok(new { readNow = shouldRead });
    }
}
