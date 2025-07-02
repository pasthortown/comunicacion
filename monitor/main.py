import os
import json
import time
import pika
from pymongo import MongoClient, ReturnDocument

# Leer variables de entorno
mongo_bdd = os.getenv('mongo_bdd')
mongo_bdd_server = os.getenv('mongo_bdd_server')
mongo_user = os.getenv('mongo_user')
mongo_password = os.getenv('mongo_password')

rabbitmq_host = os.getenv('RABBITMQ_HOST')
rabbitmq_port = int(os.getenv('RABBITMQ_PORT', 5672))
rabbitmq_user = os.getenv('RABBITMQ_USERNAME')
rabbitmq_pass = os.getenv('RABBITMQ_PASSWORD')
rabbitmq_queue = os.getenv('RABBITMQ_QUEUE', 'activity_queue')

# Conexión a MongoDB
mongo_uri = f"mongodb://{mongo_user}:{mongo_password}@{mongo_bdd_server}:27017/"
mongo_client = MongoClient(mongo_uri)
mongo_db = mongo_client[mongo_bdd]
collection = mongo_db["messagereport"]
counters = mongo_db["_counters"]

# Preparar conexión a RabbitMQ
credentials = pika.PlainCredentials(rabbitmq_user, rabbitmq_pass)
parameters = pika.ConnectionParameters(host=rabbitmq_host, port=rabbitmq_port, credentials=credentials)

print("🔄 Iniciando monitor...")

def esperar_rabbitmq():
    intentos = 0
    while True:
        try:
            connection = pika.BlockingConnection(parameters)
            connection.close()
            print("RabbitMQ está disponible.")
            return
        except pika.exceptions.AMQPConnectionError:
            intentos += 1
            print(f"RabbitMQ no disponible. Reintentando en 5 segundos... (intento {intentos})")
            time.sleep(5)

def get_next_sequence(name, cantidad):
    result = counters.find_one_and_update(
        {"_id": name},
        {"$inc": {"seq": cantidad}},
        return_document=ReturnDocument.AFTER,
        upsert=True
    )
    return result["seq"]

def procesar_mensajes():
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()
    channel.queue_declare(queue=rabbitmq_queue, durable=True)

    mensajes = []
    ack_tags = []

    def recolector(ch, method, properties, body):
        try:
            mensaje = json.loads(body.decode("utf-8"))
            mensajes.append(mensaje)
            ack_tags.append(method.delivery_tag)
        except Exception as e:
            print(f"Error procesando mensaje: {e}")
            ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

    channel.basic_qos(prefetch_count=1000)
    for _ in range(10000):
        method_frame, header_frame, body = channel.basic_get(rabbitmq_queue)
        if method_frame:
            recolector(channel, method_frame, header_frame, body)
        else:
            break

    if mensajes:
        print(f"Procesando {len(mensajes)} mensajes desde RabbitMQ...")
        secuencia_final = get_next_sequence("messagereport", len(mensajes))
        secuencia_inicio = secuencia_final - len(mensajes) + 1

        documentos = []
        for i, mensaje in enumerate(mensajes):
            documentos.append({
                "message_id": mensaje.get("message_id"),
                "email": mensaje.get("email"),
                "zona": mensaje.get("zona"),
                "estado": mensaje.get("estado"),
                "timestamp": mensaje.get("timestamp"),
                "item_id": secuencia_inicio + i
            })

        try:
            collection.insert_many(documentos)
            print(f"Insertados {len(documentos)} registros en MongoDB")
            for tag in ack_tags:
                channel.basic_ack(delivery_tag=tag)
        except Exception as e:
            print(f"Error al insertar en MongoDB: {e}")
            for tag in ack_tags:
                channel.basic_nack(delivery_tag=tag, requeue=True)

    connection.close()

if __name__ == "__main__":
    esperar_rabbitmq()
    while True:
        procesar_mensajes()
        time.sleep(600)  # 10 minutos
