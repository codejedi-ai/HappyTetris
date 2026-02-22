namespace HappyTetris.Models
{
    public class Board
    {
        private readonly int[,] _grid;
        public int Width { get; }
        public int Height { get; }

        public Board(int width = 12, int height = 24)
        {
            Width = width;
            Height = height;
            _grid = new int[height, width];
        }

        public void Clear()
        {
            Array.Clear(_grid, 0, _grid.Length);
        }

        public int this[int y, int x]
        {
            get
            {
                if (y < 0 || y >= Height || x < 0 || x >= Width)
                    return -1;
                return _grid[y, x];
            }
            set
            {
                if (y >= 0 && y < Height && x >= 0 && x < Width)
                    _grid[y, x] = value;
            }
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y < Height && (y < 0 || _grid[y, x] == 0);
        }

        public bool Collides(int[,] piece, int posX, int posY)
        {
            int height = piece.GetLength(0);
            int width = piece.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (piece[y, x] != 0)
                    {
                        int boardX = posX + x;
                        int boardY = posY + y;

                        if (boardX < 0 || boardX >= Width || boardY >= Height)
                            return true;

                        if (boardY >= 0 && _grid[boardY, boardX] != 0)
                            return true;
                    }
                }
            }

            return false;
        }

        public void MergePiece(int[,] piece, int posX, int posY)
        {
            int height = piece.GetLength(0);
            int width = piece.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (piece[y, x] != 0 && posY + y >= 0)
                    {
                        _grid[posY + y, posX + x] = piece[y, x];
                    }
                }
            }
        }

        public int ClearFullLines()
        {
            int linesCleared = 0;

            for (int y = Height - 1; y >= 0; y--)
            {
                bool isFull = true;
                for (int x = 0; x < Width; x++)
                {
                    if (_grid[y, x] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    linesCleared++;
                    // Move all rows above down
                    for (int moveY = y; moveY > 0; moveY--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            _grid[moveY, x] = _grid[moveY - 1, x];
                        }
                    }
                    // Clear top row
                    for (int x = 0; x < Width; x++)
                    {
                        _grid[0, x] = 0;
                    }
                    y++; // Check same row again
                }
            }

            return linesCleared;
        }

        public int[,] GetGrid()
        {
            return (int[,])_grid.Clone();
        }
    }
}
