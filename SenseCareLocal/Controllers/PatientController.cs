using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly PatientService _patientService;
    private readonly VitalSignsService _vitalSignService;

    public PatientController(PatientService patientService, VitalSignsService vitalSignService)
    {
        _patientService = patientService;
        _vitalSignService = vitalSignService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterPatient(RegisterPatientDto dto)
    {
        var result = await _patientService.RegisterPatient(dto);
        if (!result)
            return BadRequest(new JSONResponse
            {
                Status = 1,
                Message = "Couldn't register patient",
                MessageType = MessageType.Error
            });

        return Ok(new JSONResponse
        {
            Status = 0,
            Message = "Patient created succesfully",
            MessageType = MessageType.Success
        });
    }

    [HttpGet("signsByCaregiver/{idCaregiver}")]
    public async Task<IActionResult> GetSignsPerCaregiver(int idCaregiver)
    {
        var patient = await _patientService.ObtenerPacientePorCuidadorAsync(idCaregiver);
        if (patient == null)
            return NotFound(new JSONResponse { Status = 1, Message = "Patient not found", MessageType = MessageType.Warning });

        var signs = await _vitalSignService.GetByPatient(patient.Id);

        return Ok(new JSONResponse
        {
            Status = 0,
            Message = "Vital signs correctly get",
            MessageType = MessageType.Success,
            Data = signs
        });
    }
}