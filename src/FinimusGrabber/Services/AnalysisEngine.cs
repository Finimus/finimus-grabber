using FinimusGrabber.Models;
using System.Numerics;

namespace FinimusGrabber.Services
{
    /// <summary>
    /// Analyzes audio to detect notes automatically
    /// </summary>
    public class AnalysisEngine
    {
        private readonly int _fftSize;
        private readonly int _hopSize;
        private readonly double _minNoteConfidence;

        public AnalysisEngine(int fftSize = 2048, int hopSize = 512, double minConfidence = 0.3)
        {
            _fftSize = fftSize;
            _hopSize = hopSize;
            _minNoteConfidence = minConfidence;
        }

        /// <summary>
        /// Analyze audio and detect notes
        /// </summary>
        public List<Note> Analyze(AudioData audioData)
        {
            if (audioData.Samples.Length == 0)
                return new List<Note>();

            // Convert to mono if stereo
            var monoSamples = audioData.Channels == 1 
                ? audioData.Samples 
                : ConvertToMono(audioData.Samples, audioData.Channels);

            var notes = new List<Note>();
            var sampleRate = audioData.SampleRate;

            // Process audio in overlapping windows
            for (int i = 0; i < monoSamples.Length - _fftSize; i += _hopSize)
            {
                var window = new float[_fftSize];
                Array.Copy(monoSamples, i, window, 0, _fftSize);

                // Apply Hann window
                ApplyHannWindow(window);

                // Perform FFT
                var spectrum = PerformFFT(window);

                // Detect fundamental frequency
                var frequency = DetectPitch(spectrum, sampleRate);

                if (frequency > 0 && frequency >= 65 && frequency <= 2100) // C2 to C7 range
                {
                    var midiNote = Note.FrequencyToMidi(frequency);
                    var confidence = CalculateConfidence(spectrum, frequency, sampleRate);

                    if (confidence >= _minNoteConfidence)
                    {
                        // Check if this continues the previous note
                        if (notes.Count > 0 && notes[^1].MidiNote == midiNote)
                        {
                            // Extend duration of last note
                            notes[^1].DurationSeconds += (double)_hopSize / sampleRate;
                        }
                        else
                        {
                            // Create new note
                            var note = new Note
                            {
                                TimeSeconds = (double)i / sampleRate,
                                MidiNote = midiNote,
                                FrequencyHz = frequency,
                                DurationSeconds = (double)_hopSize / sampleRate,
                                Velocity = (int)(confidence * 127),
                                Confidence = confidence
                            };
                            notes.Add(note);
                        }
                    }
                }
            }

            // Filter out very short notes (< 50ms)
            return notes.Where(n => n.DurationSeconds >= 0.05).ToList();
        }

        private float[] ConvertToMono(float[] samples, int channels)
        {
            var monoSamples = new float[samples.Length / channels];
            for (int i = 0; i < monoSamples.Length; i++)
            {
                float sum = 0;
                for (int ch = 0; ch < channels; ch++)
                {
                    sum += samples[i * channels + ch];
                }
                monoSamples[i] = sum / channels;
            }
            return monoSamples;
        }

        private void ApplyHannWindow(float[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                var window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (samples.Length - 1)));
                samples[i] *= (float)window;
            }
        }

        private Complex[] PerformFFT(float[] samples)
        {
            // Convert to complex numbers
            var complex = new Complex[_fftSize];
            for (int i = 0; i < samples.Length; i++)
            {
                complex[i] = new Complex(samples[i], 0);
            }

            // Simple FFT implementation (for production, use a library like MathNet.Numerics)
            return FFT(complex);
        }

        private Complex[] FFT(Complex[] x)
        {
            int N = x.Length;
            if (N <= 1) return x;

            // Divide
            var even = new Complex[N / 2];
            var odd = new Complex[N / 2];
            for (int i = 0; i < N / 2; i++)
            {
                even[i] = x[i * 2];
                odd[i] = x[i * 2 + 1];
            }

            // Conquer
            var fftEven = FFT(even);
            var fftOdd = FFT(odd);

            // Combine
            var result = new Complex[N];
            for (int k = 0; k < N / 2; k++)
            {
                var t = Complex.FromPolarCoordinates(1.0, -2 * Math.PI * k / N) * fftOdd[k];
                result[k] = fftEven[k] + t;
                result[k + N / 2] = fftEven[k] - t;
            }

            return result;
        }

        private double DetectPitch(Complex[] spectrum, int sampleRate)
        {
            // Find peak frequency in spectrum
            int maxIndex = 0;
            double maxMagnitude = 0;

            // Only check up to Nyquist frequency
            for (int i = 1; i < spectrum.Length / 2; i++)
            {
                double magnitude = spectrum[i].Magnitude;
                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                    maxIndex = i;
                }
            }

            if (maxMagnitude < 0.01) // Threshold for silence
                return 0;

            // Convert bin to frequency
            double frequency = (double)maxIndex * sampleRate / _fftSize;
            return frequency;
        }

        private double CalculateConfidence(Complex[] spectrum, double targetFreq, int sampleRate)
        {
            // Calculate confidence based on spectral clarity
            int targetBin = (int)(targetFreq * _fftSize / sampleRate);
            double peakMagnitude = spectrum[targetBin].Magnitude;
            
            // Calculate average magnitude of nearby bins
            double avgMagnitude = 0;
            int range = 5;
            for (int i = Math.Max(1, targetBin - range); i < Math.Min(spectrum.Length / 2, targetBin + range); i++)
            {
                avgMagnitude += spectrum[i].Magnitude;
            }
            avgMagnitude /= (range * 2);

            // Confidence is the ratio of peak to average
            double confidence = avgMagnitude > 0 ? Math.Min(1.0, peakMagnitude / (avgMagnitude * 3)) : 0;
            return confidence;
        }
    }
}
