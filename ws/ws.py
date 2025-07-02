import os
import json
import datetime
import jwt
from bson import json_util
from pymongo import MongoClient
from tornado.ioloop import IOLoop
from tornado.web import Application, RequestHandler
from tornado.escape import json_decode
from dateutil import parser

# Variables de entorno para MongoDB
mongo_bdd = os.getenv('mongo_bdd')
mongo_bdd_server = os.getenv('mongo_bdd_server')
mongo_user = os.getenv('mongo_user')
mongo_password = os.getenv('mongo_password')
jwt_secret = os.getenv('jwt_secret', 'supersecreto')

# Conexión a MongoDB
client = MongoClient(f'mongodb://{mongo_user}:{mongo_password}@{mongo_bdd_server}/')
db = client[mongo_bdd]
counter_collection = db['_counters']

# Utilidad para CORS y autenticación
class BaseHandler(RequestHandler):
    def set_default_headers(self):
        self.set_header("Access-Control-Allow-Origin", "*")
        self.set_header("Access-Control-Allow-Headers", "Authorization, Content-Type")
        self.set_header("Access-Control-Allow-Methods", "GET, POST, PATCH, DELETE, OPTIONS")

    def options(self, *args, **kwargs):
        self.set_status(204)
        self.finish()

    def prepare(self):
        if self.request.method != "OPTIONS":
            auth_header = self.request.headers.get("Authorization", "")
            if not auth_header.startswith("Bearer "):
                self.set_status(401)
                self.finish({"error": "Token no proporcionado"})
                return

            token = auth_header.replace("Bearer ", "")
            try:
                jwt.decode(token, jwt_secret, algorithms=["HS256"])
            except jwt.ExpiredSignatureError:
                self.set_status(401)
                self.finish({"error": "Token expirado"})
            except jwt.InvalidTokenError:
                self.set_status(401)
                self.finish({"error": "Token inválido"})

# Ruta de estado base
class DefaultHandler(BaseHandler):
    def get(self):
        self.write({'response': 'WS Comunication Operative', 'status': 200})

# Ruta RESTful de catálogo
class CatalogHandler(BaseHandler):
    def post(self, catalog):
        data = json_decode(self.request.body)
        collection = db[catalog]
        item_id = get_next_id(catalog)
        data['item_id'] = item_id
        data['timestamp'] = datetime.datetime.utcnow().isoformat()
        result = collection.insert_one(data)
        data['_id'] = result.inserted_id
        self.write({'response': json.loads(json_util.dumps(data)), 'status': 200})

    def get(self, catalog):
        collection = db[catalog]
        item_id = self.get_query_argument('id', None)
        output_model = self.get_argument('output_model', default=None)
        projection = build_projection(output_model)

        if item_id:
            try:
                item_id = int(item_id)
            except ValueError:
                self.set_status(400)
                return self.write({'error': 'item_id debe ser un entero'})
            result = collection.find_one({"item_id": item_id}, projection)
            if result:
                self.write({'response': json.loads(json_util.dumps(result)), 'status': 200})
            else:
                self.set_status(404)
                self.write({'response': 'Elemento no encontrado', 'status': 404})
        else:
            cursor = collection.find({}, projection)
            items = list(cursor)
            self.write({'response': json.loads(json_util.dumps(items)), 'status': 200})

    def patch(self, catalog):
        data = json_decode(self.request.body)
        item_id = data.get("item_id")
        if item_id is None:
            self.set_status(400)
            return self.write({'error': 'item_id es obligatorio'})

        collection = db[catalog]
        data['timestamp'] = datetime.datetime.utcnow().isoformat()
        result = collection.update_one({"item_id": int(item_id)}, {"$set": data})
        if result.matched_count == 0:
            self.set_status(404)
            return self.write({'response': 'Elemento no encontrado', 'status': 404})
        self.write({'response': data, 'status': 200})

    def delete(self, catalog):
        item_id = self.get_query_argument('id', None)
        if not item_id:
            self.set_status(400)
            return self.write({'error': 'Debe enviar item_id en la URL'})

        collection = db[catalog]
        result = collection.delete_one({"item_id": int(item_id)})
        if result.deleted_count == 0:
            self.set_status(404)
            return self.write({'response': 'Elemento no encontrado', 'status': 404})
        self.write({'response': 'Elemento eliminado', 'status': 200})

# Generador de ID autoincremental por catálogo
def get_next_id(catalog):
    result = counter_collection.find_one_and_update(
        {'_id': catalog},
        {'$inc': {'seq': 1}},
        upsert=True,
        return_document=True
    )
    return result['seq']

# Construcción de proyección opcional
def build_projection(output_model_raw):
    if not output_model_raw:
        return None
    try:
        model = json.loads(output_model_raw)
        model['_id'] = False
        model['item_id'] = True
        model['timestamp'] = True
        return model
    except Exception:
        return None

# Crear app Tornado
def make_app():
    return Application([
        (r"/", DefaultHandler),
        (r"/([^/]+)", CatalogHandler)
    ], debug=True)

# Iniciar servidor
if __name__ == "__main__":
    print("Servidor escuchando en http://localhost:5050")
    app = make_app()
    app.listen(5050)
    IOLoop.current().start()
