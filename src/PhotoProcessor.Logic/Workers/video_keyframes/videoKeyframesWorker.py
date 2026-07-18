import os
import re
import shutil
import subprocess
import tempfile

from worker_core import results, storage
from worker_core.consumer import BaseConsumer
from worker_core.media import fetch_media_to_file
from worker_core.results import PermanentError

FRAME_INTERVAL_SECONDS = float(os.getenv("FRAME_INTERVAL_SECONDS", "1"))
FRAME_PATTERN = re.compile(r"frame_(\d+)\.jpg$")


class VideoKeyframesWorker(BaseConsumer):
    def process(self, job: dict) -> None:
        job_id, media_id = job["jobId"], job["mediaId"]
        results.mark_running(job_id)
        try:
            with tempfile.TemporaryDirectory() as work_dir:
                source_path = os.path.join(work_dir, "source")
                fetch_media_to_file(media_id, source_path)

                frame_dir = os.path.join(work_dir, "frames")
                os.makedirs(frame_dir, exist_ok=True)

                self._extract(source_path, frame_dir)

                frames = self._describe(frame_dir, media_id)
                if not frames:
                    raise PermanentError("no frames could be extracted")

                storage.upload_dir(frame_dir, f"frames/{media_id}")
                results.submit_keyframes(job_id, frames)
        except PermanentError as e:
            results.mark_failed(job_id, str(e))

    def _extract(self, source_path: str, frame_dir: str) -> None:
        cmd = [
            "ffmpeg", "-y", "-i", source_path,
            "-vf", f"fps=1/{FRAME_INTERVAL_SECONDS}",
            "-q:v", "3",
            os.path.join(frame_dir, "frame_%06d.jpg"),
        ]
        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0:
            raise PermanentError(f"ffmpeg keyframe extraction failed: {result.stderr.strip()[-500:]}")

    def _describe(self, frame_dir: str, media_id: str) -> list[dict]:
        frames = []
        for name in sorted(os.listdir(frame_dir)):
            match = FRAME_PATTERN.search(name)
            if not match:
                continue

            # frame_000001 is the frame at t=0.
            index = int(match.group(1)) - 1
            frames.append({
                "timestampMs": index * FRAME_INTERVAL_SECONDS * 1000.0,
                "storagePath": f"frames/{media_id}/{name}",
            })
        return frames


if __name__ == "__main__":
    if shutil.which("ffmpeg") is None:
        raise SystemExit("ffmpeg not found on PATH")
    VideoKeyframesWorker(queue="jobs.video-keyframes").run()
