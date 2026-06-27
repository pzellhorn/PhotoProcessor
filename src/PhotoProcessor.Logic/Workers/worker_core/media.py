import os 
import requests

API_BASE_URL = os.getenv("API_BASE_URL", "http://localhost:5030")


def fetch_media_bytes(media_id: str, timeout: float = 30.0) -> bytes: 
    response = requests.get(
        f"{API_BASE_URL}/api/Photo/GetDownloadUrl",
        params={"mediaId": media_id},
        timeout=timeout,
    )
    response.raise_for_status()
    download_url = response.json()["url"]

    blob = requests.get(download_url, timeout=timeout)
    blob.raise_for_status()
    return blob.content
