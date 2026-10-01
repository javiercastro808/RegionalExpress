export function mensajeError(error:any, respaldo:string):string {
 if(typeof error?.error?.mensaje==='string')return error.error.mensaje;
 const errores=error?.error?.errors;
 if(errores&&typeof errores==='object'){
  const mensajes=Object.values(errores).flat().filter(v=>typeof v==='string');
  if(mensajes.length)return mensajes.join(' ');
 }
 if(error?.status===0)return 'No se pudo conectar con el servidor. Revisa la conexión y vuelve a intentar.';
 if(error?.status===409)return 'Los datos cambiaron o ya existen. Actualiza la lista y revisa los campos.';
 if(error?.status>=500)return respaldo+' El servidor no pudo guardar los datos; inténtalo de nuevo.';
 return respaldo;
}
