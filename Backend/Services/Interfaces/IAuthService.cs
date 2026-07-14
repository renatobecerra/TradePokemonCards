using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool exito, string mensaje, object? usuario)> GoogleLoginAsync(string credential);
        Task<(bool exito, string mensaje)> CambiarPresenciaAsync(CambiarPresenciaDto datos);
        Task<(bool exito, string mensaje, object? usuario)> RegistrarAsync(Usuario nuevoUsuario);
        Task<(bool exito, string mensaje, object? usuario)> VerificarAsync(VerificarDto datosVerificacion);
        Task<(bool exito, string mensaje)> SolicitarRecuperacionAsync(RecoveryRequestDto request);
        Task<(bool exito, string mensaje)> ValidarCodigoRecuperacionAsync(VerificarDto datos);
        Task<(bool exito, string mensaje)> ResetearPasswordAsync(ResetPasswordDto datos);
        Task<(bool exito, string mensaje, object? usuario)> LoginAsync(LoginDto datosLogin);
        Task<(bool exito, string mensaje)> HacerAdminAsync(string correo);
        Task<(bool exito, string mensaje, object? perfil)> GetPerfilPublicoAsync(int id);
        Task<(bool exito, string mensaje)> ActualizarPerfilAsync(ActualizarPerfilDto datos);
        Task<(bool exito, string mensaje)> CambiarPasswordAsync(CambiarPasswordDto datos);
    }
}
