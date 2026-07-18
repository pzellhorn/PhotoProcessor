import requests

from worker_core.config import API_BASE_URL


class PermanentError(Exception):
    """Job can never succeed as-is (bad input / 4xx). Do not requeue."""


def _post(path: str, payload: dict, timeout: float = 30.0):
    response = requests.post(f"{API_BASE_URL}{path}", json=payload, timeout=timeout)
     
    if 400 <= response.status_code < 500:
        raise PermanentError(f"{path} -> {response.status_code}: {response.text}")

    
    # Any error 500+ treat as transient / as http error
    response.raise_for_status() 
    return response


def mark_running(job_id: str) -> None:
    _post("/api/Job/MarkRunning", {"jobId": job_id})


def mark_failed(job_id: str, error: str) -> None:
    _post("/api/Job/MarkFailed", {"jobId": job_id, "error": error})


def submit_fingerprints(job_id: str, faces: list) -> None:
    _post("/api/Fingerprint/Submit", {"jobId": job_id, "faces": faces})


def submit_image_embedding(job_id: str, embedding: list) -> None:
    _post("/api/ImageEmbedding/Submit", {"jobId": job_id, "embedding": embedding})


def submit_keyframes(job_id: str, frames: list) -> None:
    _post("/api/Video/SubmitKeyframes", {"jobId": job_id, "frames": frames}, timeout=600.0)


def submit_video(job_id: str, duration_ms: float, poster_path: str, renditions: list) -> None:
    _post("/api/Video/Submit", {
        "jobId": job_id,
        "durationMs": duration_ms,
        "posterPath": poster_path,
        "renditions": renditions,
    })
