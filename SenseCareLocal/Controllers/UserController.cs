using Microsoft.AspNetCore.Mvc;
using SenseCareLocal.Services;

namespace SenseCareLocal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
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
    }
}
