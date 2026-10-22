import io
import json
import os

import numpy as np
import open_clip
import torch
import torch.nn.functional as F
from PIL import Image, ImageOps
from torchvision import transforms

from flower_identification import BIOCLIP_MODEL, SPECIES_DIR, SPECIES_EMBEDDINGS_FILE, SPECIES_NAMES_FILE
from worker_core.results import PermanentError

# Decode JPEGs at roughly this size: BioCLIP only sees 224x224, and full 24 MP decodes are slow.
DECODE_SIZE = 448

# pybioclip's preprocessing for Tree of Life models: squash to 224x224 (no crop), CLIP normalisation.
PREPROCESS = transforms.Compose([
    transforms.ToTensor(),
    transforms.Resize((224, 224), antialias=True),
    transforms.Normalize(mean=(0.48145466, 0.4578275, 0.40821073), std=(0.26862954, 0.26130258, 0.27577711)),
])


def decode_upright(jpeg: bytes) -> Image.Image:
    try:
        image = Image.open(io.BytesIO(jpeg))
        image.draft("RGB", (DECODE_SIZE, DECODE_SIZE))
        return ImageOps.exif_transpose(image).convert("RGB")
    except Exception as e:
        raise PermanentError(f"could not decode image: {e}")


class SpeciesClassifier:
    """BioCLIP 2 open-set classifier over every flowering plant species in the Tree of Life."""

    def __init__(self):
        self.model, _ = open_clip.create_model_from_pretrained(BIOCLIP_MODEL)
        self.model.eval()
        self.logit_scale = self.model.logit_scale.exp().item()

        # Kept in float16: half the memory of float32 (0.5 GB), and scores match to ~1e-4.
        self.species_embeddings = torch.from_numpy(np.load(os.path.join(SPECIES_DIR, SPECIES_EMBEDDINGS_FILE)))
        with open(os.path.join(SPECIES_DIR, SPECIES_NAMES_FILE), encoding="utf-8") as f:
            self.species = json.load(f)

    @torch.no_grad()
    def embed(self, image: Image.Image) -> torch.Tensor:
        features = self.model.encode_image(PREPROCESS(image).unsqueeze(0))
        return F.normalize(features, dim=-1)[0]

    @torch.no_grad()
    def predict(self, embedding: torch.Tensor, k: int) -> list[dict]:
        similarities = (self.species_embeddings @ embedding.half()).float()
        probabilities = F.softmax(self.logit_scale * similarities, dim=0)
        top = probabilities.topk(k)

        predictions = []
        for index, score in zip(top.indices.tolist(), top.values.tolist()):
            predictions.append({**self.species[index], "score": score})
        return predictions
