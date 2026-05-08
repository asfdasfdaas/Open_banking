import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast';
import { BankApiService } from '../../services/bank-api';

@Component({
  selector: 'app-account-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './account-create.html'
})
export class AccountCreateComponent {
  createForm: FormGroup;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private bankApi: BankApiService,
    private router: Router,
    private toastService: ToastService
  ) {
    // only user input
    this.createForm = this.fb.group({
      currencyCode: ['TL', Validators.required],
      balance: [0, [Validators.required, Validators.min(0)]]
    });
  }

  onSubmit() {
    if (this.createForm.invalid) return;

    this.isSubmitting = true;

    // The form value ONLY contains { balance: X, currencyCode: "TRY" }
    // which perfectly matches our new C# DTO!
    const payload = this.createForm.value;

    this.bankApi.createAccount(payload).subscribe({
      next: () => {
        this.toastService.show('Account successfully opened!', 'success');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error(err);
        this.toastService.show('Failed to open account.', 'error');
        this.isSubmitting = false;
      }
    });
  }
}
