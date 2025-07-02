import os
import json
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

# Conexión a RabbitMQ
credentials = pika.PlainCredentials(rabbitmq_user, rabbitmq_pass)
parameters = pika.ConnectionParameters(host=rabbitmq_host, port=rabbitmq_port, credentials=credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()
channel.queue_declare(queue=rabbitmq_queue, durable=True)

print("Esperando mensajes de RabbitMQ...")

def get_next_sequence(name):
    result = counters.find_one_and_update(
        {"_id": name},
        {"$inc": {"seq": 1}},
        return_document=ReturnDocument.AFTER,
        upsert=True
    )
    return result["seq"]

def callback(ch, method, properties, body):
    try:
        mensaje = json.loads(body.decode("utf-8"))

        nuevo_documento = {
            "message_id": mensaje.get("message_id"),
            "email": mensaje.get("email"),
            "zona": mensaje.get("zona"),
            "estado": mensaje.get("estado"),
            "timestamp": mensaje.get("timestamp"),
            "item_id": get_next_sequence("messagereport")
        }

        collection.insert_one(nuevo_documento)
        print(f"Insertado en MongoDB: {nuevo_documento}")

        ch.basic_ack(delivery_tag=method.delivery_tag)

    except Exception as e:
        print(f"Error procesando mensaje: {e}")
        ch.basic_nack(delivery_tag=method.delivery_tag, requeue=False)

channel.basic_qos(prefetch_count=1)
channel.basic_consume(queue=rabbitmq_queue, on_message_callback=callback)

try:
    channel.start_consuming()
except KeyboardInterrupt:
    print("Monitor detenido.")
    channel.stop_consuming()

connection.close()
