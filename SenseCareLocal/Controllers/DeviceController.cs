using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DeviceController(DeviceService deviceService) => _deviceService = deviceService;

    [HttpPost]
    public async Task<IActionResult> RegisterDevice(Device device)
    {
        try
        {
            await _deviceService.CreateDevice(device);
            var response = new JSONResponse { Status = 0, Message = "Device registered succesfully" };
            return Ok(response);
        }
        catch (Exception e)
        {
            var response = new JSONResponse { Status = 1, Message = "Failed to register device", MessageType = MessageType.Error };
            return StatusCode(500, response);
        }

    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        try
        {
            var device = await _deviceService.GetById(id);
            if (device is null) return NotFound();


            return Ok(device);
        } catch(Exception e) 
        {
            return NotFound();
        }
    }
}

