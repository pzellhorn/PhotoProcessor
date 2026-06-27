from worker_core.consumer import BaseConsumer
from worker_core.media import fetch_media_bytes


class FaceRecognitionWorker(BaseConsumer):
    def process(self, job: dict) -> None:
        image_bytes = fetch_media_bytes(job["mediaId"]) 
        


if __name__ == "__main__":
    FaceRecognitionWorker(queue="jobs").run()
