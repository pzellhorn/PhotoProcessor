import cv2
import numpy as np
from insightface.app import FaceAnalysis

from worker_core import results
from worker_core.consumer import BaseConsumer
from worker_core.media import fetch_media_bytes
from worker_core.results import PermanentError


class FaceRecognitionWorker(BaseConsumer):
    def __init__(self, queue: str):
        super().__init__(queue)
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

    def process(self, job: dict) -> None:
        job_id, media_id = job["jobId"], job["mediaId"]
        results.mark_running(job_id)   
        try:
            image_bytes = fetch_media_bytes(media_id)
            faces = self.detect(image_bytes)
            results.submit_fingerprints(job_id, faces)
        except PermanentError as e:
            results.mark_failed(job_id, str(e))    

if __name__ == "__main__":
    FaceRecognitionWorker(queue="jobs.face-recognition").run()
