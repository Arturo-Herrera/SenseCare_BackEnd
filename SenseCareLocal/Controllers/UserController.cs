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

        public UsersController(UserService userService , DeviceService devices)
        {
            _userService = userService;
            _devices = devices;
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
                await _userService.Create(user);

                var response = new JSONResponse { Status = 0, Message = "User Succesfully Added", MessageType = MessageType.Success };

                return Ok(response);
            }
            catch (Exception e)
            {
                var response = new JSONResponse { Status = 1, Message = "Failed Creating User", MessageType = MessageType.Error };
                return StatusCode(500, response);
            }
        }


        [HttpPost("filter")]
        public async Task<IActionResult> UserFilter([FromBody] UserFilter filter)
        {
            var users = await _userService.UserFilter(filter);

            return Ok(users);
        }

        [HttpPut("updateState/{idUser}")]
        public async Task<ActionResult> UpdateUserState(int idUser, [FromQuery] bool activate)
        {
            try
            {
                Usuario result;
                if (activate)
                {
                    result = await _userService.EnableUser(idUser);
                }
                else
                {
                    result = await _userService.DisableUser(idUser);
                }

                if (result == null)
                    return NotFound($"Couldn't find a user with id: {idUser}");

                return Ok(new
                {
                    message = activate ? "User activated successfully" : "User deactivated successfully",
                    usuario = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating user state: {ex.Message}");
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
    }
}
