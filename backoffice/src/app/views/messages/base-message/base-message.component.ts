import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-base-message',
  imports: [],
  templateUrl: './base-message.component.html',
  styleUrl: './base-message.component.scss'
})
export class BaseMessageComponent {
  @Output() tipoSeleccionado = new EventEmitter<string>();

  onTipoChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.tipoSeleccionado.emit(select.value);
  }
}
