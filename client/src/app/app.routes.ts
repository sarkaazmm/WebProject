import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { H } from '@angular/cdk/keycodes';
import { HomeComponent } from './pages/home/home.component';

export const routes: Routes = [
    {
        path: '',
        component: HomeComponent
    },
    {
        path: 'login',
        component: LoginComponent
    },
];
