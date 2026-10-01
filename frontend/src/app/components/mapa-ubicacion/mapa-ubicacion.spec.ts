import { MapaUbicacion, coordenadasValidas } from "./mapa-ubicacion";
describe("Ubicación", () => {
  it("valida rangos y acepta coordenadas cero", () => {
    expect(coordenadasValidas(0, 0)).toBe(true);
    expect(coordenadasValidas(91, 0)).toBe(false);
    expect(coordenadasValidas(0, 181)).toBe(false);
    expect(coordenadasValidas(NaN, 0)).toBe(false);
  });
  it("solo comunica puntos elegidos válidos", () => {
    const c = new MapaUbicacion();
    const recibir = vi.fn();
    c.puntoChange.subscribe(recibir);
    c.elegir(14.6, -90.5);
    expect(recibir).toHaveBeenCalledWith({ latitud: 14.6, longitud: -90.5 });
    c.elegir(99, 0);
    expect(recibir).toHaveBeenCalledTimes(1);
    c.ngOnDestroy();
    c.elegir(1, 1);
    expect(recibir).toHaveBeenCalledTimes(1);
  });
});
