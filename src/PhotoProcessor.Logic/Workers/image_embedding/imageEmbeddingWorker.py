import io

from PIL import Image
from sentence_transformers import SentenceTransformer

from image_embedding import MODEL_NAME
from worker_core import results
from worker_core.consumer import BaseConsumer
from worker_core.file_converter import FileFormat
from worker_core.image_file_converter import ImageFileConverter
from worker_core.results import PermanentError


class ImageEmbeddingWorker(BaseConsumer):
    expected_format = FileFormat.JPEG

    def __init__(self, queue: str):
        super().__init__(queue, ImageFileConverter())
        self.model = SentenceTransformer(MODEL_NAME, device="cpu")

    def embed(self, image_bytes: bytes) -> list[float]:
        try:
            image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        except Exception as e:
            raise PermanentError(f"could not decode image: {e}")

        embedding = self.model.encode(image, normalize_embeddings=True)
        return embedding.tolist()

    def process(self, job: dict, media: bytes) -> None:
        embedding = self.embed(media)
        results.submit_image_embedding(job["jobId"], embedding)


if __name__ == "__main__":
    ImageEmbeddingWorker(queue="jobs.image-embedding").run()
