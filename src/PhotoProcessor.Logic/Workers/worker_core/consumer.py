import json, os, signal, sys
import pika

from worker_core import results
from worker_core.file_converter import FileFormat, IFileConverter
from worker_core.media import fetch_media_bytes
from worker_core.results import PermanentError

class BaseConsumer:
    # The format process() receives media in. None: process() gets no media and fetches the original itself.
    expected_format: FileFormat | None = None

    def __init__(self, queue: str, converter: IFileConverter | None = None):
        if self.expected_format is not None and converter is None:
            raise ValueError(f"{type(self).__name__} expects {self.expected_format.name} media but has no converter")
        self.queue = queue
        self.converter = converter

    def _params(self):
        return pika.ConnectionParameters(
            host=os.getenv("RABBITMQ_HOST", "localhost"),
            port=int(os.getenv("RABBITMQ_PORT", "5672")),
            virtual_host=os.getenv("RABBITMQ_VHOST", "/"),
            credentials=pika.PlainCredentials(
                os.getenv("RABBITMQ_USER", "app"),
                os.getenv("RABBITMQ_PASS", "app")),
        )

    def process(self, job: dict, media: bytes | None) -> None:
        raise NotImplementedError

    def _handle(self, job: dict) -> None:
        job_id = job["jobId"]
        results.mark_running(job_id)
        try:
            media = self._fetch_media(job["mediaId"])
            self.process(job, media)
        except PermanentError as e:
            results.mark_failed(job_id, str(e))

    def _fetch_media(self, media_id: str) -> bytes | None:
        if self.expected_format is None:
            return None

        media = self.converter.convert(fetch_media_bytes(media_id), self.expected_format)
        if not self.expected_format.matches(media):
            raise PermanentError(f"{type(self.converter).__name__} did not produce {self.expected_format.name}")
        return media

    def _on_message(self, ch, method, _props, body):
        try:
            self._handle(json.loads(body))
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