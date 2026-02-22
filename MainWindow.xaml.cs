using System.Globalization;
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
        private const double DesignWidth = 550;
        private const double DesignHeight = 763;
        /// <summary>Design height of the tetris board (top to bottom where blocks land) in pixels.</summary>
        private const double DesignBoardHeight = 660;
        /// <summary>Default max height of the board (top to bottom) in pixels.</summary>
        private const double DefaultMaxBoardHeightPx = 660;
        private const double MaxBoardHeightFraction = 0.92; // board cannot exceed 92% of screen height
        private const double MarginFromScreen = 24;

        private readonly GameEngine _gameEngine;
        private readonly AudioController _audioController;
        private readonly DispatcherTimer _gameTimer;
        private readonly ContextMenu _audioOptionsMenu;
        private readonly MenuItem _soundEffectsMenuItem;
        private readonly MenuItem _gameMusicMenuItem;
        private readonly double _cellSize = 27.5; // Cell size for 360x660 board (12x24 grid)
        private static readonly Color AccentGoldColor = Color.FromRgb(255, 215, 0);

        public MainWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;

            _gameEngine = new GameEngine();
            _audioController = new AudioController();
            _gameTimer = new DispatcherTimer();
            _audioOptionsMenu = new ContextMenu();
            _soundEffectsMenuItem = new MenuItem();
            _gameMusicMenuItem = new MenuItem();
            
            SetupGameEvents();
            SetupTimer();
            SetupAudioOptionsMenu();
            UpdatePauseButtonState();
            
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

        private void SetupAudioOptionsMenu()
        {
            _soundEffectsMenuItem.Header = "🔊 Sound Effects";
            _soundEffectsMenuItem.IsCheckable = true;
            _soundEffectsMenuItem.Click += (s, e) =>
            {
                _audioController.ToggleEffects();
                SyncAudioOptionsMenuState();
            };

            _gameMusicMenuItem.Header = "🎵 Game Music";
            _gameMusicMenuItem.IsCheckable = true;
            _gameMusicMenuItem.Click += (s, e) =>
            {
                _audioController.ToggleMusic();

                if (_audioController.MusicEnabled &&
                    _gameEngine.IsPlaying &&
                    !_gameEngine.IsPaused &&
                    !_gameEngine.IsGameOver)
                {
                    _audioController.StartBackgroundMusic();
                }
                else
                {
                    _audioController.StopBackgroundMusic();
                }

                SyncAudioOptionsMenuState();
            };

            _audioOptionsMenu.Items.Add(_soundEffectsMenuItem);
            _audioOptionsMenu.Items.Add(_gameMusicMenuItem);
            SyncAudioOptionsMenuState();
        }

        private void SyncAudioOptionsMenuState()
        {
            _soundEffectsMenuItem.IsChecked = _audioController.EffectsEnabled;
            _gameMusicMenuItem.IsChecked = _audioController.MusicEnabled;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConstrainWindowToScreen();
        }

        private void ConstrainWindowToScreen()
        {
            double workHeight = SystemParameters.WorkArea.Height - MarginFromScreen;
            double workWidth = SystemParameters.WorkArea.Width - MarginFromScreen;

            if (workHeight <= 0 || workWidth <= 0) return;

            // Board (tetris play area) cannot exceed 90% of screen height nor the default max (600px).
            double maxBoardHeightFromScreen = MaxBoardHeightFraction * workHeight;
            double maxBoardHeight = Math.Min(DefaultMaxBoardHeightPx, maxBoardHeightFromScreen);
            double scaleByBoard = maxBoardHeight / DesignBoardHeight;
            double scaleByWidth = workWidth / DesignWidth;
            double scale = Math.Min(1.0, Math.Min(scaleByBoard, scaleByWidth));

            if (scale >= 1.0) return;

            Width = DesignWidth * scale;
            Height = DesignHeight * scale;
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
                    Stroke = new SolidColorBrush(AccentGoldColor),
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
                    Stroke = new SolidColorBrush(AccentGoldColor),
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
                        DrawCell(x, y, Piece.GetColorForCellValue(grid[y, x]), isLanded: true);
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
                                piece.Color,
                                isLanded: false);
                        }
                    }
                }
            }
        }

        private void DrawCell(int x, int y, string color, bool isLanded)
        {
            var rect = new Rectangle
            {
                Width = _cellSize,
                Height = _cellSize,
                Fill = CreateCellFillBrush(color, isLanded)
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
                Stroke = new SolidColorBrush(AccentGoldColor),
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
                Fill = new SolidColorBrush(Color.FromArgb(isLanded ? (byte)30 : (byte)51, 255, 255, 255))
            };
            
            Canvas.SetLeft(highlight, x * _cellSize + 3);
            Canvas.SetTop(highlight, y * _cellSize + 3);
            
            GameCanvas.Children.Add(highlight);
        }

        private static Brush CreateCellFillBrush(string color, bool isLanded)
        {
            var baseColor = (Color)ColorConverter.ConvertFromString(color);
            if (!isLanded)
            {
                return new SolidColorBrush(baseColor);
            }

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0.0),
                EndPoint = new Point(0.5, 1.0)
            };
            gradient.GradientStops.Add(new GradientStop(baseColor, 0.0));
            gradient.GradientStops.Add(new GradientStop(DarkenColor(baseColor, 0.87), 0.72));
            gradient.GradientStops.Add(new GradientStop(DarkenColor(baseColor, 0.65), 1.0));
            return gradient;
        }

        private static Color DarkenColor(Color color, double factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)Math.Clamp((int)(color.R * factor), 0, 255),
                (byte)Math.Clamp((int)(color.G * factor), 0, 255),
                (byte)Math.Clamp((int)(color.B * factor), 0, 255));
        }

        private void UpdateScore()
        {
            ScoreText.Text = FormatScoreDisplay(_gameEngine.GetScore());
            LevelText.Text = _gameEngine.GetLevel().ToString("D2");
            LinesText.Text = _gameEngine.GetLines().ToString("D3");
        }

        private static string FormatScoreDisplay(int score)
        {
            if (score < 1000)
            {
                return score.ToString(CultureInfo.InvariantCulture);
            }

            if (score < 1_000_000)
            {
                double inThousands = score / 1000.0;
                return $"{inThousands.ToString("0.00", CultureInfo.InvariantCulture)}K";
            }

            double inMillions = score / 1_000_000.0;
            return $"{inMillions.ToString("0.00", CultureInfo.InvariantCulture)} MIL";
        }

        private void ShowGameOver()
        {
            _gameTimer.Stop();
            FinalScoreText.Text = _gameEngine.GetScore().ToString();
            GameOverOverlay.Visibility = Visibility.Visible;
            _audioController.StopBackgroundMusic();
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
                    _audioController.PlaySoftDrop();
                    break;
                case Key.Up:
                    _gameEngine.Rotate();
                    _audioController.PlayRotate();
                    break;
                case Key.Space:
                    _gameEngine.HardDrop();
                    _audioController.PlayHardDrop();
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
            _audioController.StopBackgroundMusic();
            _audioController.PlayStart();
            _audioController.StartBackgroundMusic();
            _gameTimer.Start();
            UpdatePauseButtonState();
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void ResumeBtn_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
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
                _audioController.StopBackgroundMusic();
            }
            else
            {
                PauseOverlay.Visibility = Visibility.Collapsed;
                _audioController.StartBackgroundMusic();
                _gameTimer.Start();
            }

            UpdatePauseButtonState();
        }

        private void UpdatePauseButtonState()
        {
            if (PauseBtn == null) return;

            PauseBtn.Content = _gameEngine.IsPaused ? "▶️" : "⏸️";
            PauseBtn.ToolTip = _gameEngine.IsPaused ? "继续" : "暂停";
        }

        private void ShowHelp()
        {
            // Open Tetris rules webpage
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://tetris.fandom.com/wiki/Tetris_Wiki",
                UseShellExecute = true
            });
        }

        private void AudioBtn_Click(object sender, RoutedEventArgs e)
        {
            SyncAudioOptionsMenuState();
            _audioOptionsMenu.PlacementTarget = AudioBtn;
            _audioOptionsMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            _audioOptionsMenu.IsOpen = true;
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
