using FinimusGrabber.Models;
using NAudio.Midi;

namespace FinimusGrabber.Services
{
    /// <summary>
    /// Exports detected notes to MIDI files
    /// </summary>
    public class MidiExporter
    {
        private readonly int _ticksPerQuarterNote;

        public MidiExporter(int ticksPerQuarterNote = 480)
        {
            _ticksPerQuarterNote = ticksPerQuarterNote;
        }

        /// <summary>
        /// Export notes to a MIDI file
        /// </summary>
        public void ExportToFile(List<Note> notes, string filePath, double bpm = 120)
        {
            if (notes == null || notes.Count == 0)
                throw new ArgumentException("No notes to export");

            var midiEvents = new MidiEventCollection(1, _ticksPerQuarterNote);
            
            // Add tempo
            int microsecondsPerQuarterNote = (int)(60000000.0 / bpm);
            midiEvents.AddEvent(new TempoEvent(microsecondsPerQuarterNote, 0), 0);

            // Add notes
            foreach (var note in notes)
            {
                int absoluteTime = SecondsToTicks(note.TimeSeconds, bpm);
                int duration = SecondsToTicks(note.DurationSeconds, bpm);

                // Note On
                var noteOn = new NoteOnEvent(
                    absoluteTime,
                    1, // Channel 1
                    note.MidiNote,
                    note.Velocity,
                    duration
                );
                midiEvents.AddEvent(noteOn, 0);

                // Note Off
                var noteOff = new NoteEvent(
                    absoluteTime + duration,
                    1,
                    MidiCommandCode.NoteOff,
                    note.MidiNote,
                    0
                );
                midiEvents.AddEvent(noteOff, 0);
            }

            // Write to file
            MidiFile.Export(filePath, midiEvents);
        }

        private int SecondsToTicks(double seconds, double bpm)
        {
            // Ticks = (seconds * BPM * TPQN) / 60
            return (int)(seconds * bpm * _ticksPerQuarterNote / 60.0);
        }
    }
}
