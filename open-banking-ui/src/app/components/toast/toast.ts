import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast';


@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-20 right-5 z-[9999] flex flex-col gap-3 pointer-events-none">
      
      @for (toast of toastService.toasts$ | async; track toast.id) {
        <div class="pointer-events-auto px-4 py-3 rounded-xl shadow-lg border text-sm font-bold flex items-center justify-between min-w-[300px] animate-fade-in-down transition-all"
             [class.bg-green-50]="toast.type === 'success'" [class.border-green-200]="toast.type === 'success'" [class.text-green-800]="toast.type === 'success'"
             [class.bg-red-50]="toast.type === 'error'" [class.border-red-200]="toast.type === 'error'" [class.text-red-800]="toast.type === 'error'"
             [class.bg-blue-50]="toast.type === 'info'" [class.border-blue-200]="toast.type === 'info'" [class.text-blue-800]="toast.type === 'info'">
          
          <div class="flex items-center gap-3">
            <span *ngIf="toast.type === 'success'" class="text-xl">✅</span>
            <span *ngIf="toast.type === 'error'" class="text-xl">🚨</span>
            <span *ngIf="toast.type === 'info'" class="text-xl">ℹ️</span>
            {{ toast.message }}
          </div>
          
          <button (click)="toastService.remove(toast.id)" class="opacity-50 hover:opacity-100 transition-opacity ml-4 text-lg">&times;</button>
        </div>
      }
      
    </div>
  `
})
export class ToastComponent {
  constructor(public toastService: ToastService) { }
}
