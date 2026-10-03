import "@angular/compiler";
import fs from "node:fs";
import path from "node:path";
import { pathToFileURL, fileURLToPath } from "node:url";
import assert from "node:assert/strict";
import ts from "typescript";
import { Injector, DestroyRef, runInInjectionContext } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { of } from "rxjs";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const out = path.join(root, ".verification");
fs.mkdirSync(out, { recursive: true });
fs.writeFileSync(path.join(out, "package.json"), '{"type":"module"}');
function compile(dir) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const f = path.join(dir, e.name);
    if (e.isDirectory()) {
      compile(f);
      continue;
    }
    if (!f.endsWith(".ts") || f.endsWith(".spec.ts")) continue;
    const dest = path
      .join(out, path.relative(path.join(root, "src"), f))
      .replace(/\.ts$/, ".js");
    fs.mkdirSync(path.dirname(dest), { recursive: true });
    let code = ts.transpileModule(fs.readFileSync(f, "utf8"), {
      compilerOptions: {
        target: ts.ScriptTarget.ES2022,
        module: ts.ModuleKind.ES2022,
        experimentalDecorators: true,
      },
    }).outputText;
    code = code.replace(
      /from\s+(['"])(\.[^'"]+)\1/g,
      (_, q, p) => "from " + q + p + ".js" + q,
    );
    fs.writeFileSync(dest, code);
  }
}
compile(path.join(root, "src"));
const load = (p) =>
  import(pathToFileURL(path.join(out, "app", p + ".js")).href);
const { Auth, SESSION_STORAGE } = await load("services/auth");
const { Carrito } = await load("services/carrito");
const { Restaurante } = await load("pages/restaurante/restaurante");
const { Motorista } = await load("pages/motorista/motorista");
const { MapaUbicacion, coordenadasValidas } = await load(
  "components/mapa-ubicacion/mapa-ubicacion",
);
function storage() {
  const data = new Map();
  return {
    getItem: (k) => data.get(k) ?? null,
    setItem: (k, v) => data.set(k, v),
    removeItem: (k) => data.delete(k),
    clear: () => data.clear(),
  };
}
globalThis.sessionStorage = storage();
const response = (id) => ({
  mensaje: "OK",
  token:
    "e30." +
    btoa(JSON.stringify({ exp: Date.now() / 1000 + 3600, sub: id })) +
    ".test",
  usuario: {
    idUsuario: id,
    nombre: "Usuario " + id,
    correo: id + "@example.test",
    idRol: 1,
    rol: "CLIENTE",
  },
});
const make = (store) =>
  Injector.create({
    providers: [
      Auth,
      { provide: SESSION_STORAGE, useValue: store },
      {
        provide: HttpClient,
        useValue: { post: (_url, body) => of(response(Number(body.correo))) },
      },
      { provide: DestroyRef, useValue: { onDestroy: () => () => {} } },
    ],
  });
let count = 0;
const test = (name, fn) => {
  fn();
  console.log("OK " + name);
  count++;
};
test("sesiones de dos pestañas, recarga y cierre independientes", () => {
  const sa = storage(),
    sb = storage();
  const a = make(sa).get(Auth),
    b = make(sb).get(Auth);
  a.login("1", "x").subscribe();
  b.login("2", "x").subscribe();
  assert.equal(a.getUsuario().idUsuario, 1);
  assert.equal(b.getUsuario().idUsuario, 2);
  b.logout();
  assert.equal(a.getUsuario().idUsuario, 1);
  assert.equal(b.getToken(), null);
  assert.equal(make(sa).get(Auth).getUsuario().idUsuario, 1);
});
const injector = make(storage()),
  auth = injector.get(Auth);
