# Imagen base de Python
FROM python:3.11-slim

# Establecer directorio de trabajo
WORKDIR /app

# Copiar archivos al contenedor
COPY requirements.txt requirements.txt
COPY ws.py ws.py

# Instalar dependencias
RUN pip install --no-cache-dir -r requirements.txt

# Puerto de escucha
EXPOSE 5050

# Ejecutar el servicio
CMD ["python", "ws.py"]
