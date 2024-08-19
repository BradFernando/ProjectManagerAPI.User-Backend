using Microsoft.AspNetCore.Mvc;
using ProjectManager.Api.User.Models;
using ProjectManager.Api.User.Repositories;

namespace ProjectManager.Api.User.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        //Inyectamos el repositorio de usuarios
        private readonly UsersRepository _usersRepository;

        public UsersController(UsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        //Métdo para registrar un usuario
        [HttpPost("register")]
        public async Task<ActionResult<Users>> CreateUser([FromBody] Users user)
        {
            var createdUser = await _usersRepository.Create(user);
            return CreatedAtAction(nameof(GetUserById), new { createdUser.id }, createdUser);
        }

        //Método para loguear un usuario
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUser([FromBody] LoginRequest loginRequest)
        {
            var result = await _usersRepository.Login(loginRequest.UserName, loginRequest.Password);
            if (result == "El nombre de usuario no existe" || result == "Contraseña incorrecta")
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        
        //Método para obtener todos los usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {
            var users = await _usersRepository.ReadAll();
            return Ok(users);
        }

        //Método para obtener un usuario por su id
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUserById(int id)
        {
            var user = await _usersRepository.ReadById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        
        //Método para obtener un usuario por su email
        [HttpGet("byemail")]
        public async Task<ActionResult<Users>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _usersRepository.ReadByEmailAddress(email);
            if (user == null)
            {
                return NotFound("El correo electrónico no existe");
            }
            return Ok(user);
        }

        //Método para actualizar un usuario
        [HttpPut("{id}")]
        public async Task<ActionResult<Users>> UpdateUser(int id, [FromBody] Users user)
        {
            if (id != user.id)
            {
                return BadRequest();
            }

            var updatedUser = await _usersRepository.Update(user);
            return Ok(updatedUser);
        }
        
        //Método para actualizar la contraseña de un usuario por su email
        [HttpPut("updatepassword")]
        public async Task<ActionResult<Users>> UpdatePasswordByEmail([FromQuery] string email, [FromBody] string newPassword)
        {
            var updatedUser = await _usersRepository.UpdatePasswordByEmail(email, newPassword);
            return Ok(updatedUser);
        }
    

        //Método para eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteUser(int id)
        {
            await _usersRepository.Delete(id);
            return Ok($"El usuario con id {id} fue eliminado correctamente.");
        }
        
        //Método para enviar un mensaje a un usuario por correo electrónico
        [HttpPost("sendmessage")]
        public async Task<ActionResult<object>> SendMessageToUser([FromQuery] string email, [FromBody] EmailSend emailConfig)
        {
            try
            {
                var result = await _usersRepository.SendMessageToUser(email, emailConfig);

                // Verifica si el resultado es un mensaje de error o éxito
                if (result == "El correo electrónico no existe")
                {
                    return BadRequest(new { message = result });
                }

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error al enviar el mensaje: {ex.Message}" });
            }
        }

        
        //Método para confirmar el correo electrónico de un usuario
        [HttpPost("confirmemail")]
        public async Task<ActionResult<Users>> ConfirmUserEmail([FromQuery] string email)
        {
            var updatedUser = await _usersRepository.ConfirmUserEmail(email);
            return Ok(updatedUser);
        }
        
        //Métdo para rechazar el correo electrónico de un usuario
        [HttpPost("rejectemail")]
        public async Task<ActionResult<Users>> RejectUserEmail([FromQuery] string email)
        {
            var updatedUser = await _usersRepository.RejectUserEmail(email);
            return Ok(updatedUser);
        }
        
        //Método para subir una imagen a Cloudinary
        [HttpPost("uploadimage")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            try
            {
                var imageUrl = await _usersRepository.UploadImageToCloudinary(file);
                return Ok(new { ImageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al cargar la imagen: {ex.Message}");
            }
        }
        
        [HttpGet("checkconfirmationstatus")]
        public async Task<ActionResult<bool>> CheckConfirmationStatus([FromQuery] string email)
        {
            try
            {
                var isConfirmed = await _usersRepository.CheckConfirmationStatus(email);
                return Ok(isConfirmed);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error al verificar el estado de confirmación: {ex.Message}" });
            }
        }
    }
}