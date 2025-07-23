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

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
  imports: [ContainerComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective, BaseMessageComponent, ImageMessageComponent, TextMessageComponent, ZoneScreenComponent]
})
export class MessagesComponent {

  tipoMensaje: string = 'Imagen';

  actualizarTipo(tipo: string) {
    this.tipoMensaje = tipo;
    console.log(this.tipoMensaje)
  }
}
