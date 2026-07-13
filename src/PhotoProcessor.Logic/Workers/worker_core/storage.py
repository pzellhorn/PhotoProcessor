import os
import mimetypes
import boto3
from botocore.client import Config

_S3_ENDPOINT = os.getenv("S3_ENDPOINT", "http://host.docker.internal:9000")
_S3_ACCESS_KEY = os.getenv("S3_ACCESS_KEY", "minioadmin")
_S3_SECRET_KEY = os.getenv("S3_SECRET_KEY", "minioadmin")
_S3_BUCKET = os.getenv("S3_BUCKET", "media")

_CONTENT_TYPES = {
    ".m3u8": "application/vnd.apple.mpegurl",
    ".ts": "video/mp2t",
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".mp4": "video/mp4",
}


def _client():
    return boto3.client(
        "s3",
        endpoint_url=_S3_ENDPOINT,
        aws_access_key_id=_S3_ACCESS_KEY,
        aws_secret_access_key=_S3_SECRET_KEY,
        config=Config(s3={"addressing_style": "path"}),
    )


def _content_type(path: str) -> str:
    ext = os.path.splitext(path)[1].lower()
    return _CONTENT_TYPES.get(ext) or mimetypes.guess_type(path)[0] or "application/octet-stream"


def upload_file(local_path: str, key: str) -> None:
    _client().upload_file(
        local_path, _S3_BUCKET, key,
        ExtraArgs={"ContentType": _content_type(local_path)},
    )


def upload_dir(local_dir: str, key_prefix: str) -> None:
    """Uploads a directory tree, mapping local relative paths onto keys under key_prefix."""
    client = _client()
    for root, _dirs, files in os.walk(local_dir):
        for name in files:
            local_path = os.path.join(root, name)
            rel = os.path.relpath(local_path, local_dir).replace(os.sep, "/")
            key = f"{key_prefix.rstrip('/')}/{rel}"
            client.upload_file(
                local_path, _S3_BUCKET, key,
                ExtraArgs={"ContentType": _content_type(local_path)},
            )
