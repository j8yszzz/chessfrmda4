using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ChessLogic
{
    public class ChessAI
    {
        public readonly Player AiPlayer;
        private readonly int searchDepth;
        private Dictionary<string, int> cache = new Dictionary<string, int>();
        private Stopwatch timer;
        private const int TIME_LIMIT = 4000;

        private static readonly int[,] PawnTable = {
            {99, 99, 99, 99, 99, 99, 99, 99},
            {50, 50, 50, 50, 50, 50, 50, 50},
            {10, 10, 20, 30, 30, 20, 10, 10},
            {5, 5, 10, 27, 27, 10, 5, 5},
            {0, 0, 0, 25, 25,  0,  0,  0},
            {5, -5,-10, 0, 0,-10,-5, 5},
            {5, 10, 10,-25,-25, 10, 10, 5},
            {0, 0, 0, 0, 0, 0, 0, 0}
        };

        private static readonly int[,] KnightTable = {
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-30,  0, 20, 15, 15, 20,  0,-40},
            {-30,  5, 15, 25, 25, 15,  5,-30},
            {-30,  0, 15, 25, 25, 15,  0,-30},
            {-10,  5, 30, 15, 15, 30,  5,-10,},
            {-40,-20,  0,  5,  5,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
        };

        private static readonly int[,] BishopTable = {
            {-20,-10,-10,-10,-10,-10,-10,-20},
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5, 10, 10,  5,  0,-10 },
            { -10,  5,  5, 10, 10,  5,  5,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10, 10, 10, 10, 10, 10, 10,-10 },
            {10,  5,  0,  0,  0,  0,  5, 10 },
            { -20,-10,-10,-10,-10,-10,-10,-20 }
        };

        private static readonly int[,] RookTable = {
            {0,  0,  0,  0,  0,  0,  0,  0 },
             { 10, 10, 10, 10, 10, 10, 10,  10 },
            {-5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
            { -5,  0,  0,  5,  5,  0,  0, -5},
            { -5,  0,  0,  0,  0,  0,  0, -5},
            {-5,  0,  0,  0,  0,  0,  0, -5},
             { 0,  0,  3,  5,  5,  3,  0,  0 }
        };

        private static readonly int[,] QueenTable = {
          {-20,-10,-10, -5, -5,-10,-10,-20 },
          {-10,  0,  0,  0,  0,  0,  0,-10 },
          {   -10,  0,  5,  5,  5,  5,  0,-10 },
           {  -5,  0,  5,  5,  5,  5,  0, -5 },
            {  0,  0,  5,  5,  5,  5,  0, -5 },
            { -10,  5,  5,  5,  5,  5,  0,-10 },
            { -10,  0,  5,  0,  0,  0,  0,-10 },
            { -20,-10,-10, -5, -5,-10,-10,-20}
        };

        private static readonly int[,] KingMiddleGameTable = {
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -20,-30,-30,-40,-40,-30,-30,-20 },
            { -10,-20,-20,-20,-20,-20,-20,-10 },
            { 13, 13,  0,  0,  0,  0, 13, 13 },
            { 15, 17,  0,  0,  0,  0, 17, 15 }
        };

        private static readonly int[,] KingEndGameTable = {
            { -50,-40,-30,-20,-20,-30,-40,-50 },
            { -40,-20,-10,  0,  0,-10,-20,-30 },
            { -40,-20, 20, 25, 25, 20,-20,-40 },
            { -40,-20, 25, 35, 35, 25,-20,-40 },
            { -40,-20, 25, 35, 35, 25,-20,-40 },
            { -40,-20, 20, 25, 25, 20,-20,-40 },
            { -40,-30,  0,  0,  0,  0,-30,-40 },
            { -50,-40,-30,-30,-30,-30,-40,-50 }
        };

        public ChessAI(Player aiPlayer, int difficulty = 6)
        {
            AiPlayer = aiPlayer;
            searchDepth = difficulty;
        }

        public Move GetBestMove(GameState gameState)
        {
            cache.Clear();
            timer = Stopwatch.StartNew();

            var allMoves = gameState.AllLegalMovesFor(AiPlayer).ToList();

            if (!allMoves.Any())
                return null;

            if (allMoves.Count == 1)
                return allMoves[0];

            Move bestMove = null;
            int bestScore = int.MinValue;

            var sortedMoves = SortMoves(allMoves, gameState);

            foreach (var move in sortedMoves)
            {
                if (timer.ElapsedMilliseconds > TIME_LIMIT)
                    break;

                var newState = MakeMove(gameState, move);
                int eval = -Minimax(newState, searchDepth - 1, int.MinValue, int.MaxValue, false);

                if (eval > bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }

            return bestMove ?? allMoves[0];
        }

        private List<Move> SortMoves(List<Move> moves, GameState state)
        {
            return moves.OrderByDescending(move =>
            {
                int score = 0;

                // Captures - take valuable pieces with cheap pieces
                if (!state.Board.IsEmpty(move.ToPos))
                {
                    var target = state.Board[move.ToPos];
                    var attacker = state.Board[move.FromPos];

                    if (target != null && attacker != null)
                    {
                        score += GetPieceValue(target.Type) * 10;
                        score -= GetPieceValue(attacker.Type);
                    }
                }

                // Check moves are good
                var testState = MakeMove(state, move);
                if (state.Board.IsInCheck(AiPlayer.Opponent()))
                {
                    score += 300;
                }

                // Promotions are high priority
                if (move.Type == MoveType.PawnPromotion)
                    score += 8000;

                // Center control
                if (move.ToPos.Row >= 2 && move.ToPos.Row <= 5 &&
                    move.ToPos.Column >= 2 && move.ToPos.Column <= 5)
                    score += 50;

                return score;
            }).ToList();
        }

        private int Minimax(GameState state, int depth, int alpha, int beta, bool isMax)
        {
            string posKey = GetPosKey(state);
            if (cache.ContainsKey(posKey))
                return cache[posKey];

            if (timer.ElapsedMilliseconds > TIME_LIMIT)
                return Evaluate(state);

            if (depth == 0 || state.IsGameOver())
            {
                int eval = Evaluate(state);
                cache[posKey] = eval;
                return eval;
            }

            var player = isMax ? AiPlayer : AiPlayer.Opponent();
            var moves = state.AllLegalMovesFor(player).ToList();

            if (!moves.Any())
            {
                int eval = Evaluate(state);
                cache[posKey] = eval;
                return eval;
            }

            var sorted = SortMoves(moves, state);

            if (isMax)
            {
                int maxEval = int.MinValue;
                foreach (var move in sorted)
                {
                    var next = MakeMove(state, move);
                    int val = Minimax(next, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, val);
                    alpha = Math.Max(alpha, val);
                    if (beta <= alpha)
                        break;
                }
                cache[posKey] = maxEval;
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in sorted)
                {
                    var next = MakeMove(state, move);
                    int val = Minimax(next, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, val);
                    beta = Math.Min(beta, val);
                    if (beta <= alpha)
                        break;
                }
                cache[posKey] = minEval;
                return minEval;
            }
        }

        private string GetPosKey(GameState state)
        {
            int pieceCount = 0;
            foreach (var pos in state.Board.PiecePositions())
                pieceCount++;
            return pieceCount.ToString() + state.CurrentPlayer.ToString();
        }

        private GameState MakeMove(GameState state, Move move)
        {
            var newBoard = state.Board.Copy();
            var newState = new GameState(state.CurrentPlayer, newBoard);
            newState.MakeMove(move);
            return newState;
        }

        private int Evaluate(GameState state)
        {
            if (state.IsGameOver())
            {
                if (state.Result.Winner == AiPlayer)
                    return 999999;
                else if (state.Result.Winner == AiPlayer.Opponent())
                    return -999999;
                else
                    return 0;
            }

            int score = 0;
            int pieceCount = 0;

            foreach (var pos in state.Board.PiecePositions())
                pieceCount++;

            bool isEndgame = pieceCount <= 12;

            // Material and position
            foreach (var pos in state.Board.PiecePositions())
            {
                var piece = state.Board[pos];
                if (piece == null) continue;

                int value = Calcpiecevalue(piece, pos, isEndgame);

                if (piece.Color == AiPlayer)
                    score += value;
                else
                    score -= value;
            }

            // Push king to edge in endgame
            if (isEndgame)
            {
                score += PushKingToEdge(state);
            }

            // Penalize hanging pieces
            score -= Evaluatehangpieces(state);

            // Reward rook activity and checks
            score += EvaluateRookAct(state);

            // Center control bonus
            score += Getcentcontrol(state) * 2;

            // Mobility
            int myMoves = state.AllLegalMovesFor(AiPlayer).Count();
            int oppMoves = state.AllLegalMovesFor(AiPlayer.Opponent()).Count();
            score += (myMoves - oppMoves) * 3;

            return score;
        }

        private int PushKingToEdge(GameState state)
        {
            int score = 0;

            var oppKing = GetKingPos(state, AiPlayer.Opponent());
            if (oppKing != null)
            {
                int distFromEdge = Math.Min(oppKing.Row, Math.Min(7 - oppKing.Row,
                                   Math.Min(oppKing.Column, 7 - oppKing.Column)));
                score += (4 - distFromEdge) * 20;
            }

            return score;
        }

        private Position GetKingPos(GameState state, Player player)
        {
            foreach (var pos in state.Board.PiecePositionsFor(player))
            {
                var piece = state.Board[pos];
                if (piece != null && piece.Type == PieceType.King)
                    return pos;
            }
            return null;
        }

        private int Evaluatehangpieces(GameState state)
        {
            int hangPenalty = 0;

            foreach (var pos in state.Board.PiecePositionsFor(AiPlayer))
            {
                var piece = state.Board[pos];
                if (piece == null || piece.Type == PieceType.King) continue;

                var opponentMoves = state.AllLegalMovesFor(AiPlayer.Opponent());
                bool isUnderAttack = opponentMoves.Any(m => m.ToPos == pos);

                if (isUnderAttack)
                {
                    int penalty = GetPieceValue(piece.Type) / 2;
                    hangPenalty += penalty;
                }
            }

            return hangPenalty;
        }

        private int EvaluateRookAct(GameState state)
        {
            int score = 0;

            foreach (var pos in state.Board.PiecePositionsFor(AiPlayer))
            {
                var piece = state.Board[pos];
                if (piece == null || piece.Type != PieceType.Rook) continue;

                bool onOpenFile = true;
                for (int row = 0; row < 8; row++)
                {
                    var checkPos = new Position(row, pos.Column);
                    if (!state.Board.IsEmpty(checkPos))
                    {
                        var p = state.Board[checkPos];
                        if (p != null && p.Type == PieceType.Pawn)
                        {
                            onOpenFile = false;
                            break;
                        }
                    }
                }

                if (onOpenFile)
                    score += 30;

                if (piece.HasMoved)
                    score += 10;

                if ((AiPlayer == Player.White && pos.Row < 7) ||
                    (AiPlayer == Player.Black && pos.Row > 0))
                    score += 15;
            }

            return score;
        }

        private int Getcentcontrol(GameState state)
        {
            int score = 0;
            Position[] center = {
                new Position(3, 3), new Position(3, 4),
                new Position(4, 3), new Position(4, 4)
            };

            foreach (var pos in center)
            {
                if (!state.Board.IsEmpty(pos))
                {
                    var piece = state.Board[pos];
                    if (piece == null) continue;

                    int bonus = piece.Type == PieceType.Pawn ? 20 : 15;

                    if (piece.Color == AiPlayer)
                        score += bonus;
                    else
                        score -= bonus;
                }
            }

            return score;
        }

        private int Calcpiecevalue(Piece piece, Position pos, bool endgame)
        {
            int baseValue = GetPieceValue(piece.Type);
            int bonus = Getposbonus(piece.Type, pos, piece.Color, endgame);
            return baseValue + bonus;
        }

        private int GetPieceValue(PieceType type)
        {
            return type switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 300,
                PieceType.Bishop => 350,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 2000,
                _ => 0
            };
        }

        private int Getposbonus(PieceType type, Position pos, Player color, bool endgame)
        {
            int row = color == Player.White ? 7 - pos.Row : pos.Row;
            int col = pos.Column;

            if (type == PieceType.King && endgame)
                return KingEndGameTable[row, col];

            return type switch
            {
                PieceType.Pawn => PawnTable[row, col],
                PieceType.Knight => KnightTable[row, col],
                PieceType.Bishop => BishopTable[row, col],
                PieceType.Rook => RookTable[row, col],
                PieceType.Queen => QueenTable[row, col],
                PieceType.King => KingMiddleGameTable[row, col],
                _ => 0
            };
        }
    }
}