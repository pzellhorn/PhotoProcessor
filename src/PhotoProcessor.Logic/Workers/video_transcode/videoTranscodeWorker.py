import json
import os
import shutil
import subprocess
import tempfile

from worker_core import results, storage
from worker_core.consumer import BaseConsumer
from worker_core.media import fetch_media_to_file
from worker_core.results import PermanentError

LADDER = [
    {"height": 480, "v_bitrate": "1400k", "a_bitrate": "128k", "bandwidth": 1_600_000},
    {"height": 720, "v_bitrate": "2800k", "a_bitrate": "128k", "bandwidth": 3_000_000},
    {"height": 1080, "v_bitrate": "5000k", "a_bitrate": "192k", "bandwidth": 5_400_000},
]
HLS_SEGMENT_SECONDS = 6


class VideoTranscodeWorker(BaseConsumer):
    def process(self, job: dict) -> None:
        job_id, media_id = job["jobId"], job["mediaId"]
        results.mark_running(job_id)
        try:
            with tempfile.TemporaryDirectory() as work_dir:
                source_path = os.path.join(work_dir, "source")
                fetch_media_to_file(media_id, source_path)

                src_height = self._probe_height(source_path)
                duration_ms = self._probe_duration_ms(source_path)

                out_dir = os.path.join(work_dir, "derived")
                os.makedirs(out_dir, exist_ok=True)

                rungs = [r for r in LADDER if r["height"] <= src_height] or [LADDER[0]]
                renditions = []
                for rung in rungs:
                    self._transcode_rung(source_path, out_dir, rung)
                    renditions.append({
                        "format": f"hls-{rung['height']}p",
                        "entryPath": f"videos/derived/{media_id}/{rung['height']}p/index.m3u8",
                        "height": rung["height"],
                        "bitrate": rung["bandwidth"],
                    })

                self._write_master(out_dir, rungs)
                self._extract_poster(source_path, os.path.join(out_dir, "poster.jpg")) # the thumbnail frame

                storage.upload_dir(out_dir, f"videos/derived/{media_id}")

                renditions.insert(0, {
                    "format": "hls",
                    "entryPath": f"videos/derived/{media_id}/master.m3u8",
                })

                results.submit_video(
                    job_id,
                    duration_ms=duration_ms,
                    poster_path=f"videos/derived/{media_id}/poster.jpg",
                    renditions=renditions,
                )
        except PermanentError as e:
            results.mark_failed(job_id, str(e))

    def _probe_height(self, path: str) -> int:
        out = self._run_probe([
            "-select_streams", "v:0", "-show_entries", "stream=height",
            "-of", "json", path,
        ])
        streams = json.loads(out).get("streams", [])
        if not streams:
            raise PermanentError("no video stream in input")
        return int(streams[0]["height"])

    def _probe_duration_ms(self, path: str) -> float:
        out = self._run_probe([
            "-show_entries", "format=duration", "-of", "json", path,
        ])
        duration = json.loads(out).get("format", {}).get("duration")
        return float(duration) * 1000.0 if duration else 0.0

    def _run_probe(self, args: list) -> str:
        result = subprocess.run(["ffprobe", "-v", "error", *args], capture_output=True, text=True)
        if result.returncode != 0:
            raise PermanentError(f"ffprobe failed: {result.stderr.strip()}")
        return result.stdout

    def _transcode_rung(self, source_path: str, out_dir: str, rung: dict) -> None:
        rung_dir = os.path.join(out_dir, f"{rung['height']}p")
        os.makedirs(rung_dir, exist_ok=True)

        cmd = [
            "ffmpeg", "-y", "-i", source_path,
            "-vf", f"scale=-2:{rung['height']}",
            "-c:v", "libx264", "-preset", "veryfast",
            "-b:v", rung["v_bitrate"], "-maxrate", rung["v_bitrate"], "-bufsize", rung["v_bitrate"],
            "-c:a", "aac", "-b:a", rung["a_bitrate"],
            "-hls_time", str(HLS_SEGMENT_SECONDS),
            "-hls_playlist_type", "vod",
            "-hls_segment_filename", os.path.join(rung_dir, "seg_%03d.ts"),
            os.path.join(rung_dir, "index.m3u8"),
        ]
        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0:
            raise PermanentError(f"ffmpeg transcode {rung['height']}p failed: {result.stderr.strip()[-500:]}")

    def _write_master(self, out_dir: str, rungs: list) -> None:
        lines = ["#EXTM3U", "#EXT-X-VERSION:3"]
        for rung in rungs:
            lines.append(f"#EXT-X-STREAM-INF:BANDWIDTH={rung['bandwidth']},RESOLUTION=x{rung['height']}")
            lines.append(f"{rung['height']}p/index.m3u8")
        with open(os.path.join(out_dir, "master.m3u8"), "w") as f:
            f.write("\n".join(lines) + "\n")

    def _extract_poster(self, source_path: str, poster_path: str) -> None:
        cmd = [
            "ffmpeg", "-y", "-ss", "1", "-i", source_path,
            "-frames:v", "1", "-q:v", "3", poster_path,
        ]
        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0 or not os.path.exists(poster_path):
            fallback = [
                "ffmpeg", "-y", "-i", source_path,
                "-frames:v", "1", "-q:v", "3", poster_path,
            ]
            subprocess.run(fallback, capture_output=True, text=True)


if __name__ == "__main__":
    if shutil.which("ffmpeg") is None:
        raise SystemExit("ffmpeg not found on PATH")
    VideoTranscodeWorker(queue="jobs.video-transcode").run()
