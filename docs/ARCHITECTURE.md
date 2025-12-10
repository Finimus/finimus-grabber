# Architecture Documentation

## System Overview

Finimus Grabber is designed as a modular, event-driven desktop application with clear separation between audio processing, data analysis, visualization, and user interaction layers.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        UI Layer                              │
│  (Waveform Display, Piano Roll, Controls, Menus)            │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Controller                    │
│  (Event Handling, State Management, User Actions)           │
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ Audio Loader │    │   Analysis   │    │    MIDI      │
│    Module    │───▶│    Engine    │───▶│   Exporter   │
└──────────────┘    └──────────────┘    └──────────────┘
        │                   │
        ▼                   ▼
┌──────────────┐    ┌──────────────┐
│ Audio Buffer │    │ Musical Data │
│   (Memory)   │    │  (Notes/Chords)│
└──────────────┘    └──────────────┘
```

## Core Modules

### 1. Audio Loader Module

**Responsibility:** Load and decode audio files into raw PCM data

**Components:**
- `FileReader` – Opens and validates audio files
- `MP3Decoder` – Decodes MP3 using libmp3lame or similar
- `WAVDecoder` – Parses WAV/PCM data with libsndfile
- `FLACDecoder` – Decodes FLAC format
- `AudioBuffer` – Manages in-memory audio samples

**Key Operations:**
```cpp
class AudioLoader {
public:
    bool LoadFile(const std::string& filepath);
    AudioBuffer* GetBuffer();
    AudioMetadata GetMetadata(); // sample rate, channels, duration
};
```

**Dependencies:**
- libsndfile (WAV/FLAC)
- libmp3lame or minimp3 (MP3)

### 2. Analysis Engine

**Responsibility:** Process audio data to extract musical information

**Components:**

#### FFT Processor
- Performs Fast Fourier Transform on windowed audio segments
- Configurable window size (1024, 2048, 4096 samples)
- Hop size determines time resolution
- Outputs frequency-domain representation

#### Onset Detector
- Identifies note attack points using spectral flux
- Marks temporal boundaries of musical events
- Adaptive threshold based on signal energy

#### Pitch Detector
- Extracts fundamental frequency from spectral data
- Uses autocorrelation or YIN algorithm
- Converts frequency to MIDI note number
- Handles vibrato and pitch bends

#### Harmonic Analyzer
- Separates harmonic and percussive content
- Identifies overtone series
- Filters noise from musical content

#### Chord Recognizer
- Analyzes simultaneous notes
- Matches patterns to chord templates
- Identifies chord types (major, minor, 7th, etc.)

**Key Operations:**
```cpp
class AnalysisEngine {
public:
    void Analyze(const AudioBuffer* buffer);
    std::vector<Note> GetDetectedNotes();
    std::vector<Chord> GetDetectedChords();
    SpectrogramData GetSpectrogram();
};

