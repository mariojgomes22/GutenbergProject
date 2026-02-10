import { Routes } from '@angular/router';
import { BooksList } from './components/books-list/books-list';
import { BookForm } from './components/book-form/book-form';
import { ClientsList } from './components/clients-list/clients-list';
import { ClientForm } from './components/client-form/client-form';
import { LoansManager } from './components/loans-manager/loans-manager';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Profile } from './components/profile/profile';

import { CategoriesList } from './components/categories-list/categories-list';
import { CategoryForm } from './components/category-form/category-form';

export const routes: Routes = [
    { path: '', redirectTo: 'books', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: 'profile', component: Profile },

    // Books
    { path: 'books', component: BooksList },
    { path: 'books/new', component: BookForm },
    { path: 'books/edit/:id', component: BookForm },

    // Clients
    { path: 'clients', component: ClientsList },
    { path: 'clients/new', component: ClientForm },
    { path: 'clients/edit/:id', component: ClientForm },

    // Loans
    { path: 'loans', component: LoansManager },

    // Categories
    { path: 'categories', component: CategoriesList },
    { path: 'categories/new', component: CategoryForm },
    { path: 'categories/edit/:id', component: CategoryForm }
];
