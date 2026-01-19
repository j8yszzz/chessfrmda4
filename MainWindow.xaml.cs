using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using System.Threading.Tasks;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private Image[,] imgs;
        private Rectangle[,] hlts;
        private Dictionary<Position, Move> cache;

        private GameController ctrl;
        private Position selected;
        private GameStats stats;
        private Player humanPlayer;
        private int difficulty;
        private int moves;

        public MainWindow(Player humanColor, int diff, GameStats gameStats)
        {
            try
            {
                InitializeComponent();

                imgs = new Image[8, 8];
                hlts = new Rectangle[8, 8];
                cache = new Dictionary<Position, Move>();

                stats = gameStats ?? new GameStats();
                humanPlayer = humanColor;
                difficulty = diff;
                selected = null;
                moves = 0;

                SetupBoard();

                ctrl = new GameController();
                ctrl.StartHumanVsAI(humanColor, diff);

                Draw();
                Updatecursor();

                if (!ctrl.IsHumanTurn())
                {
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => AITurn());
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Init error: {ex.Message}\n\n{ex.StackTrace}");
                this.Close();
            }
        }

        private void SetupBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image img = new Image();
                    imgs[r, c] = img;
                    PieceGrid.Children.Add(img);

                    Rectangle rect = new Rectangle();
                    hlts[r, c] = rect;
                    HighlightGrid.Children.Add(rect);
                }
            }
        }

        private void Draw()
        {
            Board b = ctrl.GameState.Board;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    imgs[r, c].Source = Images.GetImage(b[r, c]);
                }
            }
        }
        
        private void Updatecursor()
        {
            if (ctrl.GameState.CurrentPlayer == Player.White)
                Cursor = ChessCursors.WhiteCursor;
            else
                Cursor = ChessCursors.BlackCursor;
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (MenuContainer.Content != null) return;
                if (!ctrl.IsHumanTurn()) return;

                Point pt = e.GetPosition(BoardGrid);
                double sz = BoardGrid.ActualWidth / 8;
                int r = (int)(pt.Y / sz);
                int c = (int)(pt.X / sz);
                Position pos = new Position(r, c);

                if (selected == null)
                {
                    PickPiece(pos);
                }
                else
                {
                    movepiece(pos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Click error: " + ex.Message);
            }
        }

        private void PickPiece(Position pos)
        {
            var moves = ctrl.GetLegalMovesForPiece(pos);

            if (moves.Any())
            {
                selected = pos;
                cache.Clear();

                foreach (Move m in moves)
                {
                    cache[m.ToPos] = m;
                }

                Showgreen();
            }
        }

        private async void movepiece(Position pos)
        {
            selected = null;
            hidegreen();

            if (cache.TryGetValue(pos, out Move mv))
            {
                if (mv.Type == MoveType.PawnPromotion)
                {
                    PromotePane(mv.FromPos, mv.ToPos);
                }
                else
                {
                    await domove(mv);
                }
            }
        }

        private async void PromotePane(Position from, Position to)
        {
            imgs[to.Row, to.Column].Source = Images.GetImage(ctrl.GameState.CurrentPlayer, PieceType.Pawn);
            imgs[from.Row, from.Column].Source = null;

            PromotionMenu pm = new PromotionMenu(ctrl.GameState.CurrentPlayer);
            MenuContainer.Content = pm;

            pm.PieceSelected += async t =>
            {
                MenuContainer.Content = null;
                Move m = new PawnPromotion(from, to, t);
                await domove(m);
            };
        }

        private async Task domove(Move mv)
        {
            moves++;
            ctrl.GameState.MakeMove(mv);
            Draw();
            Updatecursor();

            // Show check warning if player is in check
            if (ctrl.GameState.Board.IsInCheck(humanPlayer))
            {
                Check checkWindow = new Check();
                checkWindow.Show();
            }

            if (ctrl.GameState.IsGameOver())
            {
                Endgame();
                return;
            }

            if (!ctrl.IsHumanTurn())
            {
                await Task.Delay(500);
                AITurn();
            }
        }

        private async void AITurn()
        {
            try
            {
                Cursor = Cursors.Wait;

                Move mv = await Task.Run(() => ctrl.MakeAIMove());

                Updatecursor();

                if (mv != null)
                {
                    moves++;
                    Draw();

                    // Show check warning if player is in check
                    if (ctrl.GameState.Board.IsInCheck(humanPlayer))
                    {
                        Check checkWindow = new Check();
                        checkWindow.Show();
                    }

                    if (ctrl.GameState.IsGameOver())
                    {
                        Endgame();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("AI error: " + ex.Message);
            }
        }

        private void Showgreen()
        {
            Color clr = Color.FromArgb(150, 125, 255, 125);
            SolidColorBrush br = new SolidColorBrush(clr);

            foreach (Position p in cache.Keys)
            {
                hlts[p.Row, p.Column].Fill = br;
            }
        }

        private void hidegreen()
        {
            foreach (Position p in cache.Keys)
            {
                hlts[p.Row, p.Column].Fill = Brushes.Transparent;
            }
        }

        private void Endgame()
        {
            stats.RecordGame(ctrl.GameState.Result.Winner, humanPlayer, difficulty, moves);

            GameOverMenu gm = new GameOverMenu(ctrl.GameState);
            MenuContainer.Content = gm;

            gm.OptionSelected += opt =>
            {
                if (opt == Option.Restart)
                {
                    MenuContainer.Content = null;
                    ResetGame();
                }
                else
                {
                    HomePage hp = new HomePage();
                    hp.Show();
                    this.Close();
                }
            };
        }

        private void ResetGame()
        {
            selected = null;
            hidegreen();
            cache.Clear();
            moves = 0;

            ctrl.StartHumanVsAI(humanPlayer, difficulty);

            Draw();
            Updatecursor();

            if (!ctrl.IsHumanTurn())
            {
                Task.Delay(500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => AITurn());
                });
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (MenuContainer.Content == null && e.Key == Key.Escape)
            {
                pausegame();
            }
        }

        private void pausegame()
        {
            PauseMenu pm = new PauseMenu();
            MenuContainer.Content = pm;

            pm.OptionSelected += opt =>
            {
                MenuContainer.Content = null;

                if (opt == Option.Continue)
                {
                    return;
                }
                else if (opt == Option.BackToHome)
                {
                    try
                    {
                        Confirm confirmWindow = new Confirm();
                        confirmWindow.Owner = this;
                        bool? dialogResult = confirmWindow.ShowDialog();

                        if (dialogResult == true && confirmWindow.ForfeitConfirmed)
                        {
                            stats.RecordGame(humanPlayer.Opponent(), humanPlayer, difficulty, moves);
                            stats.Save();

                            HomePage homePage = new HomePage();
                            homePage.Show();
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            };
        }
    }
}