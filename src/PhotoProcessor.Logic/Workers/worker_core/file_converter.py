from abc import ABC, abstractmethod
from enum import Enum


class FileFormat(Enum):
    """Each member's value is the format's file signature (leading magic bytes)."""
    JPEG = b"\xff\xd8\xff"

    def matches(self, data: bytes) -> bool:
        return data.startswith(self.value)


class IFileConverter(ABC):
    @abstractmethod
    def convert(self, data: bytes, target: FileFormat) -> bytes:
        """Returns data encoded as target. Raises PermanentError when data can never be converted."""
