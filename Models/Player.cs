namespace HappyTetris.Models
{
    public class Player
    {
        public Piece? CurrentPiece { get; set; }
        public Piece? NextPiece { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Score { get; set; }
        public int Level { get; set; }
        public int LinesCleared { get; set; }

        public Player()
        {
            Level = 1;
            Score = 0;
            LinesCleared = 0;
        }

        public void Reset()
        {
            CurrentPiece = null;
            NextPiece = null;
            PositionX = 0;
            PositionY = 0;
            Score = 0;
            Level = 1;
            LinesCleared = 0;
        }

        public void SpawnPiece(Board board)
        {
            if (NextPiece == null)
            {
                NextPiece = Piece.Create(Piece.GetRandomType());
            }

            CurrentPiece = NextPiece;
            NextPiece = Piece.Create(Piece.GetRandomType());

            PositionX = (board.Width / 2) - (CurrentPiece.Matrix.GetLength(1) / 2);
            PositionY = 0;
        }

        public int GetDropInterval()
        {
            // Slower starting speed for elderly-friendly gameplay
            // Level 1 starts at 2000ms (2 seconds) instead of 1500ms
            // Minimum speed is 800ms instead of 500ms for more reaction time
            return Math.Max(800, 2000 - (Level - 1) * 120);
        }

        public void AddLinesCleared(int lines)
        {
            LinesCleared += lines;
            int[] lineScores = { 0, 100, 300, 500, 800 };
            Score += lineScores[lines] * Level;

            int newLevel = (LinesCleared / 10) + 1;
            if (newLevel > Level)
            {
                Level = newLevel;
            }
        }
    }
}
