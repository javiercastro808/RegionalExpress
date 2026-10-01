export interface PuntoRastreo { tipo: string; latitud: number; longitud: number; color: string; etiqueta: string; }
export function puntosRastreo(ubicacion: any): PuntoRastreo[] {
 const puntos: PuntoRastreo[] = [];
 const agregar=(p:any,tipo:string,color:string,etiqueta:string)=>{
  if(p?.latitud==null||p?.longitud==null)return;
  const latitud=Number(p.latitud),longitud=Number(p.longitud);
  if(!Number.isFinite(latitud)||!Number.isFinite(longitud)||Math.abs(latitud)>90||Math.abs(longitud)>180)return;
  puntos.push({tipo,latitud,longitud,color,etiqueta});
 };
 agregar(ubicacion?.origen,'origen','#2563eb','Origen');
 agregar(ubicacion?.destino,'destino','#dc2626','Cliente / destino');
 if(ubicacion?.disponible)agregar(ubicacion,'motorista','#15803d',ubicacion.reciente?'Motorista':'Motorista: posición antigua');
 return puntos;
}
export function necesitaEncuadre(anterior:string,puntos:PuntoRastreo[]):boolean {
 return puntos.length>0 && anterior!==puntos.map(p=>p.tipo).join(',');
}
