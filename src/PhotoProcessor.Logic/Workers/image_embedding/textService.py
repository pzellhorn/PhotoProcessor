from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer

from image_embedding import MODEL_NAME

app = FastAPI()
model = SentenceTransformer(MODEL_NAME, device="cpu")


class EncodeRequest(BaseModel):
    text: str


class EncodeResponse(BaseModel):
    embedding: list[float]


@app.get("/health")
def health() -> dict:
    return {"status": "ok", "model": MODEL_NAME}


@app.post("/encode", response_model=EncodeResponse)
def encode(request: EncodeRequest) -> EncodeResponse:
    embedding = model.encode(request.text, normalize_embeddings=True)
    return EncodeResponse(embedding=embedding.tolist())
