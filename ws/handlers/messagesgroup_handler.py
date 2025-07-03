import json
from bson import json_util
from datetime import datetime, timedelta
from base import db, BaseHandler

class MessagesGroupHandler(BaseHandler):
    def get(self):
        collection = db["messagesgroup"]

        # Calcular rango de fechas del día UTC actual
        today = datetime.utcnow().date()
        start_of_day = datetime.combine(today, datetime.min.time())
        end_of_day = datetime.combine(today, datetime.max.time())

        # Buscar todos los documentos donde schedule esté en el día actual
        query = {
            "schedule": {
                "$gte": start_of_day,
                "$lte": end_of_day
            }
        }

        result = list(collection.find(query))
        if result:
            self.write({
                'response': json.loads(json_util.dumps(result)),
                'status': 200
            })
        else:
            self.set_status(404)
            self.write({
                'response': 'No se encontraron registros para hoy',
                'status': 404
            })
