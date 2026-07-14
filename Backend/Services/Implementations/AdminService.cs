using Backend.Models;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly PokemonMarketContext _context;

        public AdminService(PokemonMarketContext context)
        {
            _context = context;
        }

        private async Task EnsureReportesTableExists()
        {
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                return;
            }
            
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS reportes (
                    IdReporte INT AUTO_INCREMENT PRIMARY KEY,
                    IdUsuarioReportante INT NOT NULL,
                    IdUsuarioReportado INT NOT NULL,
                    Motivo VARCHAR(500) NOT NULL,
                    Fecha DATETIME NOT NULL,
                    Estado VARCHAR(50) DEFAULT 'Pendiente',
                    FOREIGN KEY (IdUsuarioReportante) REFERENCES usuario(ID_Usuarios) ON DELETE CASCADE,
                    FOREIGN KEY (IdUsuarioReportado) REFERENCES usuario(ID_Usuarios) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ");

            await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM inventario_usuario 
                WHERE ID_Usuario IN (
                    SELECT ID_Usuarios FROM usuario WHERE Rol = 'Administrador'
                );
            ");

            await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM mensajes 
                WHERE ID_Remitente IN (
                    SELECT ID_Usuarios FROM usuario WHERE Rol = 'Administrador'
                ) OR ID_Destinatario IN (
                    SELECT ID_Usuarios FROM usuario WHERE Rol = 'Administrador'
                );
            ");

            try
            {
                var conn = _context.Database.GetDbConnection();
                bool hasMotivoColumn = false;
                bool hasFechaColumn = false;
                
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COLUMN_NAME FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'usuario'";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var colName = reader.GetString(0);
                            if (colName.Equals("MotivoBaneo", StringComparison.OrdinalIgnoreCase)) hasMotivoColumn = true;
                            if (colName.Equals("FechaDesbaneo", StringComparison.OrdinalIgnoreCase)) hasFechaColumn = true;
                        }
                    }
                }
                
                if (!hasMotivoColumn)
                {
                    await _context.Database.ExecuteSqlRawAsync("ALTER TABLE usuario ADD COLUMN MotivoBaneo VARCHAR(500) NULL;");
                }
                if (!hasFechaColumn)
                {
                    await _context.Database.ExecuteSqlRawAsync("ALTER TABLE usuario ADD COLUMN FechaDesbaneo DATETIME NULL;");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar/crear columnas de baneo: {ex.Message}");
            }
        }

        public async Task<IEnumerable<object>> GetUsuariosAsync()
        {
            await EnsureReportesTableExists();

            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    id = u.IdUsuarios,
                    nombre = u.Nombre,
                    apellido = u.Apellido,
                    correo = u.Correo,
                    rol = u.Rol,
                    estado = u.Estado,
                    fechaRegistro = u.FechaRegistro,
                    foto = u.ImgPerfil,
                    motivoBaneo = u.MotivoBaneo,
                    fechaDesbaneo = u.FechaDesbaneo
                })
                .ToListAsync();

            return usuarios;
        }

        public async Task<(bool exito, string mensaje)> CambiarEstadoUsuarioAsync(int idUsuario, int nuevoEstado)
        {
            await EnsureReportesTableExists();

            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            usuario.Estado = (sbyte)nuevoEstado;
            if (nuevoEstado == 1)
            {
                usuario.MotivoBaneo = null;
                usuario.FechaDesbaneo = null;
            }

            await _context.SaveChangesAsync();
            return (true, $"Estado del usuario actualizado a {nuevoEstado}");
        }

        public async Task<(bool exito, string mensaje)> BanearUsuarioAsync(int idUsuario, BanDto dto)
        {
            await EnsureReportesTableExists();

            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            usuario.Estado = 0;
            usuario.MotivoBaneo = dto.Motivo;
            usuario.FechaDesbaneo = DateTime.Now.AddDays(dto.Dias);

            var listings = await _context.InventarioUsuarios.Where(i => i.IdUsuario == idUsuario).ToListAsync();
            if (listings.Any())
            {
                _context.InventarioUsuarios.RemoveRange(listings);
            }

            var msgs = await _context.Mensajes
                .Where(m => m.IdRemitente == idUsuario || m.IdDestinatario == idUsuario)
                .ToListAsync();
            if (msgs.Any())
            {
                _context.Mensajes.RemoveRange(msgs);
            }

            await _context.SaveChangesAsync();
            return (true, $"Usuario baneado por {dto.Dias} días.");
        }

        public async Task<(bool exito, string mensaje)> EliminarUsuarioAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            await EnsureReportesTableExists();

            var guardados = await _context.Guardados.Where(g => g.IdUsuario == idUsuario).ToListAsync();
            _context.Guardados.RemoveRange(guardados);

            var invUsuarios = await _context.InventarioUsuarios.Where(i => i.IdUsuario == idUsuario).ToListAsync();
            _context.InventarioUsuarios.RemoveRange(invUsuarios);

            var mensajes = await _context.Mensajes
                .Where(m => m.IdRemitente == idUsuario || m.IdDestinatario == idUsuario)
                .ToListAsync();
            _context.Mensajes.RemoveRange(mensajes);

            var notifs = await _context.Notificaciones.Where(n => n.IdUserDestinatario == idUsuario).ToListAsync();
            _context.Notificaciones.RemoveRange(notifs);

            var reviews = await _context.Reseñas.Where(r => r.IdUsuarioReseñador == idUsuario || r.IdUsuarioReseñado == idUsuario).ToListAsync();
            _context.Reseñas.RemoveRange(reviews);

            var reports = await _context.Reportes
                .Where(r => r.IdUsuarioReportante == idUsuario || r.IdUsuarioReportado == idUsuario)
                .ToListAsync();
            _context.Reportes.RemoveRange(reports);

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return (true, "Usuario eliminado con éxito de la base de datos");
        }

        public async Task<(bool exito, string mensaje)> CambiarRolUsuarioAsync(int idUsuario, string nuevoRol)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            if (nuevoRol != "Administrador" && nuevoRol != "Usuario")
            {
                return (false, "Rol no válido. Debe ser 'Administrador' o 'Usuario'.");
            }

            usuario.Rol = nuevoRol;

            if (nuevoRol == "Administrador")
            {
                var listings = await _context.InventarioUsuarios.Where(i => i.IdUsuario == idUsuario).ToListAsync();
                if (listings.Any())
                {
                    _context.InventarioUsuarios.RemoveRange(listings);
                }

                var msgs = await _context.Mensajes
                    .Where(m => m.IdRemitente == idUsuario || m.IdDestinatario == idUsuario)
                    .ToListAsync();
                if (msgs.Any())
                {
                    _context.Mensajes.RemoveRange(msgs);
                }
            }

            await _context.SaveChangesAsync();
            return (true, $"Rol del usuario actualizado a {nuevoRol}");
        }

        public async Task<IEnumerable<object>> GetArticulosMercadoAsync()
        {
            var articulos = await _context.InventarioUsuarios
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdItemNavigation)
                .Select(i => new {
                    idInventarioUser = i.IdInventarioUser,
                    idUsuario = i.IdUsuario,
                    vendedorNombre = i.IdUsuarioNavigation.Nombre + " " + i.IdUsuarioNavigation.Apellido,
                    vendedorCorreo = i.IdUsuarioNavigation.Correo,
                    idItem = i.IdItem,
                    nombreCarta = i.IdItemNavigation.Nombre,
                    rareza = i.IdItemNavigation.Rareza,
                    precio = i.IdItemNavigation.precio,
                    edicion = i.IdItemNavigation.Edicion,
                    imgLink = i.IdItemNavigation.ImgLink,
                    estadoFisico = i.EstadoFisico,
                    cantidad = i.Cantidad,
                    fechaObtencion = i.FechaObtencion
                })
                .ToListAsync();
            return articulos;
        }

        public async Task<(bool exito, string mensaje)> EliminarArticuloMercadoAsync(int idInventarioUser)
        {
            var listing = await _context.InventarioUsuarios.FindAsync(idInventarioUser);
            if (listing == null)
            {
                return (false, "Artículo de inventario no encontrado");
            }

            _context.InventarioUsuarios.Remove(listing);
            await _context.SaveChangesAsync();
            return (true, "Artículo eliminado del inventario con éxito");
        }

        public async Task<(bool exito, string mensaje)> CrearReporteAsync(ReporteDto dto)
        {
            await EnsureReportesTableExists();

            var reporte = new Reporte
            {
                IdUsuarioReportante = dto.IdUsuarioReportante,
                IdUsuarioReportado = dto.IdUsuarioReportado,
                Motivo = dto.Motivo,
                Fecha = DateTime.Now,
                Estado = "Pendiente"
            };

            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();
            return (true, "Reporte enviado con éxito.");
        }

        public async Task<IEnumerable<object>> GetReportesAsync()
        {
            await EnsureReportesTableExists();

            var reportes = await _context.Reportes
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            var resultado = new List<object>();
            foreach (var r in reportes)
            {
                var reportante = await _context.Usuarios.FindAsync(r.IdUsuarioReportante);
                var reportado = await _context.Usuarios.FindAsync(r.IdUsuarioReportado);

                resultado.Add(new
                {
                    idReporte = r.IdReporte,
                    idUsuarioReportante = r.IdUsuarioReportante,
                    reportanteNombre = reportante != null ? $"{reportante.Nombre} {reportante.Apellido}" : "Usuario Eliminado",
                    reportanteCorreo = reportante?.Correo ?? "",
                    idUsuarioReportado = r.IdUsuarioReportado,
                    reportadoNombre = reportado != null ? $"{reportado.Nombre} {reportado.Apellido}" : "Usuario Eliminado",
                    reportadoCorreo = reportado?.Correo ?? "",
                    reportadoEstado = reportado?.Estado ?? 1,
                    reportadoMotivoBaneo = reportado?.MotivoBaneo ?? "",
                    reportadoFechaDesbaneo = reportado?.FechaDesbaneo,
                    motivo = r.Motivo,
                    fecha = r.Fecha,
                    estado = r.Estado
                });
            }

            return resultado;
        }

        public async Task<(bool exito, string mensaje)> EliminarReporteAsync(int idReporte)
        {
            await EnsureReportesTableExists();

            var reporte = await _context.Reportes.FindAsync(idReporte);
            if (reporte == null)
            {
                return (false, "Reporte no encontrado");
            }

            _context.Reportes.Remove(reporte);
            await _context.SaveChangesAsync();
            return (true, "Reporte desestimado con éxito");
        }
    }
}
