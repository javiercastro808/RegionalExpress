import { API_URL } from './api-url';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Api {

  private readonly apiUrl =
    API_URL;

  constructor(
    private http: HttpClient
  ) {}

  // ==========================================================
  // CLIENTE - RESTAURANTES
  // ==========================================================

  getRestaurantes(): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Restaurantes`
    );
  }

  getRestaurante(
    id: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Restaurantes/${id}`
    );
  }

  // ==========================================================
  // CLIENTE - PRODUCTOS
  // ==========================================================

  getProductos(): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Productos`
    );
  }

  getProductosPorCategoria(
    idCategoria: number
  ): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Productos/categoria/${idCategoria}`
    );
  }

  // ==========================================================
  // CLIENTE - RASTREO
  // ==========================================================

  rastrear(
    codigo: string
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Rastreo/${codigo}`
    );
  }

  // ==========================================================
  // CLIENTE - PEDIDOS
  // ==========================================================

  crearPedido(
    pedido: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/Pedidos`,
      pedido
    );
  }

  // ==========================================================
  // CLIENTE - ENVÍOS
  // ==========================================================

  crearEnvio(
    envio: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/Envios`,
      envio
    );
  }

  calcularEnvio(
    peso: number,
    distanciaKm: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Envios/calcular`,
      {
        params: {
          peso: peso.toString(),
          distanciaKm:
            distanciaKm.toString()
        }
      }
    );
  }

  // ==========================================================
  // ADMIN GENERAL - DASHBOARD
  // ==========================================================

  getAdminDashboard():
    Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Admin/dashboard`
    );
  }

  // ==========================================================
  // ADMIN GENERAL - USUARIOS
  // ==========================================================

  getAdminUsuarios():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/usuarios`
    );
  }

  getAdminRoles():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/roles`
    );
  }

  crearAdminUsuario(
    usuario: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/Admin/usuarios`,
      usuario
    );
  }

  editarAdminUsuario(
    id: number,
    usuario: any
  ): Observable<any> {

    return this.http.put<any>(
      `${this.apiUrl}/Admin/usuarios/${id}`,
      usuario
    );
  }

  cambiarEstadoAdminUsuario(
    id: number,
    activo: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Admin/usuarios/${id}/estado`,
      {
        activo
      }
    );
  }

  // ==========================================================
  // ADMIN GENERAL - SERVICIOS
  // ==========================================================

  getAdminServicios():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/servicios`
    );
  }

  // ==========================================================
  // ADMIN GENERAL - RESTAURANTES
  // ==========================================================

  getAdminRestaurantes():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/restaurantes`
    );
  }

  getAdminRestaurante(
    id: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Admin/restaurantes/${id}`
    );
  }

  crearAdminRestaurante(
    restaurante: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/Admin/restaurantes`,
      restaurante
    );
  }

  editarAdminRestaurante(
    id: number,
    restaurante: any
  ): Observable<any> {

    return this.http.put<any>(
      `${this.apiUrl}/Admin/restaurantes/${id}`,
      restaurante
    );
  }

  cambiarEstadoAdminRestaurante(
    id: number,
    activo: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Admin/restaurantes/${id}/estado`,
      {
        activo
      }
    );
  }

  // ==========================================================
  // ADMIN GENERAL - MOTORISTAS
  // ==========================================================

  getAdminMotoristas():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/motoristas`
    );
  }

  getAdminMotorista(
    id: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Admin/motoristas/${id}`
    );
  }

  getUsuariosMotoristasDisponibles():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Admin/motoristas/usuarios-disponibles`
    );
  }

  crearAdminMotorista(
    motorista: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/Admin/motoristas`,
      motorista
    );
  }

  editarAdminMotorista(
    id: number,
    motorista: any
  ): Observable<any> {

    return this.http.put<any>(
      `${this.apiUrl}/Admin/motoristas/${id}`,
      motorista
    );
  }

  cambiarEstadoAdminMotorista(
    id: number,
    activo: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Admin/motoristas/${id}/estado`,
      {
        activo
      }
    );
  }

  cambiarDisponibilidadAdminMotorista(
    id: number,
    disponible: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Admin/motoristas/${id}/disponibilidad`,
      {
        disponible
      }
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - DASHBOARD
  // ==========================================================

  getRestauranteAdminDashboard():
    Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/AdminRestaurante/dashboard`
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - RESTAURANTE
  // ==========================================================

  getRestauranteAdmin():
    Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/AdminRestaurante/restaurante`
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - PEDIDOS
  // ==========================================================

  getRestauranteAdminPedidos():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/AdminRestaurante/pedidos`
    );
  }

  getRestauranteAdminPedido(
    idPedido: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/AdminRestaurante/pedidos/${idPedido}`
    );
  }

  getRestauranteAdminEstadosPedido(
    idPedido: number
  ): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/AdminRestaurante/pedidos/${idPedido}/estados`
    );
  }

  cambiarEstadoPedidoRestaurante(
    idPedido: number,
    idEstado: number,
    observacion: string = ''
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/AdminRestaurante/pedidos/${idPedido}/estado`,
      {
        idEstado,
        observacion
      }
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - PRODUCTOS
  // ==========================================================

  getRestauranteAdminProductos():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/AdminRestaurante/productos`
    );
  }

  crearRestauranteAdminProducto(
    producto: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/AdminRestaurante/productos`,
      producto
    );
  }

  editarRestauranteAdminProducto(
    idProducto: number,
    producto: any
  ): Observable<any> {

    return this.http.put<any>(
      `${this.apiUrl}/AdminRestaurante/productos/${idProducto}`,
      producto
    );
  }

  cambiarEstadoRestauranteAdminProducto(
    idProducto: number,
    activo: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/AdminRestaurante/productos/${idProducto}/estado`,
      {
        activo
      }
    );
  }

  cambiarDisponibilidadRestauranteAdminProducto(
    idProducto: number,
    disponible: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/AdminRestaurante/productos/${idProducto}/disponibilidad`,
      {
        disponible
      }
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - CATEGORÍAS
  // ==========================================================

  getRestauranteAdminCategorias():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/AdminRestaurante/categorias`
    );
  }

  crearRestauranteAdminCategoria(
    categoria: any
  ): Observable<any> {

    return this.http.post<any>(
      `${this.apiUrl}/AdminRestaurante/categorias`,
      categoria
    );
  }

  editarRestauranteAdminCategoria(
    idCategoria: number,
    categoria: any
  ): Observable<any> {

    return this.http.put<any>(
      `${this.apiUrl}/AdminRestaurante/categorias/${idCategoria}`,
      categoria
    );
  }

  cambiarEstadoRestauranteAdminCategoria(
    idCategoria: number,
    activo: boolean
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/AdminRestaurante/categorias/${idCategoria}/estado`,
      {
        activo
      }
    );
  }

  // ==========================================================
  // ADMIN RESTAURANTE - HISTORIAL
  //
  // Este método se conserva porque admin-restaurante.ts
  // actualmente lo utiliza.
  // ==========================================================

  getRestauranteAdminHistorial():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/AdminRestaurante/historial`
    );
  }

  // ==========================================================
  // MOTORISTA - DASHBOARD
  // ==========================================================

  getMotoristaDashboard():
    Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Motorista/dashboard`
    );
  }

  // ==========================================================
  // MOTORISTA - SERVICIOS
  // ==========================================================

  getMotoristaServicios():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Motorista/servicios`
    );
  }

  getMotoristaServicio(
    idServicio: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.apiUrl}/Motorista/servicios/${idServicio}`
    );
  }

  getMotoristaEstados(
    idServicio: number
  ): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Motorista/servicios/${idServicio}/estados`
    );
  }

  cambiarEstadoMotorista(
    idServicio: number,
    idEstado: number,
    observacion: string = ''
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Motorista/servicios/${idServicio}/estado`,
      {
        idEstado,
        observacion
      }
    );
  }

  getMotoristaHistorial():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Motorista/historial`
    );
  }

  // ==========================================================
  // ASIGNACIONES
  // ==========================================================

  getAsignacionMotoristas():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Asignaciones/motoristas`
    );
  }

  getAsignacionPendientes():
    Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/Asignaciones/pendientes`
    );
  }

  asignarMotoristaServicio(
    idServicio: number,
    idMotorista: number,
    observacion: string = ''
  ): Observable<any> {

    return this.http.patch<any>(
      `${this.apiUrl}/Asignaciones/servicios/${idServicio}/motorista`,
      {
        idMotorista,
        observacion
      }
    );
  }

  quitarMotoristaServicio(
    idServicio: number
  ): Observable<any> {

    return this.http.delete<any>(
      `${this.apiUrl}/Asignaciones/servicios/${idServicio}/motorista`
    );
  }

  guardarUbicacion(idServicio: number, latitud: number, longitud: number): Observable<any> {
    return this.http.post<any>(this.apiUrl + '/Ubicaciones/servicios/' + idServicio, { latitud, longitud });
  }
  getUbicacionRastreo(codigo: string): Observable<any> {
    return this.http.get<any>(this.apiUrl + '/Ubicaciones/rastreo/' + encodeURIComponent(codigo));
  }
}
