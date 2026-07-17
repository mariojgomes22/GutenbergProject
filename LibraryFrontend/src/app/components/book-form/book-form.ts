import { Component, OnInit, Inject, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { BookService } from '../../services/book.service';
import { CategoryService } from '../../services/category.service';
import { Book } from '../../models/book.model';
import { Category } from '../../models/category.model';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-book-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule, MatDialogModule, MatButtonModule],
  templateUrl: './book-form.html',
  styleUrl: './book-form.css'
})
export class BookForm implements OnInit {
  bookForm: FormGroup;
  isEditMode = false;
  bookId: number = 0;
  submitted = false;
  categories: Category[] = [];

  get isModal(): boolean {
    return !!this.dialogRef;
  }

  constructor(
    private fb: FormBuilder,
    private bookService: BookService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    @Optional() public dialogRef: MatDialogRef<BookForm>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.bookForm = this.fb.group({
      title: ['', Validators.required],
      author: ['', Validators.required],
      isAvailable: [true],
      categoryId: ['']
    });
  }

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe(data => {
      this.categories = data;
    });

    // Check if we have dialog data (Modal Mode) or Route data (Page Mode)
    if (this.data && this.data.id) {
      this.isEditMode = true;
      this.bookId = this.data.id;
      this.loadBook(this.bookId);
    } else {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.isEditMode = true;
        this.bookId = +id;
        this.loadBook(this.bookId);
      }
    }
  }

  // Load book for editing
  loadBook(id: number) {
    this.bookService.getBook(id).subscribe(book => {
      this.bookForm.patchValue(book);
    });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.bookForm.invalid) {
      return;
    }

    const bookData = this.bookForm.value;

    if (this.isEditMode && this.bookId) {
      bookData.id = this.bookId;
      this.bookService.updateBook(this.bookId, bookData).subscribe(() => {
        this.handleSuccess();
      });
    } else {
      this.bookService.addBook(bookData).subscribe(() => {
        this.handleSuccess();
      });
    }
  }

  handleSuccess() {
    if (this.dialogRef) {
      this.dialogRef.close(true);
    } else {
      this.router.navigate(['/books']);
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    } else {
      this.router.navigate(['/books']);
    }
  }
}
