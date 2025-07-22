using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.Services;

namespace SenseCareLocal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AlertService _alertService;
        private readonly ReportService _reportService;
        private readonly PatientService _patientService;

        public ReportController(AlertService alertService, ReportService reportService, PatientService patientService)
        {
            _alertService = alertService;
            _reportService = reportService;
            _patientService = patientService;
        }


        [HttpGet("medicalHistory/data/{idPatient}")]
        public async Task<ActionResult<List<Alert>>> GetAll(int idPatient)
        {
            var alerts = await _alertService.GetAll(idPatient);
            var reports = await _reportService.GetAllByPatient(idPatient);
            var patients = await _patientService.GetPatientSelect(); //TRAE TODOS PARA EL SELECT

            var result = new
            {
                alertsPatient = alerts,
                reportsPatient = reports,
                selectPatients = patients
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Report report)
        {
            try
            {
                await _reportService.Create(report);
                var response = new JSONResponse
                {
                    Status = 0,
                    Message = "Report Succesfully Added",
                    MessageType = MessageType.Success
                };

                return Ok(response);
            }
            catch
            {
                var response = new JSONResponse
                {
                    Status = 1,
                    Message = "Failed Creating Report",
                    MessageType = MessageType.Error
                };

                return StatusCode(500, response);
            }
        }
    }
}
