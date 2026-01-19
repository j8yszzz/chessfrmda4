using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class GameController
    {
        public GameState GameState { get; private set; }
        public ChessAI AI { get; private set; }
        public bool IsPlayingAgainstAI { get; private set; }
        public Player HumanPlayer { get; private set; }

        public void StartHumanVsAI(Player human, int diff = 3)
        {
            IsPlayingAgainstAI = true;
            HumanPlayer = human;
            Player ai = human.Opponent();

            AI = new ChessAI(ai, diff);
            GameState = new GameState(Player.White, Board.Initial());
        }

        public bool TryMakeMove(Move m)
        {
            if (GameState.IsGameOver()) return false;
            if (IsPlayingAgainstAI && GameState.CurrentPlayer != HumanPlayer) return false;

            var legal = GameState.AllLegalMovesFor(GameState.CurrentPlayer);
            if (!legal.Contains(m)) return false;

            GameState.MakeMove(m);
            return true;
        }

        public Move MakeAIMove()
        {
            if (!IsPlayingAgainstAI || AI == null || GameState.IsGameOver())
                return null;

            if (GameState.CurrentPlayer != AI.AiPlayer)
                return null;

            try
            {
                Move m = AI.GetBestMove(GameState);

                if (m != null)
                {
                    GameState.MakeMove(m);
                    return m;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public IEnumerable<Move> GetLegalMovesForPiece(Position pos)
        {
            if (GameState.IsGameOver())
                return Enumerable.Empty<Move>();

            if (IsPlayingAgainstAI && GameState.CurrentPlayer != HumanPlayer)
                return Enumerable.Empty<Move>();

            return GameState.LegalMovesForPiece(pos);
        }

        public bool IsHumanTurn()
        {
            if (!IsPlayingAgainstAI) return true;
            return GameState.CurrentPlayer == HumanPlayer;
        }
    }
}