auth.login("1", "x").subscribe();
const carrito = runInInjectionContext(injector, () => new Carrito());
const cdr = { markForCheck() {}, detectChanges() {} };
let pedidos = 0;
const restaurante = runInInjectionContext(
  injector,
  () =>
    new Restaurante(
      {},
      {
        crearPedido: () => {
          pedidos++;
          return of({ codigoRastreo: "TEST" });
        },
      },
      carrito,
      cdr,
    ),
);
test("varios productos, cantidades y 100 alternancias sin acumular envío", () => {
  carrito.agregar({ idRestaurante: 1, idProducto: 1, nombre: "A", precio: 30 });
  carrito.agregar({ idRestaurante: 1, idProducto: 1, nombre: "A", precio: 30 });
  carrito.agregar({ idRestaurante: 1, idProducto: 2, nombre: "B", precio: 10 });
  restaurante.itemsCarrito = carrito.obtenerItems();
  assert.equal(restaurante.subtotalItem(restaurante.itemsCarrito[0]), 60);
  for (let i = 0; i < 100; i++) {
    restaurante.cambiarModalidad("DOMICILIO");
    assert.equal(restaurante.total(), 80);
    restaurante.cambiarModalidad("RECOGER");
    assert.equal(restaurante.total(), 70);
  }
  carrito.disminuir(1);
  restaurante.itemsCarrito = carrito.obtenerItems();
  assert.equal(restaurante.total(), 40);
});
test("domicilio bloquea pedidos sin ubicación; recoger permite continuar", () => {
  restaurante.restaurante = { idRestaurante: 1 };
  restaurante.direccion = "Dirección de prueba";
  restaurante.cambiarModalidad("DOMICILIO");
  restaurante.confirmarPedido();
  assert.equal(pedidos, 0);
  restaurante.ubicar({ latitud: 14.6, longitud: -90.5 });
  restaurante.confirmarPedido();
  assert.equal(pedidos, 1);
  carrito.agregar({ idRestaurante: 1, idProducto: 2, nombre: "B", precio: 10 });
  restaurante.itemsCarrito = carrito.obtenerItems();
  restaurante.cambiarModalidad("RECOGER");
  restaurante.confirmarPedido();
  assert.equal(pedidos, 2);
});
test("carrito vacío sin cobro de envío", () => {
  carrito.limpiar();
  restaurante.itemsCarrito = [];
  restaurante.cambiarModalidad("DOMICILIO");
  assert.equal(restaurante.total(), 0);
});
test("carrito separado al cambiar de cuenta en la misma pestaña", () => {
  carrito.agregar({ idRestaurante: 1, idProducto: 3, nombre: "C", precio: 5 });
  auth.login("2", "x").subscribe();
  assert.equal(carrito.obtenerItems().length, 0);
  auth.login("1", "x").subscribe();
  assert.equal(carrito.obtenerItems()[0].idProducto, 3);
});
test("mapa rechaza coordenadas inválidas y callbacks después de destruirlo", () => {
  assert.equal(coordenadasValidas(0, 0), true);
  assert.equal(coordenadasValidas(91, 0), false);
  let n = 0;
  const map = new MapaUbicacion();
  map.puntoChange.subscribe(() => n++);
  map.elegir(14.6, -90.5);
  map.elegir(NaN, 1);
  map.ngOnDestroy();
  map.elegir(1, 1);
  assert.equal(n, 1);
});
test("motorista exige ubicación confirmada reciente del servicio correcto", () => {
  const m = new Motorista(cdr, {}, auth, {});
  m.servicioSeleccionado = { idServicio: 8, activo: true };
  m.idEstadoSeleccionado = 2;
  m.estadosServicio = [
    { idEstado: 2, nombreEstado: "En camino" },
    { idEstado: 3, nombreEstado: "Cancelado" },
  ];
  assert.equal(m.requiereUbicacion(), true);
  assert.equal(m.ubicacionLista(), false);
  m.compartiendoServicio = 8;
  m.ultimaUbicacionConfirmada = Date.now();
  assert.equal(m.ubicacionLista(), true);
  m.compartiendoServicio = 9;
  assert.equal(m.ubicacionLista(), false);
  m.compartiendoServicio = 8;
  m.ultimaUbicacionConfirmada = Date.now() - 121000;
  assert.equal(m.ubicacionLista(), false);
  m.idEstadoSeleccionado = 3;
  assert.equal(m.requiereUbicacion(), false);
  m.idEstadoSeleccionado = 2;
  m.servicioSeleccionado.detalle = { modalidad: "RECOGER" };
  assert.equal(m.requiereUbicacion(), false);
  m.servicioSeleccionado.detalle.modalidad = "DOMICILIO";
  assert.equal(m.requiereUbicacion(), true);
  m.detenerUbicacion();
  assert.equal(m.ubicacionLista(), false);
});
const {Admin}=await load('pages/admin/admin');
const {Envios}=await load('pages/envios/envios');
test('carrito rechaza otro restaurante y redondea centavos',()=>{
  carrito.limpiar();assert.equal(carrito.agregar({idProducto:1,idRestaurante:1,nombre:'A',precio:.1}),true);
  assert.equal(carrito.agregar({idProducto:2,idRestaurante:2,nombre:'B',precio:10}),false);
  carrito.agregar({idProducto:3,idRestaurante:1,nombre:'C',precio:.2});assert.equal(carrito.obtenerSubtotal(),.3);
  assert.equal(carrito.obtenerCantidadTotal(),2);
});
test('asignación visual no inventa un motorista; conserva el asignado real',()=>{
 const a=new Admin(cdr,{},auth,{});
 a.servicios=[{idServicio:1,idMotorista:8,motorista:'Motorista prueba',tipoServicio:'ENVIO',activo:true},
 {idServicio:2,idMotorista:null,tipoServicio:'ENVIO',activo:true},
 {idServicio:3,idMotorista:null,tipoServicio:'DELIVERY',modalidad:'RECOGER',activo:true},
 {idServicio:4,idMotorista:null,tipoServicio:'ENVIO',activo:true}];
 a.serviciosPendientes=[{idServicio:3},{idServicio:4}];a.motoristasAsignacion=[];a.prepararAsignaciones();
 assert.equal(a.servicios[0].asignacion.asignado,true);assert.equal(a.servicios[0].asignacion.nombre,'Motorista prueba');
 assert.equal(a.servicios[1].asignacion.asignado,false);assert.equal(a.servicios[2].asignacion.puedeAsignar,false);assert.equal(a.servicios[3].asignacion.puedeAsignar,true);
});
test('envío exige ambos puntos y transmite coordenadas al backend',()=>{
 let datos;const e=new Envios(cdr,auth,{crearEnvio:d=>{datos=d;return of({codigoRastreo:'ENV-TEST'});}});
 e.confirmarEnvio();assert.equal(datos,undefined);
 e.nombreRemitente='Origen';e.telefonoRemitente='55550000';e.nombreDestinatario='Destino';e.telefonoDestinatario='55551111';
 e.direccionOrigen='A';e.direccionDestino='B';e.peso=1;e.distanciaKm=2;e.tipoPaquete='CAJA';e.tarifaCalculada={total:20};
 e.puntoOrigen={latitud:14.6,longitud:-90.5};e.confirmarEnvio();assert.equal(datos,undefined);
 e.puntoDestino={latitud:14.7,longitud:-90.6};e.confirmarEnvio();assert.equal(datos.latitudOrigen,14.6);assert.equal(datos.longitudDestino,-90.6);
});
const {presentarProducto}=await load('services/imagenes-menu');
test('fotografías de referencia conservadas y asociadas a su descripción',()=>{
 const p=presentarProducto({nombre:'Desayuno Chapin',descripcion:'Huevos, frijoles, platano y tortillas'});
 assert.equal(p.imagen,'/images/menu/desayuno-chapin.png');assert.equal(fs.existsSync(path.join(root,'public',p.imagen)),true);
 assert.equal(presentarProducto({nombre:'Otro producto',descripcion:'Otra descripción'}).imagen,null);
 assert.equal(presentarProducto({nombre:'Otro',imagen:'/foto-real.png'}).imagen,'/foto-real.png');
});
// Simulate a browser that never returns GPS, then delivers an obsolete callback.
{
 const originalTimeout=globalThis.setTimeout;
 const oldNavigator=Object.getOwnPropertyDescriptor(globalThis,'navigator');
 const oldWindow=Object.getOwnPropertyDescriptor(globalThis,'window');
 let callback, sent=0;
 const m=new Motorista(cdr,{guardarUbicacion:()=>{sent++;return of({});}},auth,{});
 try {
  Object.defineProperty(globalThis,'window',{configurable:true,value:{isSecureContext:true}});
  Object.defineProperty(globalThis,'navigator',{configurable:true,value:{geolocation:{getCurrentPosition:success=>{callback=success;}}}});
  globalThis.setTimeout=(fn,ms,...args)=>originalTimeout(fn,ms===20000?20:ms,...args);
  m.servicioSeleccionado={idServicio:8,activo:true};m.compartirUbicacion();
  await new Promise(resolve=>originalTimeout(resolve,70));
  assert.equal(m.compartiendoServicio,null);
  assert.match(m.mensajeUbicacion,/no respondió/);
  callback({coords:{latitude:14.6,longitude:-90.5,accuracy:10},timestamp:Date.now()});
  assert.equal(sent,0);
  assert.equal(m.ubicacionLista(),false);
  count++;console.log('OK GPS sin respuesta termina la espera y descarta coordenadas tardías');
 } finally {
  m.ngOnDestroy();globalThis.setTimeout=originalTimeout;
  if(oldNavigator)Object.defineProperty(globalThis,'navigator',oldNavigator);else delete globalThis.navigator;
  if(oldWindow)Object.defineProperty(globalThis,'window',oldWindow);else delete globalThis.window;
 }
}
const {puntosRastreo,necesitaEncuadre}=await load('pages/rastreo/puntos-rastreo');
test('rastreo separa cliente y motorista y reencuadra al recibir al motorista',()=>{
 const datos={destino:{latitud:14.8,longitud:-90.6},disponible:false};
 const inicial=puntosRastreo(datos);assert.deepEqual(inicial.map(p=>p.tipo),['destino']);
 const llegada=puntosRastreo({...datos,disponible:true,reciente:true,latitud:14.5,longitud:-90.3});
 assert.equal(llegada.find(p=>p.tipo==='destino').latitud,14.8);
 assert.equal(llegada.find(p=>p.tipo==='motorista').latitud,14.5);
 assert.equal(necesitaEncuadre('destino',llegada),true);
 assert.equal(necesitaEncuadre('destino,motorista',llegada),false);
 assert.equal(puntosRastreo({...datos,disponible:true,latitud:null,longitud:null}).length,1);
});
test('motorista confirma siguiente paso sin saltos ni cancelación ordinaria',()=>{
 const m=new Motorista(cdr,{},auth,{});
 m.servicioSeleccionado={estado:{idEstado:1,ordenEstado:4}};
 m.estadosServicio=[{idEstado:3,ordenEstado:6,nombreEstado:'Entregado'},{idEstado:4,ordenEstado:7,nombreEstado:'Cancelado'},{idEstado:2,ordenEstado:5,nombreEstado:'En ruta'}];
 assert.equal(m.siguientePaso.idEstado,2);
 let solicitado=0;m.actualizarEstado=()=>solicitado=m.idEstadoSeleccionado;m.confirmarPaso();assert.equal(solicitado,2);
 m.servicioSeleccionado.estado={idEstado:2,ordenEstado:5};assert.equal(m.siguientePaso.idEstado,3);
 m.servicioSeleccionado.estado={idEstado:3,ordenEstado:6};assert.equal(m.siguientePaso,undefined);
});

