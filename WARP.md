# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

Finimus Grabber is a Windows desktop application for audio analysis, note detection, and MIDI export. It processes MP3, WAV, and FLAC files to detect musical notes, extract melodies and chords, and exports results as standard MIDI files.

**Status:** Early development phase - core architecture planned but implementation not yet started.

**Creator:** Finimus - musician (violin, piano, guitar) and technologist combining musical creativity with innovation. GitHub: https://github.com/Finimus

### Operating Modes

The application has two primary modes of operation:

1. **Automatic Mode** - The app scans music and automatically detects notes, chords, and melody, generating MIDI data instantly. This is the default mode for quick transcription.

2. **Manual Mode** - Users see audio waveforms visually, can select notes manually, and fine-tune the output. This mode provides granular control for precision editing and learning purposes.

Both modes should produce high-quality MIDI output that accurately represents the musical content of the input audio.

## Technology Stack

The project will use one of the following stacks (not yet decided):
- **C++ Stack:** Visual Studio 2022, CMake, FFTW3/Kiss FFT, libsndfile, libmp3lame
- **.NET Stack:** C#, .NET, WPF/WinForms, NAudio

Dependencies managed via:
- **vcpkg** for C++ libraries
- **NuGet** for .NET packages

## Common Commands

### Building (C++ with CMake)
```powershell
# Debug build
cmake -B build -S . -DCMAKE_BUILD_TYPE=Debug
cmake --build build --config Debug

# Release build
cmake -B build -S . -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
```

### Building (.NET with dotnet CLI)
```powershell
# Debug build
dotnet build --configuration Debug

# Release build
dotnet build --configuration Release
```

### Testing

#### C++ Tests (Google Test)
```powershell
ctest --test-dir build
```

#### .NET Tests (xUnit/NUnit)
```powershell
dotnet test
```

### Installing Dependencies

#### C++ Dependencies (vcpkg)
```powershell
vcpkg install fftw3:x64-windows
vcpkg install libsndfile:x64-windows
```

## Architecture

### High-Level System Design

The application follows a modular, event-driven architecture with clear separation of concerns:

```
UI Layer → Application Controller → Core Modules (Audio Loader, Analysis Engine, MIDI Exporter)
```

Audio processing flows through distinct phases:
1. **Load** → Audio file decoded to PCM buffer
2. **Analyze** → FFT processing, pitch detection, onset detection
3. **Visualize** → Waveform, spectrogram, piano roll rendering
4. **Export** → MIDI file generation

### Core Modules

#### 1. Audio Loader Module
- Decodes MP3, WAV, FLAC to raw PCM data
- Manages in-memory audio buffers
- Dependencies: libsndfile (WAV/FLAC), libmp3lame/minimp3 (MP3)

#### 2. Analysis Engine
Multi-stage audio analysis pipeline:
- **FFT Processor:** Configurable window size (1024/2048/4096), converts time → frequency domain
- **Onset Detector:** Identifies note attack points using spectral flux
- **Pitch Detector:** Extracts fundamental frequency using autocorrelation or YIN algorithm
- **Harmonic Analyzer:** Separates harmonic/percussive content
- **Chord Recognizer:** Matches simultaneous notes to chord templates
- Dependencies: FFTW3 or Kiss FFT

#### 3. Visualization Module
- Waveform renderer with zoom/pan
- Real-time spectrogram with color-coded intensity
- MIDI-style piano roll with interactive editing
- Dependencies: GDI+, Direct2D, WPF, or OpenGL

#### 4. MIDI Exporter
- Converts detected notes to MIDI messages (Note On/Off)
- Supports Type 0 (single track) and Type 1 (multi-track)
- Configurable PPQN (pulses per quarter note)
- Dependencies: Custom serialization or midifile library

#### 5. UI Layer
- Main window with file menu, toolbar, docked panels
- Playback controls and analysis settings
- Manual note editing with undo/redo
- Technology: WPF (modern), WinForms (simpler), or Qt (cross-platform)

### Key Data Structures

```cpp
struct Note {
    double time_seconds;
    int midi_note;
    double frequency_hz;
    double duration_seconds;
    int velocity;
    double confidence;
};
```

