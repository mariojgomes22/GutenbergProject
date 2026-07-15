import { Routes } from '@angular/router';
import { BooksList } from './components/books-list/books-list';
import { BookForm } from './components/book-form/book-form';
import { ClientsList } from './components/clients-list/clients-list';
import { ClientForm } from './components/client-form/client-form';
import { LoansManager } from './components/loans-manager/loans-manager';
import { Login } from './components/login/login';
import { Profile } from './components/profile/profile';

import { CategoriesList } from './components/categories-list/categories-list';
import { CategoryForm } from './components/category-form/category-form';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: 'books', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'profile', component: Profile, canActivate: [authGuard] },

    // Books
    { path: 'books', component: BooksList, canActivate: [authGuard] },
    { path: 'books/new', component: BookForm, canActivate: [authGuard] },
    { path: 'books/edit/:id', component: BookForm, canActivate: [authGuard] },

    // Clients
    { path: 'clients', component: ClientsList, canActivate: [authGuard] },
    { path: 'clients/new', component: ClientForm, canActivate: [authGuard] },
    { path: 'clients/edit/:id', component: ClientForm, canActivate: [authGuard] },

    // Loans
    { path: 'loans', component: LoansManager, canActivate: [authGuard] },

    // Categories
    { path: 'categories', component: CategoriesList, canActivate: [authGuard] },
    { path: 'categories/new', component: CategoryForm, canActivate: [authGuard] },
    { path: 'categories/edit/:id', component: CategoryForm, canActivate: [authGuard] }
];
