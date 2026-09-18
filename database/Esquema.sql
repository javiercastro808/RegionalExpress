CREATE TABLE [EstadosServicio] (
    [IdEstado] int NOT NULL IDENTITY,
    [TipoServicio] varchar(20) NOT NULL,
    [Modalidad] varchar(30) NULL,
    [NombreEstado] varchar(100) NOT NULL,
    [OrdenEstado] int NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__EstadosS__FBB0EDC108053FD4] PRIMARY KEY ([IdEstado])
);
GO


CREATE TABLE [Restaurantes] (
    [IdRestaurante] int NOT NULL IDENTITY,
    [Nombre] varchar(150) NOT NULL,
    [Descripcion] varchar(500) NULL,
    [Direccion] varchar(250) NOT NULL,
    [Telefono] varchar(20) NULL,
    [Correo] varchar(150) NULL,
    [Imagen] varchar(500) NULL,
    [HorarioApertura] time NULL,
    [HorarioCierre] time NULL,
    [Latitud] decimal(10,7) NULL,
    [Longitud] decimal(10,7) NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaRegistro] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Restaura__29CE64FABA44B441] PRIMARY KEY ([IdRestaurante])
);
GO


CREATE TABLE [Roles] (
    [IdRol] int NOT NULL IDENTITY,
    [NombreRol] varchar(50) NOT NULL,
    [Descripcion] varchar(200) NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__Roles__2A49584CD2B9185F] PRIMARY KEY ([IdRol])
);
GO


CREATE TABLE [Tarifas] (
    [IdTarifa] int NOT NULL IDENTITY,
    [TipoServicio] varchar(20) NOT NULL,
    [Nombre] varchar(100) NOT NULL,
    [TarifaBase] decimal(10,2) NOT NULL,
    [CostoPorKm] decimal(10,2) NOT NULL,
    [CostoPorLibra] decimal(10,2) NOT NULL,
    [MontoMinimo] decimal(10,2) NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaInicio] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    [FechaFin] datetime2 NULL,
    CONSTRAINT [PK__Tarifas__78F1A91D62FBD366] PRIMARY KEY ([IdTarifa])
);
GO


CREATE TABLE [CategoriasMenu] (
    [IdCategoria] int NOT NULL IDENTITY,
    [IdRestaurante] int NOT NULL,
    [Nombre] varchar(100) NOT NULL,
    [Descripcion] varchar(300) NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__Categori__A3C02A102B3DF499] PRIMARY KEY ([IdCategoria]),
    CONSTRAINT [FK_Categorias_Restaurantes] FOREIGN KEY ([IdRestaurante]) REFERENCES [Restaurantes] ([IdRestaurante])
);
GO


CREATE TABLE [Usuarios] (
    [IdUsuario] int NOT NULL IDENTITY,
    [IdRol] int NOT NULL,
    [Nombre] varchar(100) NOT NULL,
    [Apellido] varchar(100) NOT NULL,
    [Correo] varchar(150) NOT NULL,
    [Telefono] varchar(20) NULL,
    [PasswordHash] varchar(500) NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaRegistro] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Usuarios__5B65BF9700A6E884] PRIMARY KEY ([IdUsuario]),
    CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([IdRol])
);
GO


CREATE TABLE [Productos] (
    [IdProducto] int NOT NULL IDENTITY,
    [IdCategoria] int NOT NULL,
    [Nombre] varchar(150) NOT NULL,
    [Descripcion] varchar(500) NULL,
    [Precio] decimal(10,2) NOT NULL,
    [Imagen] varchar(500) NULL,
    [Disponible] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaRegistro] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Producto__09889210005BC169] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_Productos_Categorias] FOREIGN KEY ([IdCategoria]) REFERENCES [CategoriasMenu] ([IdCategoria])
);
GO


CREATE TABLE [Auditoria] (
    [IdAuditoria] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [Accion] varchar(100) NOT NULL,
    [Entidad] varchar(100) NOT NULL,
    [IdRegistro] int NULL,
    [ValorAnterior] varchar(max) NULL,
    [ValorNuevo] varchar(max) NULL,
    [Descripcion] varchar(500) NULL,
    [FechaHora] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Auditori__7FD13FA00B074016] PRIMARY KEY ([IdAuditoria]),
    CONSTRAINT [FK_Auditoria_Usuarios] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario])
);
GO


