using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Backend.Models;

public partial class PokemonMarketContext : DbContext
{
    public PokemonMarketContext()
    {
    }

    public PokemonMarketContext(DbContextOptions<PokemonMarketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Guardado> Guardados { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<InventarioUsuario> InventarioUsuarios { get; set; }

    public virtual DbSet<Mensaje> Mensajes { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Reseña> Reseñas { get; set; }

    public virtual DbSet<TopRegistro> TopRegistros { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Guardado>(entity =>
        {
            entity.HasKey(e => e.IdLista).HasName("PRIMARY");

            entity.ToTable("guardados");

            entity.HasIndex(e => e.IdItem, "FK_Guardados_Inventario");

            entity.HasIndex(e => e.IdUsuario, "FK_Guardados_Usuario");

            entity.Property(e => e.IdLista).HasColumnName("ID_Lista");
            entity.Property(e => e.FechaGuardado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Guardado");
            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Guardados)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK_Guardados_Inventario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Guardados)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Guardados_Usuario");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PRIMARY");

            entity.ToTable("inventario");

            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.Edicion).HasMaxLength(100);
            entity.Property(e => e.precio);
            entity.Property(e => e.id_tgc).HasMaxLength(20);
            entity.Property(e => e.ImgLink)
                .HasMaxLength(500)
                .HasColumnName("IMG_Link");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Rareza).HasMaxLength(50);
        });

        modelBuilder.Entity<InventarioUsuario>(entity =>
        {
            entity.HasKey(e => e.IdInventarioUser).HasName("PRIMARY");

            entity.ToTable("inventario_usuario");

            entity.HasIndex(e => e.IdItem, "FK_InvUser_Item");

            entity.HasIndex(e => e.IdUsuario, "FK_InvUser_Usuario");

            entity.Property(e => e.IdInventarioUser).HasColumnName("ID_Inventario_User");
            entity.Property(e => e.Cantidad).HasDefaultValueSql("'1'");
            entity.Property(e => e.EstadoFisico)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Perfecto Estado'")
                .HasColumnName("Estado_Fisico");
            entity.Property(e => e.FechaObtencion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Obtencion");
            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.InventarioUsuarios)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK_InvUser_Item");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InventarioUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_InvUser_Usuario");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.HasKey(e => e.IdMensaje).HasName("PRIMARY");

            entity.ToTable("mensajes");

            entity.HasIndex(e => e.IdDestinatario, "FK_Mensaje_Destinatario");

            entity.HasIndex(e => e.IdItem, "FK_Mensaje_Item");

            entity.HasIndex(e => e.IdRemitente, "FK_Mensaje_Remitente");

            entity.Property(e => e.IdMensaje).HasColumnName("ID_Mensaje");
            entity.Property(e => e.Estado).HasDefaultValueSql("'0'");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.IdDestinatario).HasColumnName("ID_Destinatario");
            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.IdRemitente).HasColumnName("ID_Remitente");
            entity.Property(e => e.Texto).HasColumnType("text");

            entity.HasOne(d => d.IdDestinatarioNavigation).WithMany(p => p.MensajeIdDestinatarioNavigations)
                .HasForeignKey(d => d.IdDestinatario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mensaje_Destinatario");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Mensajes)
                .HasForeignKey(d => d.IdItem)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Mensaje_Item");

            entity.HasOne(d => d.IdRemitenteNavigation).WithMany(p => p.MensajeIdRemitenteNavigations)
                .HasForeignKey(d => d.IdRemitente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mensaje_Remitente");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdNotificaciones).HasName("PRIMARY");

            entity.ToTable("notificaciones");

            entity.HasIndex(e => e.IdUserDestinatario, "FK_Notificacion_Usuario");

            entity.Property(e => e.IdNotificaciones).HasColumnName("ID_Notificaciones");
            entity.Property(e => e.Asunto).HasMaxLength(100);
            entity.Property(e => e.Contenido).HasColumnType("text");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.IdUserDestinatario).HasColumnName("ID_User_Destinatario");
            entity.Property(e => e.Leido).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.IdUserDestinatarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUserDestinatario)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        modelBuilder.Entity<Reseña>(entity =>
        {
            entity.HasKey(e => e.ReseñaId).HasName("PRIMARY");

            entity.ToTable("reseñas");

            entity.HasIndex(e => e.IdItem, "FK_Reseña_Item");

            entity.HasIndex(e => e.IdUsuario, "FK_Reseña_Usuario");

            entity.Property(e => e.ReseñaId).HasColumnName("Reseña_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Texto).HasColumnType("text");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK_Reseña_Item");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reseña_Usuario");
        });

        modelBuilder.Entity<TopRegistro>(entity =>
        {
            entity.HasKey(e => e.IdTop).HasName("PRIMARY");

            entity.ToTable("top_registros");

            entity.HasIndex(e => e.IdItem, "FK_Top_Item");

            entity.Property(e => e.IdTop).HasColumnName("ID_Top");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Registro");
            entity.Property(e => e.IdItem).HasColumnName("ID_Item");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.TopRegistros)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK_Top_Item");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuarios).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Correo, "Correo").IsUnique();

            entity.Property(e => e.IdUsuarios).HasColumnName("ID_Usuarios");
            entity.Property(e => e.Apellido).HasMaxLength(100);  
            entity.Property(e => e.Calificacion)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("'5.00'");
            entity.Property(e => e.CodigoRecuperacion).HasMaxLength(6);
            entity.Property(e => e.CodigoVerificacion).HasMaxLength(6);
            entity.Property(e => e.Contraseña).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.EsVerificado).HasDefaultValueSql("'0'");
            entity.Property(e => e.Estado).HasDefaultValueSql("'1'");
            entity.Property(e => e.EstadoPresencia).HasDefaultValueSql("'1'");
            entity.Property(e => e.FechaRegistro)  
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Registro");
            entity.Property(e => e.ImgPerfil).HasColumnName("IMG_Perfil");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Usuario'");
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
