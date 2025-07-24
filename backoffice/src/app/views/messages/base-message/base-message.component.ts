import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Message } from '../../../models/message.model';

@Component({
  selector: 'app-base-message',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './base-message.component.html',
  styleUrl: './base-message.component.scss'
})
export class BaseMessageComponent {
  @Input() model!: Message;
  @Output() modelChange = new EventEmitter<Message>();
  @Output() tipoCambiado = new EventEmitter<string>();

  onTipoChange(nuevoTipo: string) {
    this.tipoCambiado.emit(nuevoTipo);
  }

  onDuracionChange(_duracion: number) {
    this.modelChange.emit(this.model);
  }

  onLinkChange(_link: string) {
    this.modelChange.emit(this.model);
  }
}
