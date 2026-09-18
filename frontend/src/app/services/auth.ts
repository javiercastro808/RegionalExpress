import { API_URL } from './api-url';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface UsuarioSesion {
  idUsuario: number;
  nombre: string;
  correo: string;
  idRol: number;
  rol: string;
}

export interface LoginRespuesta {
  mensaje: string;
  token: string;
  usuario: UsuarioSesion;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private readonly apiUrl =
    API_URL + '/Auth';

  private readonly tokenKey =
    'regionalExpressToken';

  private readonly usuarioKey =
    'regionalExpressUsuario';

  private usuarioSubject =
    new BehaviorSubject<UsuarioSesion | null>(
      this.cargarUsuario()
    );

  usuario$ =
    this.usuarioSubject.asObservable();

  constructor(
    private http: HttpClient
  ) {}


  login(
    correo: string,
    password: string
  ): Observable<LoginRespuesta> {

    return this.http.post<LoginRespuesta>(
      `${this.apiUrl}/login`,
      {
        correo,
        password
      }
    )
    .pipe(
      tap(respuesta => {

        localStorage.setItem(
          this.tokenKey,
          respuesta.token
        );

        localStorage.setItem(
          this.usuarioKey,
          JSON.stringify(
            respuesta.usuario
          )
        );

        this.usuarioSubject.next(
          respuesta.usuario
        );

      })
    );
  }


  logout(): void {

    localStorage.removeItem(
      this.tokenKey
    );

    localStorage.removeItem(
      this.usuarioKey
    );

    this.usuarioSubject.next(
      null
    );
  }


  getToken(): string | null {

    return localStorage.getItem(
      this.tokenKey
    );
  }


  getUsuario(): UsuarioSesion | null {

    return this.usuarioSubject.value;
  }


  estaAutenticado(): boolean {

    return this.tokenVigente() && !!this.getUsuario();
  }


  tieneRol(rol: string): boolean {

    return (
      this.getUsuario()?.rol === rol
    );
  }


  private tokenVigente(): boolean {
    try {
      const token = this.getToken();
      if (!token) return false;
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now();
    } catch { return false; }
  }

  private cargarUsuario():
    UsuarioSesion | null {

    const usuario =
      localStorage.getItem(
        this.usuarioKey
      );

    if (!usuario || !this.tokenVigente()) {
      return null;
    }

    try {

      return JSON.parse(
        usuario
      ) as UsuarioSesion;

    }
    catch {

      localStorage.removeItem(
        this.usuarioKey
      );

      return null;
    }
  }
}