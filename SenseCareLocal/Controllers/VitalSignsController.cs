using Microsoft.AspNetCore.Mvc;

namespace SenseCareLocal.Controllers;
[ApiController]
[Route("api/[controller]")]
public class VitalSignsController : ControllerBase
{
    private readonly VitalSignsService _signs;

    public VitalSignsController(VitalSignsService vitalSignService)
    {
        _signs = vitalSignService;
    }

    [HttpPost("registerPulse")]
    public async Task<IActionResult> RegisterPulse([FromBody] PulsoDto dto)
    {
        try
        {
            await _signs.InsertPulse(dto.IdDispositivo, dto.Valor);
            return Ok("Pulse registered successfully");
        } catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("registerTempOxy")]
    public async Task<IActionResult> RegisterTempOxy([FromBody] TempOxiDto dto)
    {
        try
        {
            await _signs.InsertTemperatureAndOxygen(dto.IdDispositivo, dto.Temperatura, dto.Oxigeno);
            return Ok("Temperature and oxygen registered successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // Corregido: era Ok(ex.Message)
        }
    }
}
