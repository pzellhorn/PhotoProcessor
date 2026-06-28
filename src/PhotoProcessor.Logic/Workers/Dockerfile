FROM python:3.11-slim

WORKDIR /app
 
RUN apt-get update && apt-get install -y --no-install-recommends \
        build-essential \
        libgomp1 \
        libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY face_recognition/requirements.txt ./requirements.txt
RUN pip install --no-cache-dir -r requirements.txt
 
RUN pip uninstall -y opencv-python opencv-contrib-python || true \
    && pip install --no-cache-dir --force-reinstall opencv-python-headless

COPY worker_core ./worker_core
COPY face_recognition ./face_recognition
 
RUN python -c "from insightface.app import FaceAnalysis; a = FaceAnalysis(name='buffalo_l', providers=['CPUExecutionProvider']); a.prepare(ctx_id=-1, det_size=(640, 640))"

CMD ["python", "-m", "face_recognition.faceRecognitionWorker"]
