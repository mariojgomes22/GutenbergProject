import { Component, OnInit, Inject, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';
import { TranslateModule } from '@ngx-translate/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule, MatDialogModule, MatButtonModule],
  templateUrl: './category-form.html',
  styleUrl: './category-form.css'
})
export class CategoryForm implements OnInit {
  categoryForm: FormGroup;
  isEditMode = false;
  categoryId: number = 0;
  submitted = false;

  get isModal(): boolean {
    return !!this.dialogRef;
  }

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    @Optional() public dialogRef: MatDialogRef<CategoryForm>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    if (this.data && this.data.id) {
      this.isEditMode = true;
      this.categoryId = this.data.id;
      this.loadCategory(this.categoryId);
    } else {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.isEditMode = true;
        this.categoryId = +id;
        this.loadCategory(this.categoryId);
      }
    }
  }

  loadCategory(id: number) {
    this.categoryService.getCategory(id).subscribe(category => {
      this.categoryForm.patchValue(category);
    });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.categoryForm.invalid) {
      return;
    }

    const categoryData = this.categoryForm.value;

    if (this.isEditMode && this.categoryId) {
      categoryData.id = this.categoryId;
      this.categoryService.updateCategory(this.categoryId, categoryData).subscribe(() => {
        this.handleSuccess();
      });
    } else {
      this.categoryService.createCategory(categoryData).subscribe(() => {
        this.handleSuccess();
      });
    }
  }

  handleSuccess() {
    if (this.dialogRef) {
      this.dialogRef.close(true);
    } else {
      this.router.navigate(['/categories']);
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    } else {
      this.router.navigate(['/categories']);
    }
  }
}
