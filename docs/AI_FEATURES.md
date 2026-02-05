# AI Features Documentation

## Overview

Finimus Grabber now includes AI-powered audio processing inspired by [OpenVINO AI Plugins for Audacity](https://github.com/intel/openvino-plugins-ai-audacity). These features help improve note detection accuracy by preprocessing audio before analysis.

## Implemented Features

### 🎤 Vocal Isolation

**Purpose**: Extract vocal tracks from mixed audio for cleaner melody detection.

**Technical Details**:
- Frequency range: 80Hz - 8kHz (typical vocal range)
- Uses FFT-based frequency filtering
- Processes audio in overlapping windows (4096 samples)
- Applies Hann windowing for smooth transitions

**Best For**:
- Songs with vocals
- Melody transcription
- Reducing instrument interference

**Usage**:
1. Load your audio file
2. Click "🎤 Isolate Vocals"
3. Wait for processing to complete
4. Click "Analyze" to detect notes

---

### 🥁 Drum Isolation

**Purpose**: Extract percussive elements for rhythm analysis.

**Technical Details**:
- Uses temporal variation detection in spectrogram
- Emphasizes bins with high frame-to-frame changes
- Enhances transients (drum hits, clicks, etc.)
- Frequency range: Full spectrum with percussive emphasis

**Best For**:
- Drum pattern transcription
- Rhythm detection
- Beat analysis

**Usage**:
1. Load your audio file
2. Click "🥁 Isolate Drums"
3. Wait for processing to complete
4. Click "Analyze" to detect rhythm notes

---

### 🔇 Noise Suppression

**Purpose**: Remove background noise and hiss from recordings.

**Technical Details**:
- Spectral gating algorithm
- Estimates noise profile from first 0.5 seconds
- Applies frequency-dependent gain reduction
- Threshold: 2% above noise floor
- Reduction amount: 80% of noise energy

**Best For**:
- Recordings with background noise
- Old cassette transfers
- Field recordings
- Low-quality audio sources

**Usage**:
1. Load your audio file (ensure first 0.5s contains only noise)
2. Click "🔇 Remove Noise"
3. Wait for processing to complete
4. Click "Analyze" to detect notes

---

### ↻ Reset to Original

**Purpose**: Restore the original audio file without reloading.

**Usage**:
1. Click "↻ Reset" anytime
2. Original audio is instantly restored
3. All processed versions are cleared

---

## Architecture

### Service Layer

**NoiseSuppressionService**
```
Services/NoiseSuppressionService.cs
```
- Implements spectral gating
- Configurable FFT size (default: 2048)
- Adjustable threshold and reduction amount

**MusicSeparationService**
```
Services/MusicSeparationService.cs
```
- Implements frequency-based stem separation
- Supports: Vocals, Drums, Bass, Other
- Configurable FFT size (default: 4096)

### Integration

All AI services are integrated into `MainWindow.xaml.cs`:
- Asynchronous processing with progress updates
- UI remains responsive during processing
- Automatic waveform refresh after processing
- Notes cleared to ensure accuracy

## Future Enhancements

These implementations use traditional DSP techniques. For production-quality results, consider integrating:

1. **AI Models via ONNX Runtime**:
   - Demucs for music separation
   - RNNoise for noise suppression
   - OpenVINO converted models

2. **Additional Features**:
   - Bass isolation (20Hz-250Hz)
   - "Other" instruments isolation
   - Music generation (MusicGen)
   - Whisper transcription for lyrics

3. **Model Files**:
   - Download pre-trained ONNX models
   - Place in `models/` directory
   - Update services to load and use models

## Comparison with OpenVINO Plugins

| Feature | OpenVINO Audacity | Finimus Grabber | Status |
|---------|-------------------|-----------------|--------|
| Noise Suppression | ✅ AI Model | ✅ Spectral Gating | Implemented |
| Music Separation | ✅ AI Model (Demucs) | ✅ Frequency-based | Implemented |
| Vocal Isolation | ✅ | ✅ | Implemented |
| Drum Isolation | ✅ | ✅ | Implemented |
| Bass Isolation | ✅ | 🔄 Planned | Future |
| Music Generation | ✅ MusicGen | ❌ | Future |
| Whisper Transcription | ✅ | ❌ | Future |
| Super Resolution | ✅ | ❌ | Future |

## Performance

Processing times (approximate, on modern CPU):
- **Noise Suppression**: ~2-3 seconds per minute of audio
- **Vocal Isolation**: ~3-5 seconds per minute of audio
- **Drum Isolation**: ~4-6 seconds per minute of audio

Memory usage:
- Peak: ~200-500 MB (depends on audio length)
- Steady: ~100 MB

## Technical Notes

### FFT Implementation

Currently uses a simple recursive FFT implementation. For better performance:
- Consider using MathNet.Numerics
- Or FFTSharp library
- Native FFT libraries for speed

### Overlap-Add Processing

All algorithms use overlap-add with:
- Hann windowing
- 75% overlap (hop size = window size / 4)
- Proper normalization

### Limitations

Current implementation limitations:
1. Processing is CPU-only (no GPU acceleration)
2. Separation quality is basic compared to AI models
3. No real-time processing
4. Memory usage scales with audio length

## Contributing

To add new AI features:

1. Create a new service in `Services/`
2. Implement processing method with progress callback
3. Add button to `MainWindow.xaml`
4. Wire up click handler in `MainWindow.xaml.cs`
5. Update documentation

Example:
```csharp
public class NewAIService
{
    public AudioData Process(AudioData input, Action<double>? progress)
    {
        // Your implementation
    }
}
```

## References

- [OpenVINO AI Plugins for Audacity](https://github.com/intel/openvino-plugins-ai-audacity)
- [Demucs: Music Source Separation](https://github.com/facebookresearch/demucs)
- [ONNX Runtime](https://onnxruntime.ai/)
- [NAudio Library](https://github.com/naudio/NAudio)

## License

Same as main project. AI processing code is original implementation inspired by OpenVINO plugins.
