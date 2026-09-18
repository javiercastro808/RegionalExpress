import { soloInvitados, panelAutorizado } from './services/sesion-rutas';
import { Cliente } from './pages/cliente/cliente';
import { Routes } from '@angular/router';

import { Home } from './pages/home/home';
import { Delivery } from './pages/delivery/delivery';
import { Restaurante } from './pages/restaurante/restaurante';
import { Envios } from './pages/envios/envios';
import { Rastreo } from './pages/rastreo/rastreo';

import { Login } from './pages/login/login';
import { Admin } from './pages/admin/admin';
import { AdminRestaurante } from './pages/admin-restaurante/admin-restaurante';
import { Motorista } from './pages/motorista/motorista';

export const routes: Routes = [
  { path: 'panel/cliente', component: Cliente, canActivate: [panelAutorizado] },

  // ==========================================================
  // PÚBLICO
  // ==========================================================

  {
    path: '',
    component: Home
  },

  {
    path: 'delivery',
    component: Delivery
  },

  {
    path: 'restaurante/:id',
    component: Restaurante
  },

  {
    path: 'envios',
    component: Envios
  },

  {
    path: 'rastreo',
    component: Rastreo
  },

  {
    path: 'login',
    canActivate: [soloInvitados],
    component: Login
  },

  // ==========================================================
  // PANELES PRINCIPALES
  // ==========================================================

  {
    path: 'panel/admin',
    canActivate: [panelAutorizado],
    component: Admin
  },

  {
    path: 'panel/restaurante',
    canActivate: [panelAutorizado],
    component: AdminRestaurante
  },

  {
    path: 'panel/motorista',
    canActivate: [panelAutorizado],
    component: Motorista
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