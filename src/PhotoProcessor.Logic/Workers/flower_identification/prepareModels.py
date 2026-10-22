"""Build step: bake BioCLIP 2 into the image and cut BioCLIP 2's species list down to flowering plants.

The Tree of Life species text embeddings are 867k species x 768 dims of float32 (2.7 GB); flowering plants are ~316k
of them. They are stored as float16 rows so the worker scores an image against every candidate in one matrix product.
"""
import json
import os
import tempfile

import numpy as np
import open_clip
from huggingface_hub import hf_hub_download

from flower_identification import (
    BIOCLIP_MODEL, FLOWERING_PLANT_CLASSES, SPECIES_DIR, SPECIES_EMBEDDINGS_FILE, SPECIES_NAMES_FILE, TREE_OF_LIFE_REPO,
)


def download_models() -> None:
    open_clip.create_model_from_pretrained(BIOCLIP_MODEL)


def write_flowering_species() -> None:
    # Downloaded to a throwaway cache: only the subset is kept, and the full 2.7 GB must not end up in the image.
    with tempfile.TemporaryDirectory() as cache_dir:
        names_path = hf_hub_download(TREE_OF_LIFE_REPO, "embeddings/txt_emb_species.json", repo_type="dataset", cache_dir=cache_dir)
        embeddings_path = hf_hub_download(TREE_OF_LIFE_REPO, "embeddings/txt_emb_species.npy", repo_type="dataset", cache_dir=cache_dir)

        with open(names_path, encoding="utf-8") as f:
            names = json.load(f)
        keep = [i for i, (taxonomy, _) in enumerate(names) if taxonomy[2] in FLOWERING_PLANT_CLASSES]
        embeddings = np.ascontiguousarray(np.load(embeddings_path, mmap_mode="r")[:, keep].T, dtype=np.float16)

    species = []
    for i in keep:
        taxonomy, common_name = names[i]
        species.append({
            "scientificName": f"{taxonomy[5]} {taxonomy[6]}",
            "commonName": common_name or None,
            "family": taxonomy[4],
        })

    os.makedirs(SPECIES_DIR, exist_ok=True)
    np.save(os.path.join(SPECIES_DIR, SPECIES_EMBEDDINGS_FILE), embeddings)
    with open(os.path.join(SPECIES_DIR, SPECIES_NAMES_FILE), "w", encoding="utf-8") as f:
        json.dump(species, f, ensure_ascii=False)

    print(f"kept {len(keep)} of {len(names)} species")


if __name__ == "__main__":
    download_models()
    write_flowering_species()
