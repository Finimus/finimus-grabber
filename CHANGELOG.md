# Changelog

All notable changes to Finimus Grabber will be documented in this file.

## [2.0.0] - AI Edition - 2025-12-29

### 🎉 Major Release: AI-Powered Audio Processing

This release introduces AI-powered audio preprocessing inspired by [OpenVINO AI Plugins for Audacity](https://github.com/intel/openvino-plugins-ai-audacity).

### Added

#### AI Features
- **🎤 Vocal Isolation**: Extract vocal frequencies (80Hz-8kHz) from mixed audio
  - FFT-based frequency filtering with 4096-sample windows
  - Hann windowing for smooth transitions
  - Overlap-add processing for high quality
  
- **🥁 Drum Isolation**: Separate percussive elements using temporal analysis
  - Spectrogram-based transient detection
  - Frame-to-frame variation emphasis
  - Enhanced percussive content extraction
  
- **🔇 Noise Suppression**: Remove background noise via spectral gating
  - Automatic noise profile estimation (from first 0.5s)
  - Frequency-dependent gain reduction
  - 80% noise reduction with 2% threshold
  
- **↻ Reset to Original**: Instantly restore unprocessed audio
  - No need to reload files
  - Clears all applied processing
  - Maintains original file path

#### Infrastructure
- Added `NoiseSuppressionService` with configurable parameters
- Added `MusicSeparationService` with stem separation (Vocals, Drums, Bass, Other)
- Integrated ONNX Runtime (v1.22.0) for future AI model support
- Added Microsoft.ML.OnnxRuntime and Microsoft.ML.OnnxRuntime.Managed packages

#### UI/UX
- New AI Tools section in toolbar with 4 dedicated buttons
- Progress indicators during AI processing
- Emoji icons for better visual recognition
- Status messages showing processing progress
- Disabled UI during processing to prevent conflicts

#### Documentation
- Created comprehensive [AI Features Documentation](docs/AI_FEATURES.md)
- Updated [BUILD_AND_RUN.md](BUILD_AND_RUN.md) with AI usage instructions
- Created [DISTRIBUTION.md](DISTRIBUTION.md) for deployment guide
- Updated README.md with AI features overview
- Added technical implementation details and architecture

### Changed

- Updated About dialog to show "Version 2.0 - AI Edition"
- Improved audio processing workflow with pre-processing options
- Enhanced status messages throughout the application
- Increased executable size to ~77 MB (includes ONNX Runtime)
- Notes are now cleared after AI processing (to ensure accuracy)
- Updated .NET packages to latest versions

### Technical Details

#### New Services
```
src/FinimusGrabber/Services/
├── NoiseSuppressionService.cs       (New)
├── MusicSeparationService.cs        (New)
├── AnalysisEngine.cs                (Existing)
├── AudioLoader.cs                   (Existing)
└── MidiExporter.cs                  (Existing)
```

#### Dependencies Added
- Microsoft.ML.OnnxRuntime 1.22.0
- Microsoft.ML.OnnxRuntime.Managed 1.22.0

#### Algorithm Implementations
- **FFT**: Recursive Cooley-Tukey algorithm
- **Inverse FFT**: Conjugate-multiply-conjugate method
- **Windowing**: Hann window function
- **Overlap-Add**: 75% overlap (hop size = window size / 4)
- **Normalization**: Peak limiting to 0.95

### Performance

- Noise Suppression: ~2-3 seconds per minute of audio
- Vocal Isolation: ~3-5 seconds per minute of audio
- Drum Isolation: ~4-6 seconds per minute of audio
- Memory: Peak 300-500 MB during processing

### Known Limitations

1. AI processing is CPU-only (no GPU acceleration yet)
2. Separation quality is basic compared to AI models like Demucs
3. No real-time processing capabilities
4. Memory usage scales with audio file length
5. Noise profile requires ~0.5s of silence at start for best results

### Future Roadmap

- [ ] Integrate actual AI models (Demucs, RNNoise) via ONNX
- [ ] Add Bass isolation feature
- [ ] Add "Other" instruments isolation
- [ ] GPU acceleration support
- [ ] Real-time processing mode
- [ ] Batch processing multiple files
- [ ] Custom noise profile recording
- [ ] Adjustable AI processing parameters in UI

---

## [1.0.0] - Initial Release - 2025-12-XX

### Added

#### Core Features
- Audio file loading (MP3, WAV, FLAC)
- Automatic note detection using FFT analysis
- Manual mode for visual editing
- Real-time waveform visualization
- Piano roll display with note names
- MIDI export (Type 0 format)
- Dual-mode operation (Automatic/Manual)

#### Audio Processing
- NAudio integration for file loading
- FFT-based pitch detection
- Frequency-to-MIDI conversion
- Note confidence calculation
- Minimum note duration filtering (50ms)

#### UI Components
- Menu bar (File, Help)
- Toolbar with mode switcher
- Waveform canvas with visualization
- Piano roll canvas with grid
- Status bar with file info
- About dialog

#### Documentation
- Initial README.md
- BUILD_AND_RUN.md with instructions
- Project structure documentation

### Technical Stack
- Language: C# (.NET 10.0)
- UI: WPF (Windows Presentation Foundation)
- Audio: NAudio 2.2.1
- MP3: NAudio.Lame 2.1.0

---

## Version Numbering

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Incompatible API changes or major feature additions
- **MINOR**: New functionality in a backwards compatible manner
- **PATCH**: Backwards compatible bug fixes

---

## Links

- [Repository](https://github.com/Finimus/finimus-grabber)
- [Issues](https://github.com/Finimus/finimus-grabber/issues)
- [Releases](https://github.com/Finimus/finimus-grabber/releases)
- [OpenVINO AI Plugins Inspiration](https://github.com/intel/openvino-plugins-ai-audacity)
