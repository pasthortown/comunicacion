import { Component } from '@angular/core';

@Component({
  selector: 'app-image-message',
  imports: [],
  templateUrl: './image-message.component.html',
  styleUrl: './image-message.component.scss'
})
export class ImageMessageComponent {

  imagenCargada: string | null = null;

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.leerArchivo(files[0]);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.leerArchivo(input.files[0]);
    }
  }

  leerArchivo(file: File) {
    if (!file.type.startsWith('image/')) return;

    const reader = new FileReader();
    reader.onload = () => {
      this.imagenCargada = reader.result as string;
    };
    reader.readAsDataURL(file);
  }
}
