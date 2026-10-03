import {Component,ElementRef,Input,ViewChild,OnDestroy,signal} from '@angular/core';
@Component({selector:'app-ventana-mapa',standalone:true,template:`
<button type="button" (click)="abrir()" aria-haspopup="dialog" [attr.aria-expanded]="abierta()">Ampliar mapa</button>
<dialog #ventana aria-label="Mapa ampliado" (close)="restaurar()">
 <header><strong>Mapa ampliado</strong><button type="button" autofocus (click)="cerrar()">Reducir mapa ×</button></header>
 <p>Puedes mover y acercar el mapa. Pulsa Reducir mapa o Escape para volver.</p>
 <div #destino class="contenido"></div>
</dialog>`,styles:[`
button{padding:10px 14px;border:1px solid #16614e;border-radius:8px;background:white;color:#16614e;cursor:pointer;margin:4px 0}
dialog{width:98vw;max-width:none;height:96vh;height:96dvh;max-height:98dvh;margin:auto;padding:14px;border:0;border-radius:14px;background:#fff;color:#172b3a;box-sizing:border-box}
dialog[open]{display:flex;flex-direction:column}dialog::backdrop{background:#0009}
header{display:flex;align-items:center;justify-content:space-between;gap:12px;flex-shrink:0}p{margin:6px 0 10px;font-size:14px}.contenido{flex:1;min-height:0;width:100%}
`]})
export class VentanaMapa implements OnDestroy{
 @Input({required:true}) elemento!:HTMLElement;
 @ViewChild('ventana',{static:true}) ventana!:ElementRef<HTMLDialogElement>;
 @ViewChild('destino',{static:true}) destino!:ElementRef<HTMLDivElement>;
 abierta=signal(false);
 private padre:Node|null=null;private siguiente:Node|null=null;private estilo:string|null=null;
 abrir(){
  if(this.abierta()||!this.elemento)return;
  const dialog=this.ventana.nativeElement;
  dialog.showModal();
  this.padre=this.elemento.parentNode;this.siguiente=this.elemento.nextSibling;this.estilo=this.elemento.getAttribute('style');
  this.destino.nativeElement.appendChild(this.elemento);
  this.elemento.style.height='100%';this.elemento.style.minHeight='0';this.elemento.style.width='100%';
  this.abierta.set(true);window.dispatchEvent(new Event('resize'));
 }
 cerrar(){this.ventana.nativeElement.close();this.restaurar();}
 restaurar(){
  if(!this.padre)return;
  this.padre.insertBefore(this.elemento,this.siguiente?.parentNode===this.padre?this.siguiente:null);
  if(this.estilo===null)this.elemento.removeAttribute('style');else this.elemento.setAttribute('style',this.estilo);
  this.padre=null;this.siguiente=null;this.abierta.set(false);window.dispatchEvent(new Event('resize'));
 }
 ngOnDestroy(){this.restaurar();if(this.ventana?.nativeElement.open)this.ventana.nativeElement.close();}
}
