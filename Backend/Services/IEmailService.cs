namespace Backend.Services
{
    public interface IEmailService
    {
        Task EnviarCodigoVerificacionAsync(string emailDestino, string nombreUsuario, string codigo);
        Task EnviarCodigoRecuperacionAsync(string emailDestino, string nombreUsuario, string codigo);
    }
}
