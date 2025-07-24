import json
import bcrypt
import os
from bson import json_util
from tornado.escape import json_decode
from base import db, BaseHandler
from helpers import get_next_id

bcrypt_salt = int(os.getenv("bcrypt_salt", 12))
collection = db["managers"]

class BackofficeUserHandler(BaseHandler):
    def get(self, username=None):
        if username:
            result = collection.find_one(
                {"username": username},
                {"_id": False, "password": False}
            )
            if result:
                self.write({'response': result, 'status': 200})
            else:
                self.set_status(404)
                self.write({'response': 'Usuario no encontrado', 'status': 404})
        else:
            users = list(collection.find({}, {"_id": False, "password": False}))
            self.write({'response': users, 'status': 200})

    def post(self):
        data = json_decode(self.request.body)
        if not data.get("username") or not data.get("password"):
            self.set_status(400)
            return self.write({'response': 'username y password son requeridos', 'status': 400})

        data["id"] = get_next_id("managers")
        hashed = bcrypt.hashpw(data["password"].encode(), bcrypt.gensalt(bcrypt_salt))
        data["password"] = hashed.decode()

        result = collection.insert_one(data)

        response_data = {k: v for k, v in data.items() if k != "password"}
        response_data.pop("_id", None)
        self.write({'response': response_data, 'status': 201})

    def patch(self):
        data = json_decode(self.request.body)
        user_id = data.get("id")
        if not user_id:
            self.set_status(400)
            return self.write({'response': 'id es requerido para actualizar', 'status': 400})

        update_data = {}
        if data.get("username"):
            update_data["username"] = data["username"]
        if data.get("password"):
            hashed = bcrypt.hashpw(data["password"].encode(), bcrypt.gensalt(bcrypt_salt))
            update_data["password"] = hashed.decode()

        result = collection.update_one({"id": user_id}, {"$set": update_data})

        if result.matched_count == 0:
            self.set_status(404)
            return self.write({'response': 'Usuario no encontrado', 'status': 404})

        update_data.pop("password", None)
        self.write({'response': update_data, 'status': 200})

    def delete(self):
        user_id = self.get_query_argument("id", None)
        if not user_id:
            self.set_status(400)
            return self.write({'response': 'id es requerido', 'status': 400})

        result = collection.delete_one({"id": int(user_id)})
        if result.deleted_count:
            self.write({'response': 'Usuario eliminado', 'status': 200})
        else:
            self.set_status(404)
            self.write({'response': 'Usuario no encontrado', 'status': 404})

class BackofficeLoginHandler(BaseHandler):
    def post(self):
        data = json_decode(self.request.body)
        username = data.get("username")
        password = data.get("password")

        if not username or not password:
            self.set_status(400)
            return self.write({'response': 'username y password son requeridos', 'status': 400})

        user = collection.find_one({"username": username})
        allowed = False

        if user and bcrypt.checkpw(password.encode(), user["password"].encode()):
            allowed = True

        self.write({'username': username, 'allowed': allowed, 'status': 200})
