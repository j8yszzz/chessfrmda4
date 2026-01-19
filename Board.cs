namespace ChessLogic
{
    public class Board
    {
        private readonly Piece[,] squares = new Piece[8, 8];
        private readonly Dictionary<Player, Position> enPassantTargets = new();

        public Board()
        {
            enPassantTargets[Player.White] = null;
            enPassantTargets[Player.Black] = null;
        }

        public Piece this[int row, int col]
        {
            get => squares[row, col];
            set => squares[row, col] = value;
        }

        public Piece this[Position pos]
        {
            get => squares[pos.Row, pos.Column];
            set => squares[pos.Row, pos.Column] = value;
        }

        public Position GetPawnSkipPosition(Player player) => enPassantTargets[player];

        public void SetPawnSkipPosition(Player player, Position pos) => enPassantTargets[player] = pos;

        public static Board Initial()
        {
            var board = new Board();
            board.InitializeStartingPosition();
            return board;
        }

        private void InitializeStartingPosition()
        {
            // Black pieces
            PlaceBackRank(0, Player.Black);
            PlacePawns(1, Player.Black);

            // White pieces
            PlaceBackRank(7, Player.White);
            PlacePawns(6, Player.White);
        }

        private void PlaceBackRank(int row, Player player)
        {
            this[row, 0] = new Rook(player);
            this[row, 1] = new Knight(player);
            this[row, 2] = new Bishop(player);
            this[row, 3] = new Queen(player);
            this[row, 4] = new King(player);
            this[row, 5] = new Bishop(player);
            this[row, 6] = new Knight(player);
            this[row, 7] = new Rook(player);
        }

        private void PlacePawns(int row, Player player)
        {
            for (int col = 0; col < 8; col++)
            {
                this[row, col] = new Pawn(player);
            }
        }

        public static bool IsInside(Position pos) =>
            pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;

        public bool IsEmpty(Position pos) => this[pos] == null;

        public IEnumerable<Position> PiecePositions()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var pos = new Position(row, col);
                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<Position> PiecePositionsFor(Player player) =>
            PiecePositions().Where(pos => this[pos].Color == player);

        public bool IsInCheck(Player player) =>
            PiecePositionsFor(player.Opponent()).Any(pos => this[pos].CanCaptureOpponentKing(pos, this));

        public Board Copy()
        {
            var copy = new Board();
            foreach (var pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }
            return copy;
        }

        public Counting CountPieces()
        {
            var counting = new Counting();
            foreach (var pos in PiecePositions())
            {
                var piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }
            return counting;
        }

        public bool InsufficientMaterial()
        {
            var count = CountPieces();
            return IsKingVsKing(count) || IsKingBishopVsKing(count) ||
                   IsKingKnightVsKing(count) || IsKingBishopVsKingBishop(count);
        }

        private static bool IsKingVsKing(Counting count) => count.TotalCount == 2;

        private static bool IsKingBishopVsKing(Counting count) =>
            count.TotalCount == 3 && (count.White(PieceType.Bishop) == 1 || count.Black(PieceType.Bishop) == 1);

        private static bool IsKingKnightVsKing(Counting count) =>
            count.TotalCount == 3 && (count.White(PieceType.Knight) == 1 || count.Black(PieceType.Knight) == 1);

        private bool IsKingBishopVsKingBishop(Counting count)
        {
            if (count.TotalCount != 4) return false;
            if (count.White(PieceType.Bishop) != 1 || count.Black(PieceType.Bishop) != 1) return false;

            var whiteBishop = FindPiece(Player.White, PieceType.Bishop);
            var blackBishop = FindPiece(Player.Black, PieceType.Bishop);

            return whiteBishop.SquareColor() == blackBishop.SquareColor();
        }

        private Position FindPiece(Player color, PieceType type) =>
            PiecePositionsFor(color).First(pos => this[pos].Type == type);

        private bool HasUnmovedKingAndRook(Position kingPos, Position rookPos)
        {
            if (IsEmpty(kingPos) || IsEmpty(rookPos)) return false;

            var king = this[kingPos];
            var rook = this[rookPos];

            return king.Type == PieceType.King && rook.Type == PieceType.Rook &&
                   !king.HasMoved && !rook.HasMoved;
        }

        public bool CastleRightKS(Player player) => player switch
        {
            Player.White => HasUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
            Player.Black => HasUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
            _ => false
        };

        public bool CastleRightQS(Player player) => player switch
        {
            Player.White => HasUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
            Player.Black => HasUnmovedKingAndRook(new Position(0, 4), new Position(0, 0)),
            _ => false
        };

        private bool ExistsLegalEnPassantFrom(Player player, Position[] candidatePositions, Position target)
        {
            foreach (var pos in candidatePositions.Where(IsInside))
            {
                var piece = this[pos];
                if (piece?.Color == player && piece.Type == PieceType.Pawn)
                {
                    var move = new EnPassant(pos, target);
                    if (move.IsLegal(this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanCaptureEnPassant(Player player)
        {
            var targetSquare = GetPawnSkipPosition(player.Opponent());
            if (targetSquare == null) return false;

            var attackingSquares = player switch
            {
                Player.White => new[] { targetSquare + Direction.SouthWest, targetSquare + Direction.SouthEast },
                Player.Black => new[] { targetSquare + Direction.NorthWest, targetSquare + Direction.NorthEast },
                _ => Array.Empty<Position>()
            };

            return ExistsLegalEnPassantFrom(player, attackingSquares, targetSquare);
        }
    }
}