using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IInventarioService
    {
        Task<IEnumerable<object>> ObtenerVendedoresAsync(string idTgc);
        Task<int?> ObtenerPrecioPromedioAsync(string idTgc);
        Task<IEnumerable<object>> ObtenerInventarioAsync(int idUsuario);
        Task<(bool exito, string mensaje)> AgregarAlInventarioAsync(CrearItemDto datos);
        Task<(bool exito, string mensaje)> EditarItemAsync(int idInventarioUser, CrearItemDto datos);
        Task<(bool exito, string mensaje)> EliminarItemAsync(int idInventarioUser);
    }
}
