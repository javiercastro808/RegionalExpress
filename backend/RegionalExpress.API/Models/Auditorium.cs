using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Auditorium
{
    public int IdAuditoria { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public string Entidad { get; set; } = null!;

    public int? IdRegistro { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public string? Descripcion { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
