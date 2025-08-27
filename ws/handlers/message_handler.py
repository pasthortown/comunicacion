import json
from bson import json_util
from base import db, BaseHandler

class MessageHandler(BaseHandler):
    def get(self, message_id):
        collection = db["messagereport"]
        result = list(collection.find({"message_id": int(message_id)}))
        if result:
            self.write({'response': json.loads(json_util.dumps(result)), 'status': 200})
        else:
            self.set_status(404)
            self.write({'response': 'Mensaje no encontrado', 'status': 404})
