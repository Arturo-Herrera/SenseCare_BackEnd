using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.DataAccess;
using SenseCareLocal.Services;

[ApiController]
[Route("api/[controller]")]
public class MapController : ControllerBase
{
    private readonly MapService _mapService;

    public MapController(MapService mapService)
    {
        _mapService = mapService;
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetMapData()
    {
        var mapData = await _mapService.GetAllDevicesWithPatient();
        if (mapData == null || mapData.Count == 0)
        {
            return NotFound(new { Status = 1, Message = "No data found", MessageType = "Warning" });
        }
        return Ok(new { Status = 0, Message = "Map data retrieved successfully", MessageType = "Success", Data = mapData });
    }
}
