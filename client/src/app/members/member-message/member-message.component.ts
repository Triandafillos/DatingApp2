import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { Member } from '../../_models/member';
import { Message } from '../../_models/message';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-message',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-message.component.html',
  styleUrl: './member-message.component.css'
})
export class MemberMessageComponent {
  @ViewChild('messageForm') messageForm?: NgForm;
  private messageService = inject(MessageService);
  username = input.required<string>();
  messages = input.required<Message[]>();
  messageContent = '';
  updateMessage = output<Message>();

  sendMessage() {
    this.messageService.sendMessage(this.username(), this.messageContent).subscribe({
      next: message => {
        this.updateMessage.emit(message);
        this.messageForm?.reset();
      }
    })
  }

}
