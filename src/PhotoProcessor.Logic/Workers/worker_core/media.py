import requests

from worker_core.config import API_BASE_URL
from worker_core.results import PermanentError


def fetch_media_bytes(media_id: str, timeout: float = 30.0) -> bytes:
    response = requests.get(
        f"{API_BASE_URL}/api/Photo/GetDownloadUrl",
        params={"mediaId": media_id},
        timeout=timeout,
    )
    if 400 <= response.status_code < 500:
        raise PermanentError(f"GetDownloadUrl({media_id}) -> {response.status_code}: {response.text}")
    response.raise_for_status()
    download_url = response.json()["url"]

    blob = requests.get(download_url, timeout=timeout)
    if 400 <= blob.status_code < 500:
        raise PermanentError(f"media object for {media_id} -> {blob.status_code}")
    blob.raise_for_status()
    return blob.content