### Threading Model

- **Audio I/O Thread:** File loading and decoding
- **Analysis Thread:** FFT and pitch detection processing
- **UI Thread:** Graphics rendering and user input
- **Export Thread:** MIDI file writing

### Performance Optimization Strategies

- SIMD instructions (SSE, AVX) for DSP operations
- Multi-threading for parallel FFT processing
- GPU acceleration with DirectCompute or CUDA
- Memory pooling for buffer management
- Stream large files in chunks rather than loading entirely

## Development Guidelines

### Code Organization

- Keep DSP algorithms in separate modules from UI code
- Use dependency injection for testability
- Prefer composition over inheritance for module design
- Comment complex audio processing algorithms thoroughly
- Ensure clear separation between Automatic Mode (AI-driven detection) and Manual Mode (user-driven editing) logic

### Analysis Parameters

Key configuration values (will be tunable by users):
- FFT window size: 1024, 2048, or 4096 samples
- Hop size: determines time resolution
- Onset detection threshold: 0.1-0.9 range
- Minimum note duration: typically 50-100ms
- Confidence threshold: filter low-confidence detections

### Testing Strategy

- **Unit Tests:** Test each module independently with mocked dependencies
- **Integration Tests:** Use sample audio files in `examples/` directory
- **Verification:** Compare detected notes against ground truth data
- **Performance Tests:** Validate processing speed with large files (>100MB)
- **Mode-Specific Tests:** Verify both Automatic Mode (accuracy of auto-detection) and Manual Mode (UI responsiveness and precision editing)

Test audio files for integration testing:
- `examples/test_mp3.mp3`
- `examples/test_wav.wav`
- `examples/test_flac.flac`

### Error Handling

- Validate audio file formats before attempting to load
- Handle corrupted or unsupported files gracefully
- Provide clear error messages in UI (technical details in logs)
- Never crash on bad input - always fail safely

## Project-Specific Constraints

### Windows-Only Target
- All code must compile/run on Windows 10+ (64-bit)
- Use Windows-specific APIs where appropriate (e.g., file dialogs)
- Consider PowerShell as the default shell for commands

### Offline-First Design
- No network communication required or implemented
- No telemetry or analytics collection
- All processing happens locally

### MIDI Output Quality
- MIDI output should accurately reflect the musical content of the audio
- Both Automatic and Manual modes must produce professional-quality results
- Timing precision is critical - notes should align with the original audio
- Velocity and duration should match perceived loudness and length

### Performance Requirements
- Target: Process 1 minute of audio in <5 seconds
- Low CPU usage during idle
- Responsive UI (30-60 FPS for visualizations)
- Support files up to 30 minutes duration

### Packaging Goals
- Standalone executable (~25MB target size)
- Static linking to eliminate DLL dependencies
- Code signing for Windows SmartScreen compatibility

## Documentation

Key documentation files:
- `README.md` - Project overview and getting started
- `docs/DEVELOPMENT.md` - Build instructions and development setup
- `docs/ARCHITECTURE.md` - Detailed technical design
- `docs/APP_DESCRIPTION.md` - Feature specifications and use cases

When making significant architectural changes, update the relevant documentation files.

## Current State & Getting Started

**Note:** This is an early-stage project with planned architecture but no implementation yet. The `src/` and `tests/` directories are currently empty.

When beginning implementation:
1. Choose between C++ or C#/.NET stack based on requirements
2. Set up build system (CMake for C++, .csproj for .NET)
3. Install required audio processing libraries via vcpkg or NuGet
4. Start with Audio Loader module to establish data flow foundation
5. Implement Analysis Engine with basic FFT → pitch detection pipeline (Automatic Mode foundation)
6. Build waveform visualization for Manual Mode
7. Create mode switcher in UI to toggle between Automatic and Manual modes
8. Ensure MIDI export produces high-quality output in both modes

## About Section Content

When implementing the About dialog/section, include:
- Creator: Finimus (musician and technologist)
- Musical background: violin, piano, guitar player
- Technical interests: IT and AI
- Project vision: Making music analysis easier and more intelligent through automation + manual control
- Target audience: Beginners to professional producers
- GitHub link: https://github.com/Finimus
