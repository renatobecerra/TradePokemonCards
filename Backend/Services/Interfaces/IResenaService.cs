using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IResenaService
    {
        Task<IEnumerable<object>> GetResenasPorUsuarioAsync(int id);
        Task<IEnumerable<object>> GetResenasPorCartaAsync(int id);
        Task<(bool exito, string mensaje)> PostResenaAsync(ResenaDto dto);
    }
}
