export interface TextoContent {
  titulo: string;
  texto: string;
}

export type MessageContent = string | TextoContent;

export interface Message {
  tipo: string;
  content: MessageContent;
  duracion: number;
  link: string;
  zona?: number;
  ancho?: number;
}
