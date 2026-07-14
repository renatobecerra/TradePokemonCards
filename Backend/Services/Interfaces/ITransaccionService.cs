using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface ITransaccionService
    {
        Task<(bool exito, string mensaje)> ProponerTratoAsync(ProponerTratoDto dto);
        Task<(bool exito, string mensaje)> ConfirmarTratoAsync(ProponerTratoDto dto);
        Task<bool> VerificarTransaccionCompletadaAsync(int idVendedor, int idComprador);
    }
}
