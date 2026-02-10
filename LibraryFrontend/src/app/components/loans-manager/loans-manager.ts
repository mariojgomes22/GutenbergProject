import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LoanService } from '../../services/loan.service';
import { BookService, PagedResult } from '../../services/book.service';
import { ClientService } from '../../services/client.service';
import { Loan } from '../../models/loan.model';
import { Book } from '../../models/book.model';
import { Client } from '../../models/client.model';

import { TranslateModule } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';

@Component({
    selector: 'app-loans-manager',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, TranslateModule, MatDialogModule],
    templateUrl: './loans-manager.html',
    styleUrl: './loans-manager.css'
})
export class LoansManager implements OnInit {
    loans: Loan[] = [];
    books: Book[] = [];
    clients: Client[] = [];
    loanForm: FormGroup;
    message: string = '';
    showActiveOnly: boolean = false;

    get filteredLoans(): Loan[] {
        if (this.showActiveOnly) {
            return this.loans.filter(loan => loan.returnDate === null);
        }
        return this.loans;
    }

    constructor(
        private loanService: LoanService,
        private bookService: BookService,
        private clientService: ClientService,
        private fb: FormBuilder,
        private dialog: MatDialog
    ) {
        this.loanForm = this.fb.group({
            bookId: ['', Validators.required],
            clientId: ['', Validators.required]
        });
    }

    ngOnInit(): void {
        this.loadData();
    }

    loadData(): void {
        this.loadLoans();
        this.loadBooks();
        this.loadClients();
    }

    loadLoans(): void {
        this.loanService.getLoans().subscribe(data => {
            this.loans = data;
        });
    }

    loadBooks(): void {
        // Get all books for dropdown (using large page size)
        this.bookService.getBooks('', 1, 1000).subscribe(data => {
            // Filter only available books for the dropdown
            this.books = data.items.filter(b => b.isAvailable);
        });
    }

    loadClients(): void {
        this.clientService.getClients().subscribe(data => {
            this.clients = data;
        });
    }

    createLoan(): void {
        if (this.loanForm.invalid) return;

        const loan: Loan = {
            id: 0,
            bookId: +this.loanForm.value.bookId,
            clientId: +this.loanForm.value.clientId
        };

        this.loanService.createLoan(loan).subscribe({
            next: () => {
                this.message = 'Loan created successfully!';
                this.loanForm.reset();
                this.loadData(); // Reload loans and books (availability updates)
                setTimeout(() => this.message = '', 3000);
            },
            error: (err) => {
                this.message = 'Error creating loan.';
                console.error(err);
            }
        });
    }

    returnLoan(id: number): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: '350px',
            data: {
                title: 'Return Book',
                message: 'Return this book?'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.loanService.returnLoan(id).subscribe({
                    next: () => {
                        this.loadData();
                    },
                    error: (err) => console.error(err)
                });
            }
        });
    }
}
