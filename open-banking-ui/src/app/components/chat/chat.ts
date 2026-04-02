import { Component, ElementRef, ViewChild, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BankApiService } from '../../services/bank-api';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.scss'
})
export class ChatComponent implements AfterViewChecked {
  isOpen: boolean = false;
  userInput: string = '';
  isLoading: boolean = false;

  // Stores the conversation history for the UI
  messages: { sender: 'user' | 'ai', text: string }[] = [];


  @ViewChild('chatScroll') private chatScrollContainer!: ElementRef;

  constructor(private bankApi: BankApiService, private cdr: ChangeDetectorRef) {
    // Initial greeting when they open the app
    this.messages.push({
      sender: 'ai',
      text: 'Hello! I am your AI financial assistant. How can I help you manage your accounts today?'
    });
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  sendMessage() {
    if (!this.userInput.trim()) return;

    const text = this.userInput;

    // 1.show the user's message in the UI
    this.messages.push({ sender: 'user', text: text });
    this.userInput = '';
    this.isLoading = true;

    // 2. Send it to .NET backend
    this.bankApi.sendChatMessage(text).subscribe({
      next: (res) => {
        // 3. Catch the AI's reply and display it
        this.messages.push({ sender: 'ai', text: res.reply });
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error("AI Error:", err);
        this.messages.push({ sender: 'ai', text: 'Sorry, I am having trouble connecting to the server.' });
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Forces the chat window to scroll down every time a new message appears
  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      // The '?' safely checks if the container exists before trying to read nativeElement!
      if (this.chatScrollContainer?.nativeElement) {
        this.chatScrollContainer.nativeElement.scrollTop = this.chatScrollContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      // (Also fixed the typo in your console log from top -> bottom!)
      console.error('Failed to scroll to bottom', err);
    }
  }
 }
 

