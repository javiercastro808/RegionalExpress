import { TestBed } from "@angular/core/testing";
import { Injector } from "@angular/core";
import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from "@angular/common/http";
import {
  HttpTestingController,
  provideHttpClientTesting,
} from "@angular/common/http/testing";
import { Auth, SESSION_STORAGE, LoginRespuesta } from "./auth";
import { authInterceptor } from "./auth-interceptor";
import { config } from "./config";

function memoria(): Storage {
  const data = new Map<string, string>();
  return {
    get length() {
      return data.size;
    },
    clear: () => data.clear(),
    getItem: (k) => data.get(k) ?? null,
    setItem: (k, v) => {
      data.set(k, v);
    },
    removeItem: (k) => {
      data.delete(k);
    },
    key: (n) => [...data.keys()][n] ?? null,
  };
}
function sesion(id: number): LoginRespuesta {
  return {
    mensaje: "OK",
    token:
      "e30." +
      btoa(
        JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 3600, sub: id }),
      ) +
      ".firma",
    usuario: {
      idUsuario: id,
      nombre: "Usuario " + id,
      correo: "cuenta" + id + "@example.test",
      idRol: 1,
      rol: "CLIENTE",
    },
  };
}
describe("Sesiones independientes", () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());
  it("una segunda cuenta y su cierre no sustituyen la sesión de otra pestaña", () => {
    const aStorage = memoria(),
      bStorage = memoria();
    const crear = (storage: Storage) =>
      Injector.create({
        providers: [Auth, { provide: SESSION_STORAGE, useValue: storage }],
        parent: TestBed.inject(Injector),
      }).get(Auth);
    const a = crear(aStorage);
    const http = TestBed.inject(HttpTestingController);
    a.login("a", "clave").subscribe();
    http.expectOne(config.apiUrl + "/Auth/login").flush(sesion(1));
    const b = crear(bStorage);
    b.login("b", "clave").subscribe();
    http.expectOne(config.apiUrl + "/Auth/login").flush(sesion(2));
    expect(b.getUsuario()?.idUsuario).toBe(2);
    expect(a.getUsuario()?.idUsuario).toBe(1);
    expect(aStorage.getItem("regionalExpress.sesion.v2")).toContain(
      "Usuario 1",
    );
    expect(b.getToken()).not.toBe(a.getToken());
    b.logout();
    expect(b.getToken()).toBeNull();
    expect(a.getUsuario()?.idUsuario).toBe(1);
    expect(crear(aStorage).getUsuario()?.idUsuario).toBe(1);
  });
  it("no usa el token global heredado ni lo envía a sitios externos", () => {
    localStorage.setItem("regionalExpressToken", "token-de-otra-pestana");
    expect(TestBed.inject(Auth).getToken()).toBeNull();
    TestBed.inject(HttpClient).get("https://example.test/image").subscribe();
    const req = TestBed.inject(HttpTestingController).expectOne(
      "https://example.test/image",
    );
    expect(req.request.headers.has("Authorization")).toBe(false);
    req.flush({});
    localStorage.removeItem("regionalExpressToken");
  });
});
