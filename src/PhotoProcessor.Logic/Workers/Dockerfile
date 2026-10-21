FROM python:3.11-slim

WORKDIR /app
 
RUN apt-get update && apt-get install -y --no-install-recommends \
        build-essential \
        libgomp1 \
        libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

ENV PIP_DEFAULT_TIMEOUT=120 \
    PIP_RETRIES=10

COPY face_recognition/requirements.txt ./requirements.txt
RUN --mount=type=cache,target=/root/.cache/pip pip install -r requirements.txt

RUN --mount=type=cache,target=/root/.cache/pip pip uninstall -y opencv-python opencv-contrib-python || true \
    && pip install --force-reinstall "opencv-python-headless<5"

COPY worker_core ./worker_core
COPY face_recognition ./face_recognition
 
RUN python -c "from insightface.app import FaceAnalysis; a = FaceAnalysis(name='buffalo_l', providers=['CPUExecutionProvider']); a.prepare(ctx_id=-1, det_size=(640, 640))"

CMD ["python", "-m", "face_recognition.faceRecognitionWorker"]
