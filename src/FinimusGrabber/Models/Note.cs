namespace FinimusGrabber.Models
{
    /// <summary>
    /// Represents a detected musical note with timing and MIDI information
    /// </summary>
    public class Note
    {
        public double TimeSeconds { get; set; }
        public int MidiNote { get; set; }
        public double FrequencyHz { get; set; }
        public double DurationSeconds { get; set; }
        public int Velocity { get; set; }
        public double Confidence { get; set; }

        public Note()
        {
            Velocity = 100; // Default velocity
            Confidence = 1.0; // Default confidence
        }

        /// <summary>
        /// Get the note name (e.g., "C4", "A#3")
        /// </summary>
        public string NoteName
        {
            get
            {
                string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
                int octave = (MidiNote / 12) - 1;
                int noteIndex = MidiNote % 12;
                return $"{noteNames[noteIndex]}{octave}";
            }
        }

        /// <summary>
        /// Convert frequency to MIDI note number
        /// </summary>
        public static int FrequencyToMidi(double frequency)
        {
            if (frequency <= 0) return 0;
            return (int)Math.Round(69 + 12 * Math.Log2(frequency / 440.0));
        }

        /// <summary>
        /// Convert MIDI note number to frequency
        /// </summary>
        public static double MidiToFrequency(int midiNote)
        {
            return 440.0 * Math.Pow(2.0, (midiNote - 69) / 12.0);
        }
    }
}
