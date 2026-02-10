import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Book } from '../models/book.model';
import { environment } from '../../environments/environment';

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}

@Injectable({
    providedIn: 'root'
})
export class BookService {
    private apiUrl = `${environment.apiUrl}/books`;

    constructor(private http: HttpClient) { }

    getBooks(search: string = '', page: number = 1, pageSize: number = 10): Observable<PagedResult<Book>> {
        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);

        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<PagedResult<Book>>(this.apiUrl, { params });
    }

    getBook(id: number): Observable<Book> {
        return this.http.get<Book>(`${this.apiUrl}/${id}`);
    }

    addBook(book: Book): Observable<Book> {
        return this.http.post<Book>(this.apiUrl, book);
    }

    updateBook(id: number, book: Book): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, book);
    }

    deleteBook(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
