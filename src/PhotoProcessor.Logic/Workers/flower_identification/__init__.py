import os

BIOCLIP_MODEL = "hf-hub:imageomics/bioclip-2"
TREE_OF_LIFE_REPO = "imageomics/TreeOfLife-200M"

# Flowering plants (angiosperms) are these two classes in the TreeOfLife taxonomy.
FLOWERING_PLANT_CLASSES = ("Magnoliopsida", "Liliopsida")

SPECIES_DIR = os.getenv("FLOWER_SPECIES_DIR", "/app/species")
SPECIES_EMBEDDINGS_FILE = "flowering_species.npy"
SPECIES_NAMES_FILE = "flowering_species.json"
