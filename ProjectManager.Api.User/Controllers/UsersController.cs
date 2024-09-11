using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Api.User.Models;
using ProjectManager.Api.User.Repositories;
using ProjectManager.Api.User.Utils;

namespace ProjectManager.Api.User.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        //Inyectamos el repositorio de usuarios
        private readonly UsersRepository _usersRepository;
        private readonly JwtUtils _jwtUtils;


        public UsersController(UsersRepository usersRepository, JwtUtils jwtUtils)
        {
            _usersRepository = usersRepository;
            _jwtUtils = jwtUtils;
        }


        //Métdo para registrar un usuario
        [HttpPost("register")]
        public async Task<ActionResult<Users>> CreateUser([FromBody] Users user)
        {
            try
            {
                var createdUser = await _usersRepository.Create(user);
                return CreatedAtAction(nameof(GetUserById), new { createdUser.id }, createdUser);
            }
            catch (Exception ex)
            {
                // Si el error es por correo o nombre de usuario duplicado, devolvemos un BadRequest con un mensaje adecuado
                if (ex.Message.Contains("El correo electrónico ya ha sido registrado"))
                {
                    return BadRequest(new { message = "Este correo ya ha sido registrado. Por favor revisa tu bandeja de entrada para confirmar tu cuenta." });
                }
                else if (ex.Message.Contains("El nombre de usuario ya ha sido registrado"))
                {
                    return BadRequest(new { message = "Este nombre de usuario ya está en uso. Por favor elige otro." });
                }

                // Si es otro tipo de error, devolver un error de servidor
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error al registrar el usuario: {ex.Message}" });
            }
        }


        //Método para loguear un usuario
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginUser([FromBody] LoginRequest loginRequest)
        {
            var result = await _usersRepository.Login(loginRequest.UserName, loginRequest.Password);
            if (result == "El nombre de usuario no existe" || result == "Contraseña incorrecta" ||
                result == "Su correo electrónico no ha sido activado")
            {
                return BadRequest(result);
            }

            var token = _jwtUtils.GenerateToken(loginRequest.UserName); // Genera el token JWT
            return Ok(new { Token = token });
        }

        //Método para obtener todos los usuarios
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {
            var users = await _usersRepository.ReadAll();
            return Ok(users);
        }

        //Método para obtener un usuario por su id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Users>> GetUserById(int id)
        {
            var user = await _usersRepository.ReadById(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        
        //Metodo para obtener un usuario por su nombre de usuario
        [HttpGet("byusername")]
        [Authorize]
        public async Task<ActionResult<Users>> GetUserByUserName([FromQuery] string userName)
        {
            var user = await _usersRepository.ReadByUserName(userName);
            if (user == null)
            {
                return NotFound("El nombre de usuario no existe");
            }

            return Ok(user);
        }

        //Método para obtener un usuario por su email
        [HttpGet("byemail")]
        [Authorize]
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
        [Authorize]
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
        public async Task<ActionResult<Users>> UpdatePasswordByEmail([FromQuery] string email,
            [FromBody] string newPassword)
        {
            var updatedUser = await _usersRepository.UpdatePasswordByEmail(email, newPassword);
            return Ok(updatedUser);
        }


        //Método para eliminar un usuario
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<string>> DeleteUser(int id)
        {
            await _usersRepository.Delete(id);
            return Ok($"El usuario con id {id} fue eliminado correctamente.");
        }

        //Método para enviar un mensaje a un usuario por correo electrónico
        [HttpPost("sendmessage")]
        public async Task<ActionResult<object>> SendMessageToUser([FromQuery] string email,
            [FromBody] EmailSend emailConfig)
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = $"Error al enviar el mensaje: {ex.Message}" });
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

        //Método para subir una imagen a Imgbb
        [HttpPost("uploadimage")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            try
            {
                var imageUrl = await _usersRepository.UploadImageToImgbb(file); // Cambiado a Imgbb
                return Ok(new { ImageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al cargar la imagen: {ex.Message}");
            }
        }

    
        
        //Método para verificar el estado de confirmación de un usuario
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