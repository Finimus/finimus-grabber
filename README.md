# Finimus Grabber

> Professional Windows desktop application for audio analysis, note detection, and MIDI export

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![License](https://img.shields.io/badge/license-Proprietary-red)
![Status](https://img.shields.io/badge/status-In%20Development-yellow)

## About

Finimus Grabber is a standalone Windows desktop application that transforms audio files into musical data. It analyzes MP3, WAV, and FLAC files to detect notes, extract melodies and chords, and export the results as standard MIDI files.

**Key Features:**
- 🎵 Support for MP3, WAV, and FLAC audio formats
- 🎹 Precision note detection with chromatic recognition
- 🎼 Melody and chord extraction from complex audio
- 📊 Real-time waveform and harmonic spectrum visualization
- ✏️ Manual editing tools for fine-tuning detected notes
- 💾 Standard MIDI file export (Type 0 & Type 1)
- ⚡ Optimized for low CPU usage and fast processing
- 🔒 100% offline functionality with no telemetry

## Use Cases

- **Music Producers** – Sample melodies and extract chord progressions from reference tracks
- **Beatmakers** – Identify notes in vinyl samples and loops for remixing
- **Music Students** – Learn songs by ear with visual feedback and transcription
- **Composers** – Transcribe recordings into MIDI for notation software
- **Sound Designers** – Analyze harmonic content of sound effects and patches

## Technical Overview

### Architecture
- Standalone executable (~25MB)
- No installation or dependencies required
- Multi-threaded audio processing
- GPU-accelerated FFT when available
- Real-time and batch processing modes

### System Requirements
- **OS:** Windows 10 or Windows 11 (64-bit)
- **CPU:** Dual-core processor (2.0 GHz or higher)
- **RAM:** 4GB minimum, 8GB recommended
- **Storage:** 100MB available space

### Supported Formats
- **Input:** MP3, WAV (16/24/32-bit), FLAC
- **Output:** MIDI Type 0 and Type 1
- **Sample Rates:** 44.1kHz to 192kHz
- **Note Range:** C0 to C10 (MIDI 0-127)

## Project Structure

```
finimus-grabber/
├── src/              # Source code
├── assets/           # Icons, images, and resources
├── docs/             # Documentation and specifications
├── tests/            # Unit and integration tests
├── examples/         # Sample audio files and usage examples
└── README.md         # This file
```

## Documentation

- [**App Description**](docs/APP_DESCRIPTION.md) – Detailed feature overview and use cases
- [**Development Setup**](docs/DEVELOPMENT.md) – Build instructions and development environment
- [**Architecture**](docs/ARCHITECTURE.md) – Technical design and implementation details

## Development Status

This project is currently in active development. The application is being designed with the following technologies:

- **Language:** C++ or C# (.NET)
- **Audio Processing:** FFT libraries, DSP algorithms
- **UI Framework:** WPF, WinForms, or native Win32
- **Build System:** Visual Studio 2022

## License

Proprietary software. All rights reserved.

## Contact

For questions, feedback, or support inquiries, please contact the development team.

---

**Finimus Grabber** – Transform Audio Into Musical Data