struct Note {
    double time_seconds;
    int midi_note;
    double frequency_hz;
    double duration_seconds;
    int velocity;
    double confidence;
};
```

**Dependencies:**
- FFTW3 or Kiss FFT
- Custom DSP algorithms

### 3. Visualization Module

**Responsibility:** Render audio and musical data for user display

**Components:**

#### Waveform Renderer
- Draws audio amplitude over time
- Zoom and pan capabilities
- Marker overlay for detected notes

#### Spectrogram Renderer
- Real-time frequency spectrum display
- Color-coded intensity mapping
- Logarithmic frequency scale option

#### Piano Roll Renderer
- MIDI-style grid display
- Interactive note editing
- Velocity visualization

**Key Operations:**
```cpp
class Visualizer {
public:
    void RenderWaveform(const AudioBuffer* buffer, RenderContext* context);
    void RenderSpectrogram(const SpectrogramData& data, RenderContext* context);
    void RenderPianoRoll(const std::vector<Note>& notes, RenderContext* context);
};
```

**Dependencies:**
- Graphics API (GDI+, Direct2D, or WPF)
- OpenGL (optional for GPU acceleration)

### 4. MIDI Exporter

**Responsibility:** Convert detected notes to standard MIDI file format

**Components:**

#### MIDI Formatter
- Constructs MIDI messages (Note On, Note Off)
- Handles tempo and time signature
- Supports multiple tracks

#### File Writer
- Serializes MIDI data to binary format
- Supports Type 0 (single track) and Type 1 (multi-track)
- Configurable PPQN (pulses per quarter note)

**Key Operations:**
```cpp
class MIDIExporter {
public:
    void SetNotes(const std::vector<Note>& notes);
    void SetTempo(double bpm);
    bool ExportToFile(const std::string& filepath, MIDIFormat format);
};
```

**Dependencies:**
- Custom MIDI serialization or third-party library (e.g., midifile)

### 5. UI Layer

**Responsibility:** User interaction and application workflow

**Components:**

#### Main Window
- File menu, toolbar, status bar
- Docked panels for different views
- Drag-and-drop file loading

#### Control Panel
- Playback controls (play, pause, stop)
- Analysis settings (FFT size, threshold)
- Export options

#### Editor Panel
- Manual note editing tools
- Quantization controls
- Undo/redo management

**Technology Options:**
- **WPF** (XAML-based, modern UI)
- **WinForms** (simpler, faster development)
- **Qt** (cross-platform, if future expansion needed)

## Data Flow

### Loading and Analysis Workflow

1. User selects audio file via UI
2. `AudioLoader` decodes file into `AudioBuffer`
3. UI displays waveform using `Visualizer`
4. `AnalysisEngine` processes buffer in background thread
5. Detected notes populate data structures
6. UI updates piano roll and spectrogram in real-time
7. User reviews and edits results
8. `MIDIExporter` generates MIDI file on export command

### Real-Time Processing

For real-time playback with live analysis:
1. Audio buffer streamed in chunks to audio output
2. Concurrent FFT processing on each chunk
3. Results buffered and displayed with minimal latency
4. UI thread updates visualizations at 30-60 FPS

## Performance Considerations

### Multi-Threading

- **Audio I/O Thread:** Handles file loading and decoding
- **Analysis Thread:** Performs FFT and pitch detection
- **UI Thread:** Renders graphics and handles user input
- **Export Thread:** Writes MIDI files without blocking UI

### Memory Management

- Stream large audio files in chunks rather than loading entirely
- Use circular buffers for real-time processing
- Pool memory allocations for FFT buffers
- Release resources immediately after export

### Optimization Techniques

- **SIMD:** Vectorize FFT and DSP operations
- **GPU Acceleration:** Offload spectrogram rendering to GPU
- **Algorithmic:** Adaptive FFT window sizing based on content
- **Caching:** Store pre-computed FFT results for repeated analysis

## Configuration and Settings

### User Preferences

- Default FFT window size
- Onset detection sensitivity
- MIDI export resolution (PPQN)
- UI theme and color scheme

### Analysis Parameters

```json
{
  "fft_window_size": 2048,
  "hop_size": 512,
  "onset_threshold": 0.3,
  "pitch_detection_algorithm": "YIN",
  "min_note_duration_ms": 50,
  "confidence_threshold": 0.7
}
```

## Error Handling

- Validate audio file formats before loading
- Gracefully handle corrupted or unsupported files
- Display user-friendly error messages
- Log technical details for debugging

## Testing Strategy

### Unit Tests

- Test each module independently
- Mock dependencies (e.g., `AudioBuffer` for `AnalysisEngine`)
- Verify FFT correctness with known signals
- Validate MIDI output against reference files

### Integration Tests

- Load real audio files and verify end-to-end processing
- Compare detected notes against ground truth
- Test performance with large files (>100MB)

### User Acceptance Tests

- Musicians validate note detection accuracy
- Test with diverse audio sources (vocals, instruments, electronic)
- Verify MIDI imports correctly into DAWs

## Future Enhancements

- Batch processing of multiple files
- Plugin architecture for custom analysis algorithms
- Cloud sync for project files (optional)
- Support for additional formats (OGG, AAC)
- MIDI input for hybrid workflows
- Machine learning-based note detection

## Security and Privacy

- No network communication (fully offline)
- No telemetry or analytics
- User data never leaves local machine
- Code signing to prevent tampering

## Build and Deployment

- Single executable deployment (~25MB)
- Static linking to eliminate DLL dependencies
- Windows code signing certificate
- Optional installer for Start Menu integration

---

For development setup and build instructions, see [DEVELOPMENT.md](DEVELOPMENT.md).
