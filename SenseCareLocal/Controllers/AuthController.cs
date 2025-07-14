using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.DTOs;
using SenseCareLocal.Services;
using SenseCareLocal.DTOs;
using SenseCareLocal.Services;
using SenseCareAPI.Helpers;


namespace SenseCareLocal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _users;

    public AuthController(UserService users) => _users = users;

    [HttpPost("login")]
    public async Task<ActionResult<JSONResponse>> Login(LoginDto dto)
    {
        var user = await _users.GetByEmail(dto.Email);

        if (user == null || !PasswordHelper.Verify(dto.Password, user.Contrasena))
            return Unauthorized(new JSONResponse
            {
                Status = 1,
                Message = "Invalid Credentials",
                MessageType = MessageType.Error
            });

        // reglas de acceso según cliente
        if (dto.DeviceType == "mobile" && user.IDTipoUsuario.Id != "CUID")
            return StatusCode(403, new JSONResponse
            {
                Status = 1,
                Message = "Only caregivers can entry to mobile app",
                MessageType = MessageType.Warning
            });

        if (dto.DeviceType == "web" && user.IDTipoUsuario.Id != "MED")
            return StatusCode(403, new JSONResponse
            {
                Status = 1,
                Message = "Only doctors can entry to web app",
                MessageType = MessageType.Warning
            });

        // éxito: devolvemos info del usuario
        return Ok(new JSONResponse
        {
            Status = 0,
            Message = "Login succesfully",
            MessageType = MessageType.Success,
            Data = new
            {
                Id = user.Id,
                Email = user.Email,
                Rol = user.IDTipoUsuario.Id
            }
        });
    }

}
