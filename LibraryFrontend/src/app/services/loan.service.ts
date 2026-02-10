import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Loan } from '../models/loan.model';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class LoanService {
    private apiUrl = `${environment.apiUrl}/loans`;

    constructor(private http: HttpClient) { }

    getLoans(clientId?: number): Observable<Loan[]> {
        let params = {};
        if (clientId) {
            params = { clientId: clientId.toString() };
        }
        return this.http.get<Loan[]>(this.apiUrl, { params });
    }

    createLoan(loan: Loan): Observable<Loan> {
        return this.http.post<Loan>(this.apiUrl, loan);
    }

    returnLoan(id: number): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/return`, {});
    }
}
