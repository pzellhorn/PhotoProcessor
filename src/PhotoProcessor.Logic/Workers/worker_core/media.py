import requests

from worker_core.config import API_BASE_URL
from worker_core.results import PermanentError


def _download_url(media_id: str, timeout: float) -> str:
    response = requests.get(
        f"{API_BASE_URL}/api/Photo/GetDownloadUrl",
        params={"mediaId": media_id},
        timeout=timeout,
    )
    if 400 <= response.status_code < 500:
        raise PermanentError(f"GetDownloadUrl({media_id}) -> {response.status_code}: {response.text}")
    response.raise_for_status()
    return response.json()["url"]


def fetch_media_bytes(media_id: str, timeout: float = 30.0) -> bytes:
    blob = requests.get(_download_url(media_id, timeout), timeout=timeout)
    if 400 <= blob.status_code < 500:
        raise PermanentError(f"media object for {media_id} -> {blob.status_code}")
    blob.raise_for_status()
    return blob.content


def fetch_media_to_file(media_id: str, dest_path: str, timeout: float = 120.0) -> None:
    with requests.get(_download_url(media_id, timeout), stream=True, timeout=timeout) as blob:
        if 400 <= blob.status_code < 500:
            raise PermanentError(f"media object for {media_id} -> {blob.status_code}")
        blob.raise_for_status()
        with open(dest_path, "wb") as out:
            for chunk in blob.iter_content(chunk_size=1024 * 1024):
                out.write(chunk)