CREATE TABLE [DireccionesUsuario] (
    [IdDireccion] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [NombreDireccion] varchar(50) NULL,
    [Direccion] varchar(250) NOT NULL,
    [Referencia] varchar(250) NULL,
    [Latitud] decimal(10,7) NULL,
    [Longitud] decimal(10,7) NULL,
    [Predeterminada] bit NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__Direccio__1F8E0C76055F218E] PRIMARY KEY ([IdDireccion]),
    CONSTRAINT [FK_Direcciones_Usuarios] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario])
);
GO


CREATE TABLE [Motoristas] (
    [IdMotorista] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [NumeroLicencia] varchar(50) NULL,
    [Disponible] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaRegistro] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Motorist__70C5824B2AF32346] PRIMARY KEY ([IdMotorista]),
    CONSTRAINT [FK_Motoristas_Usuarios] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario])
);
GO


CREATE TABLE [UsuarioRestaurante] (
    [IdUsuarioRestaurante] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [IdRestaurante] int NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaAsignacion] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__UsuarioR__869F1F9977DB42EE] PRIMARY KEY ([IdUsuarioRestaurante]),
    CONSTRAINT [FK_UsuarioRestaurante_Restaurante] FOREIGN KEY ([IdRestaurante]) REFERENCES [Restaurantes] ([IdRestaurante]),
    CONSTRAINT [FK_UsuarioRestaurante_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario])
);
GO


CREATE TABLE [Servicios] (
    [IdServicio] int NOT NULL IDENTITY,
    [CodigoRastreo] varchar(30) NOT NULL,
    [IdCliente] int NOT NULL,
    [IdMotorista] int NULL,
    [TipoServicio] varchar(20) NOT NULL,
    [IdEstadoActual] int NOT NULL,
    [Total] decimal(10,2) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    [FechaFinalizacion] datetime2 NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__Servicio__2DCCF9A2B333C200] PRIMARY KEY ([IdServicio]),
    CONSTRAINT [FK_Servicios_Clientes] FOREIGN KEY ([IdCliente]) REFERENCES [Usuarios] ([IdUsuario]),
    CONSTRAINT [FK_Servicios_Estados] FOREIGN KEY ([IdEstadoActual]) REFERENCES [EstadosServicio] ([IdEstado]),
    CONSTRAINT [FK_Servicios_Motoristas] FOREIGN KEY ([IdMotorista]) REFERENCES [Motoristas] ([IdMotorista])
);
GO


CREATE TABLE [Vehiculos] (
    [IdVehiculo] int NOT NULL IDENTITY,
    [IdMotorista] int NOT NULL,
    [TipoVehiculo] varchar(50) NOT NULL,
    [Marca] varchar(100) NULL,
    [Modelo] varchar(100) NULL,
    [Placa] varchar(30) NULL,
    [Color] varchar(50) NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__Vehiculo__70861215E0640977] PRIMARY KEY ([IdVehiculo]),
    CONSTRAINT [FK_Vehiculos_Motoristas] FOREIGN KEY ([IdMotorista]) REFERENCES [Motoristas] ([IdMotorista])
);
GO


