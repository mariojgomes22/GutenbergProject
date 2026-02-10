import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';
import { TranslateModule } from '@ngx-translate/core';
import { CategoryForm } from '../category-form/category-form';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';


@Component({
  selector: 'app-categories-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, MatDialogModule],
  templateUrl: './categories-list.html',
  styleUrl: './categories-list.css'
})
export class CategoriesList implements OnInit {
  categories: Category[] = [];

  constructor(
    private categoryService: CategoryService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe(data => {
      this.categories = data;
    });
  }

  deleteCategory(id: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Category',
        message: 'Are you sure you want to delete this category?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categoryService.deleteCategory(id).subscribe(() => {
          this.loadCategories();
        });
      }
    });
  }

  openModal(id: number = 0): void {
    const dialogRef = this.dialog.open(CategoryForm, {
      width: '600px',
      data: { id: id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCategories();
      }
    });
  }
}
