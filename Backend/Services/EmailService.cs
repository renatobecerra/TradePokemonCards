using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCodigoVerificacionAsync(string emailDestino, string nombreUsuario, string codigo)
        {
            var googleSettings = _config.GetSection("GoogleSettings");
            
            var clientID = googleSettings["ClientId"];
            var clientSecret = googleSettings["ClientSecret"];
            var refreshToken = googleSettings["RefreshToken"];
            var senderEmail = googleSettings["SenderEmail"] ?? throw new Exception("SenderEmail no configurado");

            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Configuración de Google incompleta en appsettings.json");
            }

            var tokenResponse = new TokenResponse { RefreshToken = refreshToken };
            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer {
                    ClientSecrets = new ClientSecrets { ClientId = clientID, ClientSecret = clientSecret }
                }), "user", tokenResponse);

            await credentials.RefreshTokenAsync(CancellationToken.None);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pokemon Market", senderEmail));
            message.To.Add(new MailboxAddress(nombreUsuario, emailDestino));
            message.Subject = "Tu código de verificación - Pokemon Market";

            message.Body = new TextPart("html") {
                Text = $@"
                    <div style='background-color: #000000; padding: 60px 40px; font-family: ""Inter"", Arial, sans-serif; color: #ffffff; text-align: center; max-width: 600px; margin: 0 auto; border: 1px solid #1a1a1a; border-radius: 40px;'>
                        <div style='margin-bottom: 40px;'>
                            <span style='font-family: monospace; font-size: 28px; font-weight: 900; color: #ffffff; letter-spacing: 4px;'>
                                <span style='color: #2ecc71;'>[</span>POKET<span style='color: #2ecc71;'>]</span>.
                            </span>
                        </div>
                        
                        <h1 style='color: #ffffff; font-size: 42px; font-family: ""Bebas Neue"", Arial, sans-serif; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 1px;'>¡Hola, {nombreUsuario}!</h1>
                        <p style='color: rgba(255, 255, 255, 0.4); font-size: 16px; line-height: 1.6; margin-bottom: 40px;'>Estás a punto de entrar al punto de encuentro de los mejores coleccionistas. Confirma tu identidad con el siguiente código:</p>
                        
                        <div style='background: rgba(46, 204, 113, 0.05); border: 1px dashed rgba(46, 204, 113, 0.4); padding: 40px; border-radius: 24px; margin: 30px 0;'>
                            <span style='display: block; font-family: monospace; letter-spacing: 20px; color: #2ecc71; font-size: 48px; font-weight: 800; margin-left: 20px;'>{codigo}</span>
                        </div>
                        
                        <p style='color: rgba(255, 255, 255, 0.2); font-size: 13px; margin-top: 40px; line-height: 1.5;'>Si no intentaste crear una cuenta en POKET, puedes ignorar este correo de forma segura.</p>
                        
                        <div style='margin-top: 50px; padding-top: 30px; border-top: 1px solid #111111;'>
                            <p style='color: rgba(255, 255, 255, 0.15); font-size: 11px; font-family: monospace; letter-spacing: 2px; text-transform: uppercase;'>© 2026 POKET. EL PUNTO DE ENCUENTRO PARA COLECCIONISTAS.</p>
                        </div>
                    </div>"
            };

            using var client = new SmtpClient();
            try 
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(senderEmail, credentials.Token.AccessToken);
                await client.AuthenticateAsync(oauth2);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // En un entorno real, usaríamos un Logger aquí
                throw new Exception($"Error al enviar el correo: {ex.Message}");
            }
        }

        public async Task EnviarCodigoRecuperacionAsync(string emailDestino, string nombreUsuario, string codigo)
        {
            var googleSettings = _config.GetSection("GoogleSettings");
            
            var clientID = googleSettings["ClientId"];
            var clientSecret = googleSettings["ClientSecret"];
            var refreshToken = googleSettings["RefreshToken"];
            var senderEmail = googleSettings["SenderEmail"] ?? throw new Exception("SenderEmail no configurado");

            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("Configuración de Google incompleta en appsettings.json");
            }

            var tokenResponse = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
            var credentials = new Google.Apis.Auth.OAuth2.UserCredential(new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow(
                new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow.Initializer {
                    ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets { ClientId = clientID, ClientSecret = clientSecret }
                }), "user", tokenResponse);

            //Inicio prueba de cod
            await credentials.RefreshTokenAsync(CancellationToken.None);


            //FIn inicio de prueba 
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pokemon Market", senderEmail));
            message.To.Add(new MailboxAddress(nombreUsuario, emailDestino));
            message.Subject = "Recuperación de contraseña - Pokemon Market";

            message.Body = new TextPart("html") {
                Text = $@"
                    <div style='background-color: #000000; padding: 60px 40px; font-family: ""Inter"", Arial, sans-serif; color: #ffffff; text-align: center; max-width: 600px; margin: 0 auto; border: 1px solid #1a1a1a; border-radius: 40px;'>
                        <div style='margin-bottom: 40px;'>
                            <span style='font-family: monospace; font-size: 28px; font-weight: 900; color: #ffffff; letter-spacing: 4px;'>
                                <span style='color: #2ecc71;'>[</span>POKET<span style='color: #2ecc71;'>]</span>.
                            </span>
                        </div>
                        
                        <h1 style='color: #ffffff; font-size: 42px; font-family: ""Bebas Neue"", Arial, sans-serif; margin-bottom: 10px; text-transform: uppercase; letter-spacing: 1px;'>¿Olvidaste tu contraseña?</h1>
                        <p style='color: rgba(255, 255, 255, 0.4); font-size: 16px; line-height: 1.6; margin-bottom: 40px;'>No te preocupes, {nombreUsuario}. Nos pasa a los mejores entrenadores. Usa este código para restablecer tu acceso:</p>
                        
                        <div style='background: rgba(46, 204, 113, 0.05); border: 1px dashed rgba(46, 204, 113, 0.4); padding: 40px; border-radius: 24px; margin: 30px 0;'>
                            <span style='display: block; font-family: monospace; letter-spacing: 20px; color: #2ecc71; font-size: 48px; font-weight: 800; margin-left: 20px;'>{codigo}</span>
                        </div>
                        
                        <p style='color: rgba(255, 255, 255, 0.2); font-size: 13px; margin-top: 40px; line-height: 1.5;'>Si no solicitaste este cambio, te recomendamos asegurar tu cuenta cambiando tu contraseña actual.</p>
                        
                        <div style='margin-top: 50px; padding-top: 30px; border-top: 1px solid #111111;'>
                            <p style='color: rgba(255, 255, 255, 0.15); font-size: 11px; font-family: monospace; letter-spacing: 2px; text-transform: uppercase;'>© 2026 POKET. EL PUNTO DE ENCUENTRO PARA COLECCIONISTAS.</p>
                        </div>
                    </div>"
            };

            using var client = new SmtpClient();
            try 
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(senderEmail, credentials.Token.AccessToken);
                await client.AuthenticateAsync(oauth2);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el correo de recuperación: {ex.Message}");
            }
        }
    }
}