CREATE TABLE [Envios] (
    [IdEnvio] int NOT NULL IDENTITY,
    [IdServicio] int NOT NULL,
    [NombreRemitente] varchar(150) NOT NULL,
    [TelefonoRemitente] varchar(20) NOT NULL,
    [CorreoRemitente] varchar(150) NULL,
    [NombreDestinatario] varchar(150) NOT NULL,
    [TelefonoDestinatario] varchar(20) NOT NULL,
    [CorreoDestinatario] varchar(150) NULL,
    [DireccionOrigen] varchar(250) NOT NULL,
    [ReferenciaOrigen] varchar(250) NULL,
    [LatitudOrigen] decimal(10,7) NULL,
    [LongitudOrigen] decimal(10,7) NULL,
    [DireccionDestino] varchar(250) NOT NULL,
    [ReferenciaDestino] varchar(250) NULL,
    [LatitudDestino] decimal(10,7) NULL,
    [LongitudDestino] decimal(10,7) NULL,
    [Peso] decimal(10,2) NOT NULL,
    [TipoPaquete] varchar(100) NOT NULL,
    [DescripcionPaquete] varchar(500) NULL,
    [TarifaBase] decimal(10,2) NOT NULL,
    [CostoDistancia] decimal(10,2) NOT NULL,
    [CostoPeso] decimal(10,2) NOT NULL,
    [Total] decimal(10,2) NOT NULL,
    [FechaSolicitud] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Envios__B814A62E0DB0EC9B] PRIMARY KEY ([IdEnvio]),
    CONSTRAINT [FK_Envios_Servicios] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO


CREATE TABLE [HistorialEstados] (
    [IdHistorial] int NOT NULL IDENTITY,
    [IdServicio] int NOT NULL,
    [IdEstado] int NOT NULL,
    [IdUsuario] int NOT NULL,
    [FechaHora] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    [Observacion] varchar(500) NULL,
    CONSTRAINT [PK__Historia__9CC7DBB4AF165E5D] PRIMARY KEY ([IdHistorial]),
    CONSTRAINT [FK_HistorialEstados_Estados] FOREIGN KEY ([IdEstado]) REFERENCES [EstadosServicio] ([IdEstado]),
    CONSTRAINT [FK_HistorialEstados_Servicios] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio]),
    CONSTRAINT [FK_HistorialEstados_Usuarios] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario])
);
GO


CREATE TABLE [Pedidos] (
    [IdPedido] int NOT NULL IDENTITY,
    [IdServicio] int NOT NULL,
    [IdRestaurante] int NOT NULL,
    [ModalidadEntrega] varchar(30) NOT NULL,
    [DireccionEntrega] varchar(250) NULL,
    [ReferenciaEntrega] varchar(250) NULL,
    [LatitudEntrega] decimal(10,7) NULL,
    [LongitudEntrega] decimal(10,7) NULL,
    [CostoProductos] decimal(10,2) NOT NULL,
    [CostoDelivery] decimal(10,2) NOT NULL,
    [Total] decimal(10,2) NOT NULL,
    [Observaciones] varchar(500) NULL,
    [FechaPedido] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Pedidos__9D335DC33952CC2F] PRIMARY KEY ([IdPedido]),
    CONSTRAINT [FK_Pedidos_Restaurantes] FOREIGN KEY ([IdRestaurante]) REFERENCES [Restaurantes] ([IdRestaurante]),
    CONSTRAINT [FK_Pedidos_Servicios] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO


CREATE TABLE [UbicacionesMotorista] (
    [IdUbicacion] int NOT NULL IDENTITY,
    [IdServicio] int NOT NULL,
    [IdMotorista] int NOT NULL,
    [Latitud] decimal(10,7) NOT NULL,
    [Longitud] decimal(10,7) NOT NULL,
    [FechaHora] datetime2 NOT NULL DEFAULT ((sysdatetime())),
    CONSTRAINT [PK__Ubicacio__778CAB1D03D706EB] PRIMARY KEY ([IdUbicacion]),
    CONSTRAINT [FK_Ubicaciones_Motoristas] FOREIGN KEY ([IdMotorista]) REFERENCES [Motoristas] ([IdMotorista]),
    CONSTRAINT [FK_Ubicaciones_Servicios] FOREIGN KEY ([IdServicio]) REFERENCES [Servicios] ([IdServicio])
);
GO


CREATE TABLE [DetallePedido] (
    [IdDetallePedido] int NOT NULL IDENTITY,
    [IdPedido] int NOT NULL,
    [IdProducto] int NOT NULL,
    [Cantidad] int NOT NULL,
    [PrecioUnitario] decimal(10,2) NOT NULL,
    [Subtotal] decimal(10,2) NOT NULL,
    CONSTRAINT [PK__DetalleP__48AFFD95AE3EBAA0] PRIMARY KEY ([IdDetallePedido]),
    CONSTRAINT [FK_DetallePedido_Pedidos] FOREIGN KEY ([IdPedido]) REFERENCES [Pedidos] ([IdPedido]),
    CONSTRAINT [FK_DetallePedido_Productos] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto])
);
GO


