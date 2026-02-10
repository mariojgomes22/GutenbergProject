import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookService, PagedResult } from '../../services/book.service';
import { AuthService } from '../../services/auth.service';
import { LoanService } from '../../services/loan.service';
import { Book } from '../../models/book.model';
import { Loan } from '../../models/loan.model';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';
import { BookForm } from '../book-form/book-form';

@Component({
  selector: 'app-books-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule, MatDialogModule, MatPaginatorModule],
  templateUrl: './books-list.html',
  styleUrl: './books-list.css'
})
export class BooksList implements OnInit {
  books: Book[] = [];
  message: string = '';

  // Pagination & Search
  searchQuery: string = '';
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private bookService: BookService,
    public authService: AuthService,
    private loanService: LoanService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadBooks();
  }

  loadBooks(): void {
    // MatPaginator uses 0-based index, backend uses 1-based
    this.bookService.getBooks(this.searchQuery, this.currentPage, this.pageSize)
      .subscribe((data: PagedResult<Book>) => {
        this.books = data.items;
        this.totalCount = data.totalCount;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
      });
  }

  onSearch(): void {
    this.currentPage = 1; // Reset to first page
    this.loadBooks();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1; // Convert 0-based to 1-based
    this.pageSize = event.pageSize;
    this.loadBooks();
  }

  deleteBook(id: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Book',
        message: 'Are you sure you want to delete this book?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.bookService.deleteBook(id).subscribe(() => {
          this.loadBooks();
        });
      }
    });
  }

  openModal(id: number = 0): void {
    const dialogRef = this.dialog.open(BookForm, {
      width: '600px',
      data: { id: id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadBooks();
      }
    });
  }

  requestBook(book: Book): void {
    const currentUser = this.authService.currentUser();
    if (!currentUser) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Request Book',
        message: `Request the book "${book.title}"?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const loan: Loan = {
          id: 0,
          bookId: book.id,
          clientId: currentUser.id
        };

        this.loanService.createLoan(loan).subscribe({
          next: () => {
            this.message = `Book "${book.title}" requested successfully!`;
            this.loadBooks();
            setTimeout(() => this.message = '', 3000);
          },
          error: () => {
            this.message = 'Error requesting book.';
          }
        });
      }
    });
  }
}
