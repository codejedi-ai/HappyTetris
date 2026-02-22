namespace HappyTetris.Models
{
    public enum PieceType
    {
        I, L, J, O, Z, S, T
    }

    public class Piece
    {
        public PieceType Type { get; set; }
        public int[,] Matrix { get; set; }
        public string Color { get; set; }

        public static Piece Create(PieceType type)
        {
            var piece = new Piece { Type = type };
            piece.Matrix = type switch
            {
                PieceType.I => new int[,] { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 } },
                PieceType.L => new int[,] { { 0, 2, 0 }, { 0, 2, 0 }, { 0, 2, 2 } },
                PieceType.J => new int[,] { { 0, 3, 0 }, { 0, 3, 0 }, { 3, 3, 0 } },
                PieceType.O => new int[,] { { 4, 4 }, { 4, 4 } },
                PieceType.Z => new int[,] { { 5, 5, 0 }, { 0, 5, 5 }, { 0, 0, 0 } },
                PieceType.S => new int[,] { { 0, 6, 6 }, { 6, 6, 0 }, { 0, 0, 0 } },
                PieceType.T => new int[,] { { 0, 7, 0 }, { 7, 7, 7 }, { 0, 0, 0 } },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            piece.Color = GetColor(type);
            return piece;
        }

        private static string GetColor(PieceType type) => type switch
        {
            PieceType.I => "#c41e3a",
            PieceType.L => "#ffd700",
            PieceType.J => "#ff6b35",
            PieceType.O => "#ffcc00",
            PieceType.Z => "#8b0000",
            PieceType.S => "#fff8e7",
            PieceType.T => "#00a86b",
            _ => "#ffffff"
        };

        public static PieceType GetRandomType()
        {
            var types = Enum.GetValues<PieceType>();
            return types[Random.Shared.Next(types.Length)];
        }
    }
}
