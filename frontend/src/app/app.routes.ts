import { soloInvitados, panelAutorizado } from './services/sesion-rutas';
import { Routes } from '@angular/router';



export const routes: Routes = [
  { path: 'panel/cliente', loadComponent:()=>import('./pages/cliente/cliente').then(m=>m.Cliente), canActivate: [panelAutorizado] },

  // ==========================================================
  // PÚBLICO
  // ==========================================================

  {
    path: '',
    loadComponent:()=>import('./pages/home/home').then(m=>m.Home)
  },

  {
    path: 'delivery',
    loadComponent:()=>import('./pages/delivery/delivery').then(m=>m.Delivery)
  },

  {
    path: 'restaurante/:id',
    loadComponent:()=>import('./pages/restaurante/restaurante').then(m=>m.Restaurante)
  },

  {
    path: 'envios',
    loadComponent:()=>import('./pages/envios/envios').then(m=>m.Envios)
  },

  {
    path: 'rastreo',
    loadComponent:()=>import('./pages/rastreo/rastreo').then(m=>m.Rastreo)
  },

  {
    path: 'login',
    canActivate: [soloInvitados],
    loadComponent:()=>import('./pages/login/login').then(m=>m.Login)
  },

  // ==========================================================
  // PANELES PRINCIPALES
  // ==========================================================

  {
    path: 'panel/admin',
    canActivate: [panelAutorizado],
    loadComponent:()=>import('./pages/admin/admin').then(m=>m.Admin)
  },

  {
    path: 'panel/restaurante',
    canActivate: [panelAutorizado],
    loadComponent:()=>import('./pages/admin-restaurante/admin-restaurante').then(m=>m.AdminRestaurante)
  },

  {
    path: 'panel/motorista',
    canActivate: [panelAutorizado],
    loadComponent:()=>import('./pages/motorista/motorista').then(m=>m.Motorista)
  },

  // ==========================================================
  // RUTAS ANTERIORES
  // Las conservamos para no romper enlaces existentes
  // ==========================================================

  {
    path: 'admin',
    redirectTo: 'panel/admin',
    pathMatch: 'full'
  },

  {
    path: 'admin-restaurante',
    redirectTo: 'panel/restaurante',
    pathMatch: 'full'
  },

  {
    path: 'motorista',
    redirectTo: 'panel/motorista',
    pathMatch: 'full'
  },

  // ==========================================================
  // RUTA NO ENCONTRADA
  // ==========================================================

  {
    path: '**',
    redirectTo: ''
  }

];