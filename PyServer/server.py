# from contextlib import asynccontextmanager
from typing import Annotated

from fastapi import Depends, FastAPI
import pika
# from pika.adapters.asyncio_connection import AsyncioConnection
from pika.adapters.blocking_connection import BlockingChannel

queue_name = 'hello'

def get_rabbitmq_channel():
	conn = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
	channel = conn.channel()

	channel.queue_declare(queue=queue_name)

	try:
		yield lambda message: channel.basic_publish(body=message, exchange='', routing_key=queue_name)
	finally:
		conn.close()

app = FastAPI()

@app.get('/')
def home():
	return {'Ok': True}

@app.get('/send/{message}')
def send(message: str, send: Annotated[BlockingChannel, Depends(get_rabbitmq_channel)]):
	send(f'{message} published by FastAPI')
	return {'Ok': True}