const {mensajeError}=await load('services/mensajes-error');
test('formularios muestran validaciones de campos y problemas de conexión',()=>{
 assert.equal(mensajeError({error:{errors:{Correo:['Correo inválido'],Password:['Mínimo 8 caracteres']}}},'Error'),'Correo inválido Mínimo 8 caracteres');
 assert.equal(mensajeError({error:{mensaje:'Correo ya registrado'}},'Error'),'Correo ya registrado');
 assert.match(mensajeError({status:0},'Error'),/conectar/);
});
test('tarjetas del motorista abren filtros y muestran un solo título',()=>{
 const m=new Motorista(cdr,{},auth,{});m.servicios=[{activo:true,tipoServicio:'DELIVERY'},{activo:false,tipoServicio:'ENVIO'}];
 m.verResumen('servicios',true);assert.equal(m.serviciosVista.length,1);assert.equal(m.tituloServicios,'Servicios activos');
 m.verResumen('envios');assert.equal(m.serviciosVista.length,1);assert.equal(m.tituloServicios,'Envíos de paquetes');
 m.verResumen('delivery');assert.equal(m.tituloServicios,'Entregas de comida');
});
{
 const oldNav=Object.getOwnPropertyDescriptor(globalThis,'navigator'),oldWin=Object.getOwnPropertyDescriptor(globalThis,'window');
 let precision=245,envios=0;
 const m=new Motorista(cdr,{guardarUbicacion:()=>{envios++;return of({});}},auth,{});
 try {
 Object.defineProperty(globalThis,'window',{configurable:true,value:{isSecureContext:true}});
 Object.defineProperty(globalThis,'navigator',{configurable:true,value:{geolocation:{getCurrentPosition:cb=>cb({coords:{latitude:14.6,longitude:-90.5,accuracy:precision},timestamp:Date.now()})}}});
 m.servicioSeleccionado={idServicio:8,activo:true};m.compartirUbicacion();await new Promise(r=>setTimeout(r,25));
 assert.equal(envios,1);assert.equal(m.ubicacionLista(),true);assert.match(m.mensajeUbicacion,/aproximada/);
 precision=501;m.compartirUbicacion();await new Promise(r=>setTimeout(r,25));assert.equal(envios,1);assert.equal(m.ubicacionLista(),false);
 count++;console.log('OK ubicación de 245 metros se confirma como aproximada; más de 500 no avanza');
 }finally{m.ngOnDestroy();if(oldNav)Object.defineProperty(globalThis,'navigator',oldNav);else delete globalThis.navigator;if(oldWin)Object.defineProperty(globalThis,'window',oldWin);else delete globalThis.window;}
}

