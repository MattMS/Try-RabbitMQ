import pika

def callback(ch, method, props, body):
	print(f'{body} handled by Python')

def main():
	conn = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
	channel = conn.channel()

	queue_name = 'hello'

	channel.queue_declare(queue=queue_name)

	channel.basic_consume(auto_ack=True, on_message_callback=callback, queue=queue_name)

	print('Waiting for messages')
	channel.start_consuming()

if __name__ == '__main__':
	# try:
	main()
	# except KeyboardInterrupt:
	# 	print('Done')
