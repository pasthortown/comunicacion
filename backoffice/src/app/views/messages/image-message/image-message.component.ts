import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Message } from '../../../models/message.model';

@Component({
  selector: 'app-image-message',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './image-message.component.html',
  styleUrl: './image-message.component.scss'
})
export class ImageMessageComponent implements OnChanges {
  @Input() model!: Message;
  @Output() modelChange = new EventEmitter<Message>();

  imagenCargada: string | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['model'] && this.model?.content && typeof this.model.content === 'string') {
      this.imagenCargada = this.model.content;
    }
  }

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
      const base64 = reader.result as string;
      this.imagenCargada = base64;
      this.model.content = base64;
      this.modelChange.emit(this.model);
    };
    reader.readAsDataURL(file);
  }

  emitirAncho(event: Event) {
    this.modelChange.emit(this.model);
  }
}
