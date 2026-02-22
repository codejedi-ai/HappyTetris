using System.Globalization;

namespace HappyTetris.Audio
{
    public sealed class MusicNote
    {
        private static readonly IReadOnlyDictionary<string, int> NoteSemitonesFromC =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["C"] = 0,
                ["C#"] = 1,
                ["D"] = 2,
                ["D#"] = 3,
                ["E"] = 4,
                ["F"] = 5,
                ["F#"] = 6,
                ["G"] = 7,
                ["G#"] = 8,
                ["A"] = 9,
                ["A#"] = 10,
                ["B"] = 11
            };

        private MusicNote(string name, double pitchHz, int durationMs)
        {
            if (durationMs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration must be greater than zero.");
            }

            Name = name;
            PitchHz = pitchHz;
            DurationMs = durationMs;
        }

        public string Name { get; }

        public double PitchHz { get; }

        public int DurationMs { get; }

        public bool IsRest => PitchHz <= 0;

        public static MusicNote Named(string name, int durationMs)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Note name is required.", nameof(name));
            }

            string canonicalName = name.Trim().ToUpperInvariant();
            if (!TryGetPitchHz(canonicalName, out double pitchHz))
            {
                throw new ArgumentException(
                    $"Unknown note '{name}'. Use formats like A4, C#5, F3.",
                    nameof(name));
            }

            return new MusicNote(canonicalName, pitchHz, durationMs);
        }

        public static MusicNote Rest(int durationMs)
        {
            return new MusicNote("REST", 0, durationMs);
        }

        public override string ToString()
        {
            return IsRest
                ? $"{Name} ({DurationMs}ms)"
                : $"{Name} {PitchHz.ToString("0.00", CultureInfo.InvariantCulture)}Hz ({DurationMs}ms)";
        }

        private static bool TryGetPitchHz(string noteName, out double pitchHz)
        {
            pitchHz = 0;
            if (string.IsNullOrWhiteSpace(noteName) || noteName.Length < 2)
            {
                return false;
            }

            int octaveStart = noteName.Length - 1;
            if (!char.IsDigit(noteName[octaveStart]))
            {
                return false;
            }

            string tone = noteName[..octaveStart];
            string octaveText = noteName[octaveStart..];
            if (!int.TryParse(octaveText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int octave))
            {
                return false;
            }

            if (!NoteSemitonesFromC.TryGetValue(tone, out int semitoneFromC))
            {
                return false;
            }

            // MIDI note formula: C-1 is 0, A4 is 69.
            int midiNote = ((octave + 1) * 12) + semitoneFromC;
            pitchHz = 440.0 * Math.Pow(2, (midiNote - 69) / 12.0);
            return true;
        }
    }
}
