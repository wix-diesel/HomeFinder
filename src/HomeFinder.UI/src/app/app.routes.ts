// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { ItemListComponent } from './components/item-list/item-list';
import { CategoryComponent } from './components/category/category';
import { AdminComponent } from './components/admin/admin';

// ここで必ず export
export const routes: Routes = [
  { path: 'items', component: ItemListComponent },
  { path: 'categories', component: CategoryComponent },
  { path: 'admin', component: AdminComponent },
  { path: '', redirectTo: '/items', pathMatch: 'full' }
];
