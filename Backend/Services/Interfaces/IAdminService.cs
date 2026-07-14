using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<object>> GetUsuariosAsync();
        Task<(bool exito, string mensaje)> CambiarEstadoUsuarioAsync(int idUsuario, int nuevoEstado);
        Task<(bool exito, string mensaje)> BanearUsuarioAsync(int idUsuario, BanDto dto);
        Task<(bool exito, string mensaje)> EliminarUsuarioAsync(int idUsuario);
        Task<(bool exito, string mensaje)> CambiarRolUsuarioAsync(int idUsuario, string nuevoRol);
        Task<IEnumerable<object>> GetArticulosMercadoAsync();
        Task<(bool exito, string mensaje)> EliminarArticuloMercadoAsync(int idInventarioUser);
        Task<(bool exito, string mensaje)> CrearReporteAsync(ReporteDto dto);
        Task<IEnumerable<object>> GetReportesAsync();
        Task<(bool exito, string mensaje)> EliminarReporteAsync(int idReporte);
    }
}
