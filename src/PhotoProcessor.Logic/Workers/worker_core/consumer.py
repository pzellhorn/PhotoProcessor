import json, os, signal, sys
import pika

class BaseConsumer:
    def __init__(self, queue: str):
        self.queue = queue

    def _params(self):
        return pika.ConnectionParameters(
            host=os.getenv("RABBITMQ_HOST", "localhost"),
            port=int(os.getenv("RABBITMQ_PORT", "5672")),
            virtual_host=os.getenv("RABBITMQ_VHOST", "/"),
            credentials=pika.PlainCredentials(
                os.getenv("RABBITMQ_USER", "app"),
                os.getenv("RABBITMQ_PASS", "app")),
        )

    def process(self, job: dict) -> None:
        raise NotImplementedError  

    def _on_message(self, ch, method, _props, body):
        try:
            self.process(json.loads(body))
            ch.basic_ack(method.delivery_tag)
        except Exception as e:
            print(f"job failed, requeueing: {e}", file=sys.stderr)
            ch.basic_nack(method.delivery_tag, requeue=True)

    def run(self):
        conn = pika.BlockingConnection(self._params())
        ch = conn.channel()
        ch.queue_declare(queue=self.queue, durable=True)
        ch.basic_qos(prefetch_count=1)
        ch.basic_consume(self.queue, self._on_message, auto_ack=False)
        signal.signal(signal.SIGTERM, lambda *_: ch.stop_consuming())
        print(f"[{self.queue}] waiting for jobs...")
        ch.start_consuming()
        conn.close()