import { Component } from '@angular/core';

@Component({
  selector: 'app-zone-screen',
  imports: [],
  templateUrl: './zone-screen.component.html',
  styleUrl: './zone-screen.component.scss'
})
export class ZoneScreenComponent {
  matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
  ];

  hovered: number | null = null;
  seleccion: number | null = null;

  seleccionar(valor: number) {
    this.seleccion = valor;
    console.log('Celda seleccionada:', valor);
  }
}
