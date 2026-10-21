import cv2
import numpy as np
from insightface.app import FaceAnalysis

from worker_core import results
from worker_core.consumer import BaseConsumer
from worker_core.file_converter import FileFormat
from worker_core.image_file_converter import ImageFileConverter
from worker_core.results import PermanentError


class FaceRecognitionWorker(BaseConsumer):
    expected_format = FileFormat.JPEG

    def __init__(self, queue: str):
        super().__init__(queue, ImageFileConverter())
        # ArcFace buffalo
        self.app = FaceAnalysis(name="buffalo_l", providers=["CPUExecutionProvider"])
        self.app.prepare(ctx_id=-1, det_size=(640, 640))   

    def detect(self, image_bytes: bytes) -> list[dict]:
        image = cv2.imdecode(np.frombuffer(image_bytes, np.uint8), cv2.IMREAD_COLOR)
        if image is None:
            raise PermanentError("could not decode image")

        faces = []
        for face in self.app.get(image):
            x1, y1, x2, y2 = face.bbox
            faces.append({
                "embedding": face.normed_embedding.tolist(),
                "detectionScore": float(face.det_score),
                "boundingX": float(x1),
                "boundingY": float(y1),
                "boundingWidth": float(x2 - x1),
                "boundingHeight": float(y2 - y1),
            })
        return faces

    def process(self, job: dict, media: bytes) -> None:
        faces = self.detect(media)
        results.submit_fingerprints(job["jobId"], faces)

if __name__ == "__main__":
    FaceRecognitionWorker(queue="jobs.face-recognition").run()