const {capaMapa}=await load('services/capa-mapa');
test('cartografía conserva atribución y referencia del sitio e informa fallos',()=>{
 let opciones,mensaje='';const eventos={};const capa={on:(e,cb)=>{eventos[e]=cb;return capa;}};
 capaMapa({tileLayer:(url,opts)=>{opciones=opts;assert.equal(url,'https://tile.openstreetmap.org/{z}/{x}/{y}.png');return capa;}},v=>mensaje=v);
 assert.equal(opciones.referrerPolicy,'strict-origin-when-cross-origin');assert.match(opciones.attribution,/OpenStreetMap/);
 eventos.loading();eventos.tileerror();eventos.load();assert.match(mensaje,/cartografía/);
 eventos.loading();eventos.load();assert.equal(mensaje,'');
});
const {JSDOM}=await import('jsdom');
const {VentanaMapa}=await load('components/ventana-mapa/ventana-mapa');
test('ventana ampliada conserva el mismo mapa y restaura posición y tamaño',()=>{
 const dom=new JSDOM('<div id="base"><div id="mapa" style="height:300px"></div><p id="despues"></p></div><dialog><div id="destino"></div></dialog>');
 const old=Object.getOwnPropertyDescriptor(globalThis,'window');
 Object.defineProperty(globalThis,'window',{configurable:true,value:{dispatchEvent:()=>{}}});
 try {
 const doc=dom.window.document,v=new VentanaMapa(),mapa=doc.getElementById('mapa'),dialog=doc.querySelector('dialog');
 dialog.showModal=()=>dialog.setAttribute('open','');dialog.close=()=>dialog.removeAttribute('open');
 v.elemento=mapa;v.ventana={nativeElement:dialog};v.destino={nativeElement:doc.getElementById('destino')};
 v.abrir();assert.equal(v.abierta(),true);assert.equal(mapa.parentNode.id,'destino');assert.equal(mapa.style.height,'100%');
 v.cerrar();assert.equal(mapa.parentNode.id,'base');assert.equal(mapa.nextSibling.id,'despues');assert.equal(mapa.style.height,'300px');
 v.abrir();v.restaurar();assert.equal(v.abierta(),false);assert.equal(mapa.parentNode.id,'base');v.ngOnDestroy();
 }finally{dom.window.close();if(old)Object.defineProperty(globalThis,'window',old);else delete globalThis.window;}
});
console.log(
  count +
    " pruebas de lógica aprobadas. No reemplazan las pruebas visuales ni la integración con SQL.",
);
