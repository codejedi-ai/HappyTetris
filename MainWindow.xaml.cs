using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using HappyTetris.Audio;
using HappyTetris.Engine;
using HappyTetris.Models;

namespace HappyTetris
{
    public partial class MainWindow : Window
    {
        private readonly GameEngine _gameEngine;
        private readonly AudioController _audioController;
        private readonly DispatcherTimer _gameTimer;
        private readonly int _cellSize = 30; // Clean cell size for 360x720 board (12x24 grid)

        public MainWindow()
        {
            InitializeComponent();
            
            _gameEngine = new GameEngine();
            _audioController = new AudioController();
            _gameTimer = new DispatcherTimer();
            
            SetupGameEvents();
            SetupTimer();
            
            ClearGameCanvas();
        }

        private void SetupGameEvents()
        {
            _gameEngine.OnGameUpdated += DrawGame;
            _gameEngine.OnScoreUpdated += UpdateScore;
            _gameEngine.OnGameOver += ShowGameOver;
            _gameEngine.OnLinesCleared += (lines) =>
            {
                if (lines > 0)
                    _audioController.PlayClear();
            };
            _gameEngine.OnLevelUp += () => _audioController.PlayLevelUp();
        }

        private void SetupTimer()
        {
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _gameTimer.Tick += (s, e) =>
            {
                _gameEngine.Update(Environment.TickCount);
            };
        }

        private void ClearGameCanvas()
        {
            GameCanvas.Children.Clear();
            
            // Draw grid - subtle lines for better visibility
            for (int x = 0; x <= 12; x++)
            {
                var line = new Line
                {
                    X1 = x * _cellSize,
                    Y1 = 0,
                    X2 = x * _cellSize,
                    Y2 = 24 * _cellSize,
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                    StrokeThickness = 0.5,
                    Opacity = 0.15
                };
                GameCanvas.Children.Add(line);
            }
            
            for (int y = 0; y <= 24; y++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y * _cellSize,
                    X2 = 12 * _cellSize,
                    Y2 = y * _cellSize,
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                    StrokeThickness = 0.5,
                    Opacity = 0.15
                };
                GameCanvas.Children.Add(line);
            }
        }

        private void DrawGame()
        {
            ClearGameCanvas();
            
            // Draw board
            var grid = _gameEngine.Board.GetGrid();
            for (int y = 0; y < 24; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    if (grid[y, x] != 0)
                    {
                        DrawCell(x, y, GetColorForValue(grid[y, x]));
                    }
                }
            }
            
