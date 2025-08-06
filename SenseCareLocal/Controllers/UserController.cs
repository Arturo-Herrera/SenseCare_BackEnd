using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using SenseCareLocal.Services;
using System;

namespace SenseCareLocal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly DeviceService _devices;
        private readonly PatientService _patientService;

        public UsersController(UserService userService, DeviceService devices, PatientService patient)
        {
            _userService = userService;
            _devices = devices;
            _patientService = patient;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetById(id);

                if (user is null) return NotFound();

                return Ok(user);
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<JSONResponse>> Create(User user)
        {
            try
            {
                var createdUser = await _userService.Create(user);
                var response = new JSONResponse
                {
                    Status = 0,
                    Message = "User Successfully Added",
                    MessageType = MessageType.Success,
                    Data = new { Id = createdUser.Id } // Incluir el ID en la respuesta
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                var response = new JSONResponse
                {
                    Status = 1,
                    Message = "Failed Creating User",
                    MessageType = MessageType.Error
                };
                return StatusCode(500, response);
            }
        }


        [HttpPost("filter")]
        public async Task<IActionResult> UserFilter([FromBody] UserFilter filter)
        {
            var users = await _userService.UserFilter(filter);

            return Ok(users);
        }

        [HttpPut("disable/{idPaciente}")]
        public async Task<ActionResult> DisablePatientAndCaregiver([FromRoute] int idPaciente, [FromQuery] bool activate)
        {
            try
            {
                // 1. Buscar al paciente en la colección Paciente
                var patient = await _patientService.GetPatientById(idPaciente);
                if (patient == null)
                    return NotFound($"No patient found with id: {idPaciente}");

                // 2. Deshabilitar (o habilitar) al usuario base del paciente
                var updatedPatientUser = activate
                    ? await _userService.EnableUser(patient.IDUsuario)
                    : await _userService.DisableUser(patient.IDUsuario);

                // 3. Deshabilitar (o habilitar) al cuidador
                var updatedCaregiverUser = activate
                    ? await _userService.EnableUser(patient.IDCuidador)
                    : await _userService.DisableUser(patient.IDCuidador);

                return Ok(new
                {
                    message = activate ? "Patient and caregiver activated successfully" : "Patient and caregiver deactivated successfully",
                    patient = updatedPatientUser,
                    caregiver = updatedCaregiverUser
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error disabling patient and caregiver: {ex.Message}");
            }
        }


        [HttpGet("caregivers")]
        public async Task<ActionResult<List<Usuario>>> GetCaregivers()
        {
            try
            {
                var caregivers = await _userService.GetAvailableCaregivers();
                var devices = await _devices.GetAvailableDevices();

                var info = new
                {
                    AvailableCaregivers = caregivers,
                    availableDevices = devices
                };

                var result = new JSONResponse
                {
                    Status = 0,
                    Message = "Succesfull retrieved information",
                    MessageType = MessageType.Success,
                    Data = info
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving caregivers: {ex.Message}");
            }
        }

        [HttpPut("updateUser")]
        public async Task<ActionResult> UpdateUser([FromBody] UpdateUserDTO userDto)
        {
            try
            {
                if (userDto == null || userDto.Id == 0)
                {
                    return BadRequest("Datos de usuario inválidos.");
                }

                var updatedUser = await _userService.UpdateUser(userDto);

                if (updatedUser == null)
                {
                    return NotFound($"No se encontró un usuario con ID {userDto.Id}.");
                }

                return Ok(new
                {
                    message = "Usuario actualizado correctamente.",
                    usuario = updatedUser
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
            }
        }



    }
}