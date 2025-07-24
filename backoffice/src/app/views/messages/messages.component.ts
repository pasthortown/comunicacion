import { Component } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonDirective,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  RowComponent
} from '@coreui/angular';
import { BaseMessageComponent } from './base-message/base-message.component';
import { ImageMessageComponent } from './image-message/image-message.component';
import { TextMessageComponent } from './text-message/text-message.component';
import { ZoneScreenComponent } from './zone-screen/zone-screen.component';
import { Message } from '../../models/message.model';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
  imports: [ContainerComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective, BaseMessageComponent, ImageMessageComponent, TextMessageComponent, ZoneScreenComponent]
})
export class MessagesComponent {

   message: Message = {
    tipo: 'Imagen',
    content: '',
    duracion: 10,
    link: 'https://ec.linkedin.com/company/gurpo-kfc-ecuador',
    zona: 9,
    ancho: 300
  };

  actualizarTipo(tipo: string) {
    this.message.tipo = tipo;

    if (tipo === 'Texto') {
      this.message.content = { titulo: '', texto: '' };
      this.message.ancho = undefined;
    } else if (tipo === 'Imagen') {
      this.message.content = '';
    }
  }

  onTipoCambiado(nuevoTipo: string) {
    this.message.tipo = nuevoTipo;

    if (nuevoTipo === 'Texto') {
      this.message.content = { titulo: '', texto: '' };
      this.message.ancho = undefined;
    } else if (nuevoTipo === 'Imagen') {
      this.message.content = '';
    }
  }

  prueba() {
    console.log('Mensaje generado:', this.message);
  }
}
