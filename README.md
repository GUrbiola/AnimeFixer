# AnimeFixer

A Windows desktop utility for fixing media files that don't play correctly on Roku devices served through a Jellyfin media server. Built with .NET Framework 4.8 and WinForms.

## What It Does

Roku clients have strict codec and container requirements that most anime and foreign-media files don't meet out of the box. AnimeFixer addresses three distinct failure modes through separate tools launched from a central hub window.

### MP4 Fixer

Scans a folder of MP4 files and remuxes those with invalid GPAC data streams.

Some MP4 files contain `data`-type streams injected by GPAC/MP4Box (identifiable by handler names like `GPAC`, `OD Handler`, or `Scene Description`, or codec IDs of 0 / codec names of `none` or `mp4s`). Jellyfin cannot transcode these cleanly, causing playback failures on Roku. The fix is a lossless remux (`ffmpeg -c copy`) that maps only the valid streams into a new container, discarding the invalid ones. The original file is replaced atomically: the temp output is only swapped in if the remux succeeds and produces a non-empty file, otherwise the original is left untouched.

### MKV Roku Fixer

Scans MKV files, detects HEVC 10-bit video, and prepares them for Roku:

- **Re-encode to H.264 MP4** — Roku does not support HEVC 10-bit (`hevc` + `pix_fmt yuv420p10le`). Files are transcoded to H.264 High Profile Level 4.1 in an MP4 container with `-movflags +faststart`. The faststart flag moves the `moov` atom to the front of the file so Roku can seek using pure HTTP byte-range requests without round-tripping to Jellyfin for every seek operation. Output filename is `<original>.roku.mp4` alongside the source.
- **Extract subtitles to SRT** — MKV subtitle tracks (ASS, SSA, etc.) are extracted to individual `.srt` files. The extracted SRT is cleaned: ASS override blocks (`{...}`) and non-standard HTML tags are stripped since Roku only honours `<b>`, `<i>`, and `<u>`.
- **NVIDIA NVENC support** — the re-encode can use `h264_nvenc` for hardware-accelerated encoding (5–15× faster than libx264, no CPU load). Falls back to `libx264` with selectable presets (veryfast / fast / medium / slow).
- **Subtitle selection dialog** — double-clicking a file opens a dialog to pick which subtitle tracks to extract, with language and codec information shown per track.

### Compress

Batch compresses large video files using HandBrakeCLI. Scans a folder for files above a configurable size threshold and queues them for compression with a fixed H.264 MKV 1080p30 preset (CRF 20). Shows before/after file size and space saved on completion.

## Architecture

The application is structured around three independently-opening forms launched from a single hub window, each owning its own service instances. All long-running operations run on background threads via `Task.Run`, with all UI updates marshalled back through `InvokeRequired` / `Invoke`. Every operation accepts a `CancellationToken` so the user can abort mid-batch cleanly.

```
Form1 (hub)
├── MP4FixerForm       → FFmpegService, ScannerService
├── MkvRokuForm        → FFmpegService, MkvScannerService
└── CompressForm       → HandBrakeService, CompressScannerService
```

Settings (FFmpeg folder, HandBrakeCLI path, last used folder) persist to `%APPDATA%\MP4Fixer\settings.ini` as plain key=value pairs. FFmpeg is auto-detected from common Jellyfin install locations on startup.

## Interesting Implementation Notes

**Hand-rolled JSON parser for ffprobe output.** Rather than pulling in a JSON library, `FFmpegService` parses ffprobe's `-print_format json` output using brace-depth tracking (`SplitStreamBlocks`) and targeted regex field extraction. This handles nested objects and escaped strings correctly without any external dependencies.

**Progress via `-progress pipe:1`.** Instead of scraping ffmpeg's stderr for progress (fragile and locale-dependent), the re-encode uses `-progress pipe:1 -nostats` which writes structured `key=value` lines to stdout. The parser accumulates `frame`, `fps`, and `speed` values and fires the progress callback on each `progress=continue` line, giving accurate ETA display.

**Frame count fallback.** Total frame count is first read from the `nb_frames` stream attribute. When that's absent (common in HEVC MKV files that don't store it), it falls back to `duration × r_frame_rate` from a second ffprobe call. If neither works the progress bar degrades gracefully to showing raw frame numbers.

**GPAC stream identification.** The `StreamInfo.IsInvalid` predicate identifies GPAC-injected data streams by the combination of `codec_type == "data"` and any of: `codec_id == 0`, `codec_name` being `none`/`mp4s`/empty, or the handler name containing `GPAC`, `OD Handler`, or `Scene Description`. This covers the full range of GPAC muxer variants encountered in the wild.

**SRT cleaning after ASS conversion.** ffmpeg's built-in ASS→SRT converter leaves override codes (`{\an8}`, `{\pos(x,y)}`, etc.) and arbitrary HTML tags in the output. `CleanSrtFile` removes them with two regex passes: one for `{...}` blocks and one that strips any HTML tag except `<b>`, `<i>`, and `<u>` (the only tags Roku's SRT renderer honours).

**Two-phase MKV probe.** `ProbeMkv` runs two separate ffprobe invocations — one targeted at the video stream for codec/pixel-format detection, one targeted at subtitle streams for track enumeration — rather than one full probe. This keeps the video HEVC check fast and keeps subtitle metadata extraction clean.

## Dependencies

- **ffmpeg / ffprobe** — required for MP4 Fixer and MKV Roku Fixer. Auto-detected from Jellyfin install paths or configured manually.
- **HandBrakeCLI** — required for Compress. Must be pointed to manually or available on `PATH`.
- **.NET Framework 4.8** — Windows only.
