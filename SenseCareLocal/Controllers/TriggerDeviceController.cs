using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[ApiController]
[Route("api/[controller]")]


public class TriggerDeviceController : ControllerBase
{
    private readonly TriggerDevice _trigger;

    public TriggerDeviceController(TriggerDevice triggerStore)
    {
        _trigger = triggerStore;
    }

    [HttpPost("trigger")]
    public IActionResult TriggerRead([FromBody] int id)
    {
        _trigger.SetTrigger(id);
        return Ok();
    }

    [HttpGet("trigger/{id}")]
    public IActionResult CheckTrigger(int id)
    {
        bool shouldRead = _trigger.ConsumeTrigger(id);
        return Ok(new { readNow = shouldRead });
    }
}