CREATE INDEX [IX_Auditoria_IdUsuario] ON [Auditoria] ([IdUsuario]);
GO


CREATE UNIQUE INDEX [UQ_CategoriasMenu_Restaurante_Nombre] ON [CategoriasMenu] ([IdRestaurante], [Nombre]);
GO


CREATE INDEX [IX_DetallePedido_IdPedido] ON [DetallePedido] ([IdPedido]);
GO


CREATE INDEX [IX_DetallePedido_IdProducto] ON [DetallePedido] ([IdProducto]);
GO


CREATE INDEX [IX_DireccionesUsuario_IdUsuario] ON [DireccionesUsuario] ([IdUsuario]);
GO


CREATE UNIQUE INDEX [UQ__Envios__2DCCF9A3269FB593] ON [Envios] ([IdServicio]);
GO


CREATE UNIQUE INDEX [UX_EstadosServicio_Unico] ON [EstadosServicio] ([TipoServicio], [Modalidad], [NombreEstado], [OrdenEstado]) WHERE [Modalidad] IS NOT NULL;
GO


CREATE INDEX [IX_HistorialEstados_IdEstado] ON [HistorialEstados] ([IdEstado]);
GO


CREATE INDEX [IX_HistorialEstados_IdUsuario] ON [HistorialEstados] ([IdUsuario]);
GO


CREATE UNIQUE INDEX [UQ_HistorialEstados_Servicio_Estado] ON [HistorialEstados] ([IdServicio], [IdEstado]);
GO


CREATE UNIQUE INDEX [UQ_Motoristas_Usuario] ON [Motoristas] ([IdUsuario]);
GO


CREATE INDEX [IX_Pedidos_IdRestaurante] ON [Pedidos] ([IdRestaurante]);
GO


CREATE UNIQUE INDEX [UQ__Pedidos__2DCCF9A3FAAE56D0] ON [Pedidos] ([IdServicio]);
GO


CREATE INDEX [IX_Productos_IdCategoria] ON [Productos] ([IdCategoria]);
GO


CREATE UNIQUE INDEX [UQ__Roles__4F0B537F3A3B42AB] ON [Roles] ([NombreRol]);
GO


CREATE INDEX [IX_Servicios_IdCliente] ON [Servicios] ([IdCliente]);
GO


CREATE INDEX [IX_Servicios_IdEstadoActual] ON [Servicios] ([IdEstadoActual]);
GO


CREATE INDEX [IX_Servicios_IdMotorista] ON [Servicios] ([IdMotorista]);
GO


CREATE UNIQUE INDEX [UQ__Servicio__9A41B285E8F416D3] ON [Servicios] ([CodigoRastreo]);
GO


CREATE INDEX [IX_UbicacionesMotorista_IdMotorista] ON [UbicacionesMotorista] ([IdMotorista]);
GO


CREATE INDEX [IX_UbicacionesMotorista_IdServicio] ON [UbicacionesMotorista] ([IdServicio]);
GO


CREATE INDEX [IX_UsuarioRestaurante_IdRestaurante] ON [UsuarioRestaurante] ([IdRestaurante]);
GO


CREATE UNIQUE INDEX [UQ_UsuarioRestaurante] ON [UsuarioRestaurante] ([IdUsuario], [IdRestaurante]);
GO


CREATE INDEX [IX_Usuarios_IdRol] ON [Usuarios] ([IdRol]);
GO


CREATE UNIQUE INDEX [UQ__Usuarios__60695A191EA04E0E] ON [Usuarios] ([Correo]);
GO


CREATE INDEX [IX_Vehiculos_IdMotorista] ON [Vehiculos] ([IdMotorista]);
GO


CREATE UNIQUE INDEX [UX_Vehiculos_Placa] ON [Vehiculos] ([Placa]) WHERE ([Placa] IS NOT NULL);
GO



