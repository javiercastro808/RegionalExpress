import { Injectable, InjectionToken, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject, tap } from "rxjs";
import { config } from "./config";
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
export const SESSION_STORAGE = new InjectionToken<Storage>("Session storage", {
  providedIn: "root",
  factory: () => sessionStorage,
});
@Injectable({ providedIn: "root" })
export class Auth {
  private http = inject(HttpClient);
  private storage = inject(SESSION_STORAGE);
  private readonly key = "regionalExpress.sesion.v2";
  private sesion: LoginRespuesta | null = this.cargar();
  private usuarioSubject = new BehaviorSubject<UsuarioSesion | null>(
    this.sesion?.usuario ?? null,
  );
  usuario$ = this.usuarioSubject.asObservable();
  login(correo: string, password: string) {
    return this.http
      .post<LoginRespuesta>(config.apiUrl + "/Auth/login", { correo, password })
      .pipe(
        tap((r) => {
          if (!r.token || !r.usuario || !this.vigente(r.token))
            throw new Error("Respuesta de sesión inválida.");
          this.storage.setItem(this.key, JSON.stringify(r));
          this.sesion = r;
          this.usuarioSubject.next(r.usuario);
        }),
      );
  }
  logout() {
    this.sesion = null;
    try {
      this.storage.removeItem(this.key);
    } catch {}
    this.usuarioSubject.next(null);
  }
  getToken(): string | null {
    if (this.sesion && !this.vigente(this.sesion.token)) this.logout();
    return this.sesion?.token ?? null;
  }
  getUsuario() {
    this.getToken();
    return this.sesion?.usuario ?? null;
  }
  estaAutenticado() {
    return !!this.getToken() && !!this.sesion?.usuario;
  }
  tieneRol(rol: string) {
    return this.getUsuario()?.rol?.trim().toUpperCase() === rol.toUpperCase();
  }
  private vigente(token: string) {
    try {
      const parte = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
      const datos = JSON.parse(atob(parte));
      return Number.isFinite(datos.exp) && datos.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
  private cargar(): LoginRespuesta | null {
    try {
      const r = JSON.parse(this.storage.getItem(this.key) || "null");
      if (r?.usuario?.idUsuario && this.vigente(r.token)) return r;
      this.storage.removeItem(this.key);
    } catch {}
    return null;
  }
}
