# Distribution Guide

## Executable Location

The standalone executable is located at:
```
publish/FinimusGrabber.exe
```

**Size**: ~77 MB (self-contained with .NET runtime and all dependencies)

## What to Distribute

For end users, distribute the **entire `publish` folder** contents:

```
publish/
├── FinimusGrabber.exe          (Main executable - 77MB)
├── FinimusGrabber.pdb          (Debug symbols - optional)
├── onnxruntime.lib             (ONNX Runtime library)
└── onnxruntime_providers_shared.lib
```

**Minimum distribution**: Just `FinimusGrabber.exe` and the two `.lib` files.

## System Requirements

- **OS**: Windows 10/11 (64-bit)
- **CPU**: Dual-core processor, 2.0 GHz or higher
- **RAM**: 4GB minimum, 8GB recommended for AI processing
- **Storage**: 100MB free space

## Installation

**No installation required!** Simply:
1. Copy the files to any folder
2. Double-click `FinimusGrabber.exe` to run

The application is fully portable and can run from:
- Desktop
- Program Files
- USB drive
- Any folder with write permissions

## Usage

### Quick Start

1. **Launch** `FinimusGrabber.exe`
2. **Click** "Open File" and select an audio file (MP3, WAV, or FLAC)
3. **Use AI tools** (optional):
   - 🎤 Isolate Vocals
   - 🥁 Isolate Drums
   - 🔇 Remove Noise
4. **Click** "Analyze" to detect notes
5. **Click** "Export MIDI" to save results

### AI Processing Workflow

For best results:
1. Load audio file
2. Apply "Remove Noise" if needed
3. Apply "Isolate Vocals" or "Isolate Drums" depending on content
4. Click "Analyze"
5. Export to MIDI

## Features

### Core Features
- Audio to MIDI transcription
- Support for MP3, WAV, FLAC formats
- Real-time waveform visualization
- Piano roll note display
- MIDI export (Type 0)

### AI Features (NEW in v2.0)
- **Vocal Isolation**: Extract vocals (80Hz-8kHz)
- **Drum Isolation**: Separate percussion with transient detection
- **Noise Suppression**: Remove background noise via spectral gating
- **Reset**: Restore original audio instantly

### Modes
- **Automatic Mode**: Instant AI-powered detection
- **Manual Mode**: Visual editing and fine-tuning

## Performance

### Processing Times (Approximate)
- **Load Audio**: Instant
- **Noise Suppression**: ~2-3 seconds per minute of audio
- **Vocal Isolation**: ~3-5 seconds per minute of audio
- **Drum Isolation**: ~4-6 seconds per minute of audio
- **Note Analysis**: ~1-2 seconds per minute of audio
- **MIDI Export**: Instant

### Memory Usage
- Base: ~100 MB
- With audio loaded: ~200-300 MB
- During AI processing: ~300-500 MB

## Troubleshooting

### Application won't start
- Ensure Windows 10/11 64-bit
- Run as Administrator if needed
- Check antivirus isn't blocking the .exe

### Audio file won't load
- Supported formats: MP3, WAV, FLAC
- Check file isn't corrupted
- Try converting to WAV format

### AI processing is slow
- Normal for large files (>5 minutes)
- Close other applications
- Consider upgrading CPU

### No notes detected
- Try "Remove Noise" first
- Ensure audio has clear melodic content
- Adjust volume if too quiet
- Use "Isolate Vocals" for songs

## Creating a Redistributable Package

### Option 1: ZIP Archive
```powershell
Compress-Archive -Path publish\* -DestinationPath FinimusGrabber-v2.0-win64.zip
```

### Option 2: Installer (Advanced)
Consider using:
- Inno Setup
- WiX Toolset
- NSIS

### Option 3: Microsoft Store
Package as MSIX for Microsoft Store distribution.

## Version History

### Version 2.0 - AI Edition (Current)
- ✅ AI-powered vocal isolation
- ✅ AI-powered drum isolation
- ✅ Spectral noise suppression
- ✅ ONNX Runtime integration
- ✅ Improved UI with AI tools
- ✅ Reset to original audio

### Version 1.0
- ✅ Basic note detection
- ✅ MP3, WAV, FLAC support
- ✅ MIDI export
- ✅ Dual mode (Auto/Manual)
- ✅ Waveform visualization

## Licensing

**Proprietary Software** - All rights reserved.

## Support

For issues or questions, refer to:
- [BUILD_AND_RUN.md](BUILD_AND_RUN.md) - User guide
- [docs/AI_FEATURES.md](docs/AI_FEATURES.md) - AI features documentation

## Credits

- Created by **Finimus**
- Inspired by [OpenVINO AI Plugins for Audacity](https://github.com/intel/openvino-plugins-ai-audacity)
- Built with NAudio, ONNX Runtime, and .NET 10.0

---

**Finimus Grabber v2.0 - AI Edition**  
Transform Audio Into Musical Data with AI
