﻿using System.Net;
using System.Net.Mail;
using ProjectManager.Api.User.Context;
using ProjectManager.Api.User.Models;
using ProjectManager.Api.User.Utils;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.User.Services;

namespace ProjectManager.Api.User.Repositories
{
    public class UsersRepository : IActions<Users>
    {
        //Llamamos al contexto de la base de datos y a Cloudinary
        private readonly UserDbContext _context;
        private readonly CloudinaryService _cloudinaryService;


        //Inyectamos el contexto de la base de datos y Cloudinary
        public UsersRepository(UserDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;

        }

        //Método para leer todos los usuarios
        public async Task<IEnumerable<Users>> ReadAll()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer todos los usuarios", ex);
            }
        }

        //Método para crear un usuario
        public async Task<Users> Create(Users user)
        {
            try
            {
                if (user.type < 0 || user.type > 1)
                {
                    throw new ArgumentException("El campo 'type' debe ser 0 o 1");
                }

                if (user.status < 0 || user.status > 1)
                {
                    throw new ArgumentException("El campo 'status' debe ser 0 o 1");
                }

                user.password = PasswordProtected.HashPassword(user.password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario", ex);
            }
        }

        //Método para leer un usuario por su id
        public async Task<Users?> ReadById(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al leer el usuario con id {id}", ex);
            }
        }
        
        //Método para leer un usuario por su email
        public async Task<Users?> ReadByEmailAddress(string emailAddress)
        {
            try
            {
                return await _context.Users.SingleOrDefaultAsync(u => u.email == emailAddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al leer el usuario con email {emailAddress}", ex);
            }
        }

        //Método para actualizar un usuario
        public async Task<Users> Update(Users user)
        {
            try
            {
                if (user.type < 0 || user.type > 1)
                {
                    throw new ArgumentException("El campo 'type' debe ser 0 o 1");
                }

                if (user.status < 0 || user.status > 1)
                {
                    throw new ArgumentException("El campo 'status' debe ser 0 o 1");
                }

                user.password = PasswordProtected.HashPassword(user.password);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el usuario", ex);
            }
        }
        
        //Método para actualizar la contraseña de un usuario por su email
        public async Task<Users> UpdatePasswordByEmail(string emailAddress, string newPassword)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.email == emailAddress);
                if (user == null)
                {
                    throw new Exception("El correo electrónico no existe");
                }

                user.password = PasswordProtected.HashPassword(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar la contraseña del usuario con email {emailAddress}", ex);
            }
        }
        

        //Método para eliminar un usuario
        public async Task<Users> Delete(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    throw new Exception($"Usuario con id {id} no encontrado");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el usuario con id {id}", ex);
            }
        }
        
        //Método para iniciar sesión
        public async Task<string> Login(string userName, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.userName == userName);
                if (user == null)
                {
                    return "El nombre de usuario no existe";
                }

                if (!PasswordProtected.VerifyPassword(password, user.password))
                {
                    return "Contraseña incorrecta";
                }

                return "Inicio de sesión exitoso";
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar sesión", ex);
            }
        }
        
        //Método para enviar un mensaje a un usuario por correo electrónico
        public async Task<string> SendMessageToUser(string email, EmailSend emailConfig)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.email == email);
                if (user == null)
                {
                    return "El correo electrónico no existe";
                }

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailConfig.FromEmail, emailConfig.FromPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailConfig.FromEmail),
                    Subject = emailConfig.Subject,
                    Body = emailConfig.Body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);
                return "Mensaje enviado correctamente";
            }
            catch (Exception ex)
            {
                throw new Exception("Error al enviar el mensaje", ex);
            }
        }
        
        //Método para confirmar el correo electrónico de un usuario
        public async Task<Users> ConfirmUserEmail(string emailAddress)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.email == emailAddress);
                if (user == null)
                {
                    throw new Exception("El correo electrónico no existe");
                }

                user.status = 1; // Update status to 1
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al confirmar el correo electrónico del usuario con email {emailAddress}", ex);
            }
        }
        
        //Metodo para rechazar el correo electrónico de un usuario
        public async Task<Users> RejectUserEmail(string emailAddress)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.email == emailAddress);
                if (user == null)
                {
                    throw new Exception("El correo electrónico no existe");
                }

                user.status = 0; // Update status to 0
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al rechazar el correo electrónico del usuario con email {emailAddress}", ex);
            }
        }
        
        // Método para cargar una imagen a Cloudinary
        public async Task<string> UploadImageToCloudinary(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("No file provided");
                }

                var imageUrl = await _cloudinaryService.UploadImageAsync(file);
                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar la imagen a Cloudinary", ex);
            }
        }
        
        public async Task<bool> CheckConfirmationStatus(string emailAddress)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.email == emailAddress);
                if (user == null)
                {
                    throw new Exception("El correo electrónico no existe");
                }

                return user.status == 1; // Devuelve true si el estado es 1 (confirmado), de lo contrario, false
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar el estado de confirmación del usuario con email {emailAddress}", ex);
            }
        }

        
    }
}