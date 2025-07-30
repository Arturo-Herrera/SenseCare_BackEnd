using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.Services;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly PatientService _patientService;
    private readonly VitalSignsService _vitalSignService;
    private readonly AlertService _alertsService;

    public PatientController(PatientService patientService, VitalSignsService vitalSignService, AlertService alertService)
    {
        _patientService = patientService;
        _vitalSignService = vitalSignService;
        _alertsService = alertService;
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


    [HttpGet("dailyAverage")]

    public async Task<ActionResult> GetDailyAverage(int idMedic)
    {
        var averages = await _vitalSignService.GetAverageVitalsPerDay(idMedic);
        if (averages == null || !averages.Any())
            return NotFound(new JSONResponse { Status = 1, Message = "No data found", MessageType = MessageType.Warning });
        return Ok(new JSONResponse
        {
            Status = 0,
            Message = "Daily averages retrieved successfully",
            MessageType = MessageType.Success,
            Data = averages
        });
    }

    [HttpGet("activePatients/{idMedic}")]
    public async Task<ActionResult> GetActivePatients(int idMedic)
    {
        try
        {
            var activePatients = await _patientService.GetActivePatients(idMedic);
            return Ok(new JSONResponse
            {
                Status = 0,
                Message = "Active patients retrieved successfully",
                MessageType = MessageType.Success,
                Data = activePatients
            });
        }
        catch (Exception ex)
        {

            return NotFound(new JSONResponse
            {
                Status = 1,
                Message = $"Error: {ex.Message}",
                MessageType = MessageType.Warning
            });
        }
    }



    [HttpGet("dashboard/data/{idMedic}")]
    public async Task<ActionResult> GetDashboardData(int idMedic)
    {
        try
        {

            var vitalSigns = await _vitalSignService.GetAverageVitalsPerDay(idMedic);
            var activePatients = await _patientService.GetActivePatients(idMedic);
            var alerts = await _alertsService.GetTotalsToday(idMedic);
            var oxygen = await _vitalSignService.GetOxygenLevelAvg(idMedic);

            var result = new
            {
                averageVitals = vitalSigns,
                alertsPerDay = alerts,
                activePatients = activePatients,
                oxygenLevel = oxygen
            };

            return Ok(new JSONResponse
            {
                Status = 0,
                Message = "Dashboard data retrieved successfully",
                MessageType = MessageType.Success,
                Data = result
            });
        }
        catch
        {
            return NotFound(new JSONResponse
            {
                Status = 1,
                Message = "No data found",
                MessageType = MessageType.Warning
            });
        }
    }


    [HttpGet("patients/data/{idPatient}")]
    public async Task<ActionResult> GetPatientData(int idPatient)
    {
        //try
        //{
        var vitalSigns = await _vitalSignService.GetAveragePatient(idPatient);
        var infoPatient = await _patientService.GetInfoPatient(idPatient);
        var lastAlerts = await _alertsService.GetLastAlerts(idPatient);
        var lastLectures = await _vitalSignService.GetLastLectures(idPatient);


        var result = new
        {
            patient = infoPatient,
            averageVitals = vitalSigns,
            alerts = lastAlerts,
            lectures = lastLectures
        };

        return Ok(result);

        //    return Ok(new JSONResponse
        //    {
        //        Status = 0,
        //        Message = "Patient data retrieved successfully",
        //        MessageType = MessageType.Success,
        //        Data = result
        //    });
        //}
        //catch
        //{
        //    return NotFound(new JSONResponse
        //    {
        //        Status = 1,
        //        Message = "No data found",
        //        MessageType = MessageType.Warning
        //    });
        //}
    }

    [HttpGet("getSelect/{idDoctor}")]
    public async Task<ActionResult> GetInfoPatientSelect(int idDoctor)
    {
        try
        {
            var result = await _patientService.GetPatientSelect(idDoctor);// SELECT A PATIENT

            return Ok(new JSONResponse
            {
                Status = 0,
                Message = "Patient data retrieved successfully",
                MessageType = MessageType.Success,
                Data = result
            });
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }

    }
}