            // Draw current piece
            if (_gameEngine.Player.CurrentPiece != null)
            {
                var piece = _gameEngine.Player.CurrentPiece;
                for (int y = 0; y < piece.Matrix.GetLength(0); y++)
                {
                    for (int x = 0; x < piece.Matrix.GetLength(1); x++)
                    {
                        if (piece.Matrix[y, x] != 0 && 
                            _gameEngine.Player.PositionY + y >= 0)
                        {
                            DrawCell(
                                _gameEngine.Player.PositionX + x,
                                _gameEngine.Player.PositionY + y,
                                piece.Color);
                        }
                    }
                }
            }
        }

        private void DrawCell(int x, int y, string color)
        {
            var rect = new Rectangle
            {
                Width = _cellSize,
                Height = _cellSize,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
            };
            
            Canvas.SetLeft(rect, x * _cellSize);
            Canvas.SetTop(rect, y * _cellSize);
            
            GameCanvas.Children.Add(rect);
            
            // Add border
            var border = new Rectangle
            {
                Width = _cellSize,
                Height = _cellSize,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                StrokeThickness = 1
            };
            
            Canvas.SetLeft(border, x * _cellSize);
            Canvas.SetTop(border, y * _cellSize);
            
            GameCanvas.Children.Add(border);
            
            // Add highlight
            var highlight = new Rectangle
            {
                Width = _cellSize - 6,
                Height = _cellSize - 6,
                Fill = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255))
            };
            
            Canvas.SetLeft(highlight, x * _cellSize + 3);
            Canvas.SetTop(highlight, y * _cellSize + 3);
            
            GameCanvas.Children.Add(highlight);
        }

        private string GetColorForValue(int value) => value switch
        {
            1 => "#c41e3a",  // Red
            2 => "#ffd700",  // Gold
            3 => "#ff6b35",  // Orange
            4 => "#ffcc00",  // Yellow
            5 => "#8b0000",  // Dark red
            6 => "#fff8e7",  // Cream
            7 => "#00a86b",  // Jade
            _ => "#ffffff"
        };

        private void UpdateScore()
        {
            ScoreText.Text = _gameEngine.GetScore().ToString();
            LevelText.Text = _gameEngine.GetLevel().ToString();
            LinesText.Text = _gameEngine.GetLines().ToString();
        }

        private void ShowGameOver()
        {
            _gameTimer.Stop();
            FinalScoreText.Text = _gameEngine.GetScore().ToString();
            GameOverOverlay.Visibility = Visibility.Visible;
            _audioController.PlayGameOver();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Start game with Space when on start screen
            if (StartOverlay.Visibility == Visibility.Visible && e.Key == Key.Space)
            {
                StartGame();
                return;
            }
            
            // Restart with Space on game over
            if (GameOverOverlay.Visibility == Visibility.Visible && e.Key == Key.Space)
            {
                StartGame();
                return;
            }
            
            if (e.Key == Key.H || e.Key == Key.F1)
            {
                ShowHelp();
                return;
            }
            
            if (_gameEngine.IsGameOver) return;
            
            if (e.Key == Key.P)
            {
                TogglePause();
                return;
            }
            
            if (_gameEngine.IsPaused || !_gameEngine.IsPlaying) return;
            
            switch (e.Key)
            {
                case Key.Left:
                    _gameEngine.MoveLeft();
                    _audioController.PlayMove();
                    break;
                case Key.Right:
                    _gameEngine.MoveRight();
                    _audioController.PlayMove();
                    break;
                case Key.Down:
                    _gameEngine.DropPiece();
                    _audioController.PlayDrop();
                    break;
                case Key.Up:
                    _gameEngine.Rotate();
                    break;
                case Key.Space:
                    _gameEngine.HardDrop();
                    _audioController.PlayDrop();
                    break;
            }
            
            e.Handled = true;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            _gameEngine.Start();
            StartOverlay.Visibility = Visibility.Collapsed;
            GameOverOverlay.Visibility = Visibility.Collapsed;
            PauseOverlay.Visibility = Visibility.Collapsed;
            _audioController.PlayStart();
            _gameTimer.Start();
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void ResumeBtn_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void TogglePause()
        {
            if (_gameEngine.IsGameOver || !_gameEngine.IsPlaying) return;
            
            _gameEngine.TogglePause();
            
            if (_gameEngine.IsPaused)
            {
                PauseOverlay.Visibility = Visibility.Visible;
                _gameTimer.Stop();
            }
            else
            {
                PauseOverlay.Visibility = Visibility.Collapsed;
                _gameTimer.Start();
            }
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "🎮\n\n" +
                "⬅️ ➡️\n" +
                "⬆️\n" +
                "⬇️\n" +
                "␣\n\n" +
                "🏆\n\n" +
                "1️⃣ 📋 = 1️⃣0️⃣0️⃣\n" +
                "2️⃣ 📋 = 3️⃣0️⃣0️⃣\n" +
                "3️⃣ 📋 = 5️⃣0️⃣0️⃣\n" +
                "4️⃣ 📋 = 8️⃣0️⃣0️⃣\n\n" +
                "P = ⏸️\n" +
                "␣ = ⬇️⬇️⬇️",
                "❓",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AudioBtn_Click(object sender, RoutedEventArgs e)
        {
            _audioController.Toggle();
            AudioBtn.Content = _audioController.Enabled ? "🔊" : "🔇";
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowHelp();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _audioController.Cleanup();
            _gameTimer.Stop();
            base.OnClosing(e);
        }
    }
}
