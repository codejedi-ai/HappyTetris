using HappyTetris.Models;

namespace HappyTetris.Engine
{
    public class GameEngine
    {
        private readonly Board _board;
        private readonly Player _player;
        private readonly Random _random;
        
        public event Action? OnGameUpdated;
        public event Action? OnScoreUpdated;
        public event Action? OnGameOver;
        public event Action<int>? OnLinesCleared;
        public event Action? OnLevelUp;

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsGameOver { get; private set; }

        private int _dropCounter = 0;
        private int _lastDropTime = 0;

        public GameEngine()
        {
            _board = new Board();
            _player = new Player();
            _random = new Random();
            IsPlaying = false;
            IsPaused = false;
            IsGameOver = false;
        }

        public Board Board => _board;
        public Player Player => _player;

        public void Start()
        {
            _board.Clear();
            _player.Reset();
            _player.NextPiece = Piece.Create(Piece.GetRandomType());
            _player.SpawnPiece(_board);
            
            IsPlaying = true;
            IsPaused = false;
            IsGameOver = false;
            _dropCounter = 0;
            _lastDropTime = 0;

            OnScoreUpdated?.Invoke();
            OnGameUpdated?.Invoke();
        }

        public void Update(int currentTime)
        {
            if (!IsPlaying || IsPaused || IsGameOver) return;

            if (_lastDropTime == 0)
                _lastDropTime = currentTime;

            int deltaTime = currentTime - _lastDropTime;
            _lastDropTime = currentTime;
            _dropCounter += deltaTime;

            int dropInterval = _player.GetDropInterval();

            if (_dropCounter >= dropInterval)
            {
                DropPiece();
                _dropCounter = 0;
            }

            OnGameUpdated?.Invoke();
        }

        public void MoveLeft()
        {
            if (!CanMove()) return;

            if (_player.CurrentPiece != null &&
                !_board.Collides(_player.CurrentPiece.Matrix, _player.PositionX - 1, _player.PositionY))
            {
                _player.PositionX--;
                OnGameUpdated?.Invoke();
            }
        }

        public void MoveRight()
        {
            if (!CanMove()) return;

            if (_player.CurrentPiece != null &&
                !_board.Collides(_player.CurrentPiece.Matrix, _player.PositionX + 1, _player.PositionY))
            {
                _player.PositionX++;
                OnGameUpdated?.Invoke();
            }
        }

        public void DropPiece()
        {
            if (_player.CurrentPiece == null) return;

            _player.PositionY++;

            if (_board.Collides(_player.CurrentPiece.Matrix, _player.PositionX, _player.PositionY))
            {
                _player.PositionY--;
                LockPiece();
            }
        }

        public void HardDrop()
        {
            if (_player.CurrentPiece == null) return;

            while (!_board.Collides(_player.CurrentPiece.Matrix, _player.PositionX, _player.PositionY + 1))
            {
                _player.PositionY++;
            }

            LockPiece();
        }

        public void Rotate()
        {
            if (_player.CurrentPiece == null || !CanMove()) return;

            int[,] rotated = RotateMatrix(_player.CurrentPiece.Matrix);
            int originalX = _player.PositionX;
            int offset = 1;

            while (_board.Collides(rotated, _player.PositionX, _player.PositionY))
            {
                _player.PositionX += offset;
                offset = -(offset + (offset > 0 ? 1 : -1));

                if (offset > rotated.GetLength(1))
                {
                    _player.PositionX = originalX;
                    return;
                }
            }

            _player.CurrentPiece.Matrix = rotated;
            OnGameUpdated?.Invoke();
        }

        private int[,] RotateMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - 1 - y] = matrix[y, x];
                }
            }

            return rotated;
        }

        private void LockPiece()
        {
            if (_player.CurrentPiece == null) return;

            _board.MergePiece(_player.CurrentPiece.Matrix, _player.PositionX, _player.PositionY);
            
            int linesCleared = _board.ClearFullLines();
            if (linesCleared > 0)
            {
                _player.AddLinesCleared(linesCleared);
                OnLinesCleared?.Invoke(linesCleared);
                OnScoreUpdated?.Invoke();

                int newLevel = (_player.LinesCleared / 10) + 1;
                if (newLevel > _player.Level - 1)
                {
                    OnLevelUp?.Invoke();
                }
            }

            _player.SpawnPiece(_board);

            if (_board.Collides(_player.CurrentPiece.Matrix, _player.PositionX, _player.PositionY))
            {
                GameOver();
            }

            OnGameUpdated?.Invoke();
        }

        private bool CanMove()
        {
            return IsPlaying && !IsPaused && !IsGameOver && _player.CurrentPiece != null;
        }

        public void TogglePause()
        {
            if (IsGameOver || !IsPlaying) return;

            IsPaused = !IsPaused;
            _lastDropTime = 0;
        }

        private void GameOver()
        {
            IsPlaying = false;
            IsGameOver = true;
            OnGameOver?.Invoke();
        }

        public int GetScore() => _player.Score;
        public int GetLevel() => _player.Level;
        public int GetLines() => _player.LinesCleared;
    }
}
