# Development Setup

## Prerequisites

### Required Software

- **Visual Studio 2022** (Community, Professional, or Enterprise)
  - Desktop development with C++ workload (if using C++)
  - .NET desktop development workload (if using C#)
- **Git** for version control
- **Windows 10 SDK** (version 10.0.19041.0 or later)

### Recommended Tools

- **Visual Studio Code** for quick file editing
- **CMake** (if building with C++)
- **NuGet Package Manager** (included with Visual Studio)
- **Windows Terminal** for command-line operations

## Getting Started

### Clone the Repository

```powershell
git clone <repository-url>
cd finimus-grabber
```

### Project Configuration

1. Open the solution file in Visual Studio 2022
2. Restore NuGet packages (if using C#/.NET)
3. Configure build settings for Debug or Release mode
4. Set the startup project if multiple projects exist

## Audio Processing Libraries

### FFT and DSP

Consider integrating one of the following libraries:

- **FFTW3** – Fast Fourier Transform library (C++)
- **Kiss FFT** – Simple, lightweight FFT implementation
- **NAudio** – .NET audio library with comprehensive format support
- **libsndfile** – For reading WAV and FLAC files
- **LAME** or **libmp3lame** – For MP3 decoding

### Installation

Libraries should be managed via:
- **vcpkg** for C++ dependencies
- **NuGet** for .NET dependencies

Example with vcpkg:
```powershell
vcpkg install fftw3:x64-windows
vcpkg install libsndfile:x64-windows
```

## Build Instructions

### Debug Build

```powershell
# For C++ with CMake
cmake -B build -S . -DCMAKE_BUILD_TYPE=Debug
cmake --build build --config Debug

# For C# with dotnet CLI
dotnet build --configuration Debug
```

### Release Build

```powershell
# For C++ with CMake
cmake -B build -S . -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release

# For C# with dotnet CLI
dotnet build --configuration Release
```

### Output

Compiled executables will be placed in:
- `build/Debug/` or `build/Release/` (C++)
- `bin/Debug/` or `bin/Release/` (.NET)

## Project Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed technical documentation.

### Core Modules

1. **Audio Loader** – File I/O and format decoding
2. **Analysis Engine** – FFT processing and note detection
3. **Visualization** – Waveform and spectrum rendering
4. **MIDI Exporter** – Musical data serialization
5. **UI Layer** – User interface and interaction

## Testing

### Unit Tests

Run unit tests from Visual Studio Test Explorer or command line:

```powershell
# For C++ with Google Test
ctest --test-dir build

# For C# with xUnit/NUnit
dotnet test
```

### Integration Tests

Test with sample audio files located in the `examples/` directory:
- `examples/test_mp3.mp3`
- `examples/test_wav.wav`
- `examples/test_flac.flac`

## Code Style

- Use consistent indentation (4 spaces or tabs)
- Follow Microsoft C++ or C# coding conventions
- Comment complex algorithms and DSP functions
- Keep functions focused and modular

## Debugging

### Visual Studio Debugger

1. Set breakpoints in critical sections
2. Use Watch windows to monitor DSP buffers
3. Profile CPU and memory usage with Performance Profiler
4. Visualize audio data with custom debug visualizers

### Logging

Implement logging for:
- File loading events
- Analysis progress and results
- Error conditions and exceptions
- Performance metrics

## Performance Optimization

### Profiling

- Use Visual Studio Performance Profiler
- Identify CPU bottlenecks in FFT and pitch detection
- Optimize memory allocations in real-time processing

### Optimization Techniques

- SIMD instructions (SSE, AVX) for DSP operations
- Multi-threading for parallel FFT processing
- GPU acceleration with DirectCompute or CUDA
- Memory pooling for buffer management

## Continuous Integration

Consider setting up CI/CD with:
- GitHub Actions
- Azure Pipelines
- Jenkins

Automate:
- Build verification
- Unit test execution
- Static code analysis
- Release packaging

## Packaging and Distribution

### Creating the .exe

For a standalone executable:

1. Build in Release mode with optimizations
2. Link statically to eliminate DLL dependencies
3. Include all required resources in the executable
4. Code-sign the binary for Windows SmartScreen

### Installer (Optional)

While the app is designed to run standalone, an installer can be created with:
- **Inno Setup** – Lightweight Windows installer
- **WiX Toolset** – MSI-based installer
- **NSIS** – Nullsoft Scriptable Install System

## Contributing

Guidelines for contributors:

1. Create a feature branch from `main`
2. Implement changes with appropriate tests
3. Ensure all tests pass
4. Submit a pull request with clear description
5. Code review and approval required before merge

## Troubleshooting

### Common Issues

**Audio files not loading:**
- Verify codec support and library installation
- Check file permissions and paths
- Ensure sample rate is within supported range

**Poor note detection accuracy:**
- Tune FFT window size and hop length
- Adjust onset detection threshold
- Review polyphonic detection algorithm

**High CPU usage:**
- Profile with Performance Profiler
- Reduce FFT resolution if appropriate
- Optimize hot code paths with SIMD

## Resources

- [FFTW Documentation](http://www.fftw.org/fftw3_doc/)
- [NAudio Documentation](https://github.com/naudio/NAudio)
- [MIDI File Format Specification](https://www.midi.org/specifications)
- [Digital Signal Processing Resources](https://www.dspguide.com/)

---

For additional help, consult the [Architecture Guide](ARCHITECTURE.md) or contact the development team.
