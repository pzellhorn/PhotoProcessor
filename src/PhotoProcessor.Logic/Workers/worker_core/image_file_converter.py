import io

import rawpy
from PIL import Image

from worker_core.file_converter import FileFormat, IFileConverter
from worker_core.results import PermanentError

JPEG_QUALITY = 95
# Raws also carry tiny IFD thumbnails (160x120 on a D750); anything this small gets developed instead.
MIN_PREVIEW_LONG_EDGE = 1024


class ImageFileConverter(IFileConverter):
    """Converts still images to JPEG. Camera raws (NEF, CR2, ARW, DNG, ...) use their embedded full-size
    preview when there is one, which keeps the camera's own rendering and EXIF orientation."""

    def convert(self, data: bytes, target: FileFormat) -> bytes:
        if target is not FileFormat.JPEG:
            raise ValueError(f"{type(self).__name__} cannot produce {target.name}")

        if target.matches(data):
            return data

        jpeg = self._from_raw(data)
        if jpeg is not None:
            return jpeg

        return self._from_image(data)

    def _from_raw(self, data: bytes) -> bytes | None:
        try:
            with rawpy.imread(io.BytesIO(data)) as raw:
                return self._preview(raw) or self._develop(raw)
        except rawpy.LibRawFileUnsupportedError:
            return None
        except rawpy.LibRawError as e:
            raise PermanentError(f"could not read raw image: {e}")

    def _preview(self, raw: rawpy.RawPy) -> bytes | None:
        try:
            thumb = raw.extract_thumb()
        except (rawpy.LibRawNoThumbnailError, rawpy.LibRawUnsupportedThumbnailError):
            return None

        if thumb.format == rawpy.ThumbFormat.BITMAP:
            image = Image.fromarray(thumb.data)
            return _encode_jpeg(image) if _large_enough(image) else None

        preview = Image.open(io.BytesIO(thumb.data))
        return thumb.data if _large_enough(preview) else None

    def _develop(self, raw: rawpy.RawPy) -> bytes:
        return _encode_jpeg(Image.fromarray(raw.postprocess(use_camera_wb=True)))

    def _from_image(self, data: bytes) -> bytes:
        try:
            image = Image.open(io.BytesIO(data))
            image.load()
        except Exception as e:
            raise PermanentError(f"could not decode image: {e}")

        return _encode_jpeg(image)


def _large_enough(image: Image.Image) -> bool:
    return max(image.size) >= MIN_PREVIEW_LONG_EDGE


def _encode_jpeg(image: Image.Image) -> bytes:
    out = io.BytesIO()
    image.convert("RGB").save(out, format="JPEG", quality=JPEG_QUALITY, exif=image.getexif())
    return out.getvalue()
