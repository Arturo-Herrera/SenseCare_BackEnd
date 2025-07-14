using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SenseCareLocal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InformeController : ControllerBase
    {
        private readonly VitalSignsService _vitalSignsService;
        private readonly PatientService _patientService;
        private readonly InformeService _informeService;

        public InformeController(
            VitalSignsService vitalSignsService,
            PatientService patientService,
            InformeService informeService) 
        {
            _vitalSignsService = vitalSignsService;
            _patientService = patientService;
            _informeService = informeService; 
        }

        [HttpGet]
        public async Task<ActionResult<List<Informe>>> GetAll()
        {
            var informes = await _informeService.GetAll();
            return Ok(informes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Informe>> GetInforme(int id)
        {
            var patient = await _patientService.ObtenerPacientePorCuidadorAsync(id);
            if (patient == null) return NotFound("Patient not found.");

            var vitalSigns = await _vitalSignsService.GetByPatient(patient.Id);
            if (vitalSigns.Count == 0) return NotFound("No vital signs found for this patient.");

            var informe = new Informe
            {
                // Puedes mapear datos aquí si es necesario
            };

            return Ok(informe);
        }
    }
}
