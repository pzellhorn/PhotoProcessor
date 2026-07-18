import io

from PIL import Image
from sentence_transformers import SentenceTransformer

from worker_core import results
from worker_core.consumer import BaseConsumer
from worker_core.media import fetch_media_bytes
from worker_core.results import PermanentError

MODEL_NAME = "clip-ViT-B-32"


class ImageEmbeddingWorker(BaseConsumer):
    def __init__(self, queue: str):
        super().__init__(queue)
        self.model = SentenceTransformer(MODEL_NAME, device="cpu")

    def embed(self, image_bytes: bytes) -> list[float]:
        try:
            image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        except Exception as e:
            raise PermanentError(f"could not decode image: {e}")

        embedding = self.model.encode(image, normalize_embeddings=True)
        return embedding.tolist()

    def process(self, job: dict) -> None:
        job_id, media_id = job["jobId"], job["mediaId"]
        results.mark_running(job_id)
        try:
            image_bytes = fetch_media_bytes(media_id)
            embedding = self.embed(image_bytes)
            results.submit_image_embedding(job_id, embedding)
        except PermanentError as e:
            results.mark_failed(job_id, str(e))


if __name__ == "__main__":
    ImageEmbeddingWorker(queue="jobs.image-embedding").run()
