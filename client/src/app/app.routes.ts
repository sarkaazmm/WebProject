import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { H } from '@angular/cdk/keycodes';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { AccountComponent } from './pages/account/account.component';

export const routes: Routes = [
    {
        path: '',
        component: HomeComponent
    },
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'register',
        component: RegisterComponent
    },
    {
        path: 'account/:id',
        component: AccountComponent
    }
];
