import type * as Leaflet from 'leaflet';
/** Identificación de origen y caché normal según la política de teselas de OSM. */
export function capaMapa(L:typeof Leaflet, avisar:(mensaje:string)=>void):Leaflet.TileLayer {
 const capa=L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png',{
  maxZoom:19,referrerPolicy:'strict-origin-when-cross-origin',updateWhenIdle:true,keepBuffer:1,
  attribution:'&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap contributors</a>'
 });
 let errores=0;
 capa.on('loading',()=>{errores=0;});
 capa.on('tileerror',()=>{errores++;avisar('No se pudo cargar la cartografía. Si aparece Access blocked, el proveedor bloqueó las imágenes; no es un error de tu ubicación. Abre la página en Chrome, Edge o Safari con el enlace HTTPS y sin un navegador integrado.');});
 capa.on('load',()=>{if(!errores)avisar('');});
 return capa;
}
