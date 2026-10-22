from flower_identification.species import SpeciesClassifier, decode_upright
from worker_core import results
from worker_core.consumer import BaseConsumer
from worker_core.file_converter import FileFormat
from worker_core.image_file_converter import ImageFileConverter

TOP_PREDICTIONS = 5


class FlowerIdentificationWorker(BaseConsumer):
    expected_format = FileFormat.JPEG

    def __init__(self, queue: str):
        super().__init__(queue, ImageFileConverter())
        self.classifier = SpeciesClassifier()

    def process(self, job: dict, media: bytes) -> None:
        image = decode_upright(media)
        embedding = self.classifier.embed(image)

        results.submit_flower_identification(
            job["jobId"],
            embedding=embedding.tolist(),
            predictions=self.classifier.predict(embedding, TOP_PREDICTIONS),
        )


if __name__ == "__main__":
    FlowerIdentificationWorker(queue="jobs.flower-identification").run()
