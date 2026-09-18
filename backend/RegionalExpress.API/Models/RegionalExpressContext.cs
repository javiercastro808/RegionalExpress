using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RegionalExpress.API.Models;

public partial class RegionalExpressContext : DbContext
{
    public RegionalExpressContext()
    {
    }

    public RegionalExpressContext(DbContextOptions<RegionalExpressContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<CategoriasMenu> CategoriasMenus { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<DireccionesUsuario> DireccionesUsuarios { get; set; }

    public virtual DbSet<Envio> Envios { get; set; }

    public virtual DbSet<EstadosServicio> EstadosServicios { get; set; }

    public virtual DbSet<HistorialEstado> HistorialEstados { get; set; }

    public virtual DbSet<Motorista> Motoristas { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Restaurante> Restaurantes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<Tarifa> Tarifas { get; set; }

    public virtual DbSet<UbicacionesMotoristum> UbicacionesMotorista { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRestaurante> UsuarioRestaurantes { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PK__Auditori__7FD13FA00B074016");

            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Entidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ValorAnterior).IsUnicode(false);
            entity.Property(e => e.ValorNuevo).IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Usuarios");
        });

        modelBuilder.Entity<CategoriasMenu>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__A3C02A102B3DF499");

            entity.ToTable("CategoriasMenu");

            entity.HasIndex(e => new { e.IdRestaurante, e.Nombre }, "UQ_CategoriasMenu_Restaurante_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRestauranteNavigation).WithMany(p => p.CategoriasMenus)
                .HasForeignKey(d => d.IdRestaurante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categorias_Restaurantes");
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.IdDetallePedido).HasName("PK__DetalleP__48AFFD95AE3EBAA0");

            entity.ToTable("DetallePedido");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Pedidos");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Productos");
        });

        modelBuilder.Entity<DireccionesUsuario>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__Direccio__1F8E0C76055F218E");

            entity.ToTable("DireccionesUsuario");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Latitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.NombreDireccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Referencia)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.DireccionesUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Direcciones_Usuarios");
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasKey(e => e.IdEnvio).HasName("PK__Envios__B814A62E0DB0EC9B");

            entity.HasIndex(e => e.IdServicio, "UQ__Envios__2DCCF9A3269FB593").IsUnique();

            entity.Property(e => e.CorreoDestinatario)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CorreoRemitente)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CostoDistancia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CostoPeso).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DescripcionPaquete)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DireccionDestino)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DireccionOrigen)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LatitudDestino).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.LatitudOrigen).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.LongitudDestino).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.LongitudOrigen).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.NombreDestinatario)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.NombreRemitente)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Peso).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReferenciaDestino)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ReferenciaOrigen)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.TarifaBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TelefonoDestinatario)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoRemitente)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoPaquete)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdServicioNavigation).WithOne(p => p.Envio)
                .HasForeignKey<Envio>(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Envios_Servicios");
        });

        modelBuilder.Entity<EstadosServicio>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__EstadosS__FBB0EDC108053FD4");

            entity.ToTable("EstadosServicio");

            entity.HasIndex(e => new { e.TipoServicio, e.Modalidad, e.NombreEstado, e.OrdenEstado }, "UX_EstadosServicio_Unico").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Modalidad)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HistorialEstado>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PK__Historia__9CC7DBB4AF165E5D");

            entity.HasIndex(e => new { e.IdServicio, e.IdEstado }, "UQ_HistorialEstados_Servicio_Estado").IsUnique();

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Observacion)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstados_Estados");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstados_Servicios");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialEstados)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialEstados_Usuarios");
        });

        modelBuilder.Entity<Motorista>(entity =>
        {
            entity.HasKey(e => e.IdMotorista).HasName("PK__Motorist__70C5824B2AF32346");

            entity.HasIndex(e => e.IdUsuario, "UQ_Motoristas_Usuario").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Disponible).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NumeroLicencia)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Motorista)
                .HasForeignKey<Motorista>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Motoristas_Usuarios");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK__Pedidos__9D335DC33952CC2F");

            entity.HasIndex(e => e.IdServicio, "UQ__Pedidos__2DCCF9A3FAAE56D0").IsUnique();

            entity.Property(e => e.CostoDelivery).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CostoProductos).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DireccionEntrega)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaPedido).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.LatitudEntrega).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.LongitudEntrega).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.ModalidadEntrega)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ReferenciaEntrega)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdRestauranteNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdRestaurante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_Restaurantes");

            entity.HasOne(d => d.IdServicioNavigation).WithOne(p => p.Pedido)
                .HasForeignKey<Pedido>(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedidos_Servicios");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__09889210005BC169");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Disponible).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Imagen)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Categorias");
        });

        modelBuilder.Entity<Restaurante>(entity =>
        {
            entity.HasKey(e => e.IdRestaurante).HasName("PK__Restaura__29CE64FABA44B441");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Imagen)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Latitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584CD2B9185F");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__4F0B537F3A3B42AB").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__2DCCF9A2B333C200");

            entity.HasIndex(e => e.CodigoRastreo, "UQ__Servicio__9A41B285E8F416D3").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CodigoRastreo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Servicios_Clientes");

            entity.HasOne(d => d.IdEstadoActualNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdEstadoActual)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Servicios_Estados");

            entity.HasOne(d => d.IdMotoristaNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdMotorista)
                .HasConstraintName("FK_Servicios_Motoristas");
        });

        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.HasKey(e => e.IdTarifa).HasName("PK__Tarifas__78F1A91D62FBD366");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CostoPorKm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CostoPorLibra).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MontoMinimo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TarifaBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UbicacionesMotoristum>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion).HasName("PK__Ubicacio__778CAB1D03D706EB");

            entity.Property(e => e.FechaHora).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Latitud).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(10, 7)");

            entity.HasOne(d => d.IdMotoristaNavigation).WithMany(p => p.UbicacionesMotorista)
                .HasForeignKey(d => d.IdMotorista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicaciones_Motoristas");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.UbicacionesMotorista)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicaciones_Servicios");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF9700A6E884");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A191EA04E0E").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<UsuarioRestaurante>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRestaurante).HasName("PK__UsuarioR__869F1F9977DB42EE");

            entity.ToTable("UsuarioRestaurante");

            entity.HasIndex(e => new { e.IdUsuario, e.IdRestaurante }, "UQ_UsuarioRestaurante").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.IdRestauranteNavigation).WithMany(p => p.UsuarioRestaurantes)
                .HasForeignKey(d => d.IdRestaurante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRestaurante_Restaurante");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRestaurantes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRestaurante_Usuario");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.IdVehiculo).HasName("PK__Vehiculo__70861215E0640977");

            entity.HasIndex(e => e.Placa, "UX_Vehiculos_Placa")
                .IsUnique()
                .HasFilter("([Placa] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Modelo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Placa)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TipoVehiculo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMotoristaNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdMotorista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehiculos_Motoristas");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
