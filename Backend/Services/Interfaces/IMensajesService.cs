using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IMensajesService
    {
        Task<IEnumerable<object>> GetConversacionesAsync(int usuarioId);
        Task<IEnumerable<object>> GetHistorialAsync(int usuarioId, int contactoId);
        Task<(bool exito, string mensaje, object? mensajeObj)> EnviarMensajeAsync(EnviarMensajeDto dto);
        Task<(bool exito, string mensaje, object? usuario)> GetUsuarioDetalleAsync(int usuarioId);
        Task<(bool exito, string mensaje)> DeleteConversacionAsync(int usuarioId, int contactoId);
    }
}
