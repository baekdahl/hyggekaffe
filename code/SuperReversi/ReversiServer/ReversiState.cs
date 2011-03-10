using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SuperReversi
{
    public class ReversiState
    {
        public enum Pieces { None, White, Black };

        private static Point[] directions = new Point[] {
            new Point(1,1),
            new Point(-1,-1),
            new Point(-1,1),
            new Point(1,-1),
            new Point(0,1),
            new Point(0,-1),
            new Point(1,0),
            new Point(-1,0),
        };

        public struct Piece
        {
            public Point position;
            public Pieces type;

            public Piece(Point position, Pieces type)
            {
                this.position = position;
                this.type = type;
            }
        }

        Pieces[,] game = new Pieces[8, 8];
        public Pieces turn = Pieces.White;

        public ReversiState()
        {
        }

        public ReversiState(ReversiState oldState, Pieces newPiece, Point position)
        {
            Array.Copy(oldState.game, this.game, this.game.Length);
            this.turn = (oldState.turn == Pieces.White) ? Pieces.Black : Pieces.White;
            game[position.X, position.Y] = newPiece;
            foreach (Point p in directions)
            {
                flipPieces(position, p);
            }
        }

        private Point endOfFlip(Point position, Point direction, Pieces piece = Pieces.None)
        {
            Pieces startPiece = (piece == Pieces.None) ? game[position.X, position.Y] : piece;
            Point searchPos = new Point(position.X, position.Y);
            Point nextPos = new Point(position.X + direction.X, position.Y + direction.Y);

            while (
                nextPos.X >= 0 && nextPos.X < 8 &&
                nextPos.Y >= 0 && nextPos.Y < 8 &&
                game[nextPos.X, nextPos.Y] != Pieces.None &&
                game[nextPos.X, nextPos.Y] != startPiece
            )
            {
                searchPos.X = nextPos.X;
                searchPos.Y = nextPos.Y;
                nextPos.X += direction.X;
                nextPos.Y += direction.Y;
            }

            if (nextPos.X >= 0 && nextPos.X < 8 &&
                nextPos.Y >= 0 && nextPos.Y < 8 &&
                (searchPos.X != position.X || searchPos.Y != position.Y) &&
                (game[nextPos.X, nextPos.Y] == startPiece))
            {
                return searchPos;
            }

            return new Point(-1,-1);
        }

        private void flipPieces(Point startPosition, Point direction)
        {
            Point endPosition = endOfFlip(startPosition, direction);
            Point position = new Point(startPosition.X, startPosition.Y);
            position.X += direction.X;
            position.Y += direction.Y;

            while(endPosition.X >= 0 && 
                endPosition.Y >= 0 
                )
            {
                game[position.X, position.Y] = flipPiece(game[position.X, position.Y]);
                if (position.X == endPosition.X && position.Y == endPosition.Y)
                {
                    break;
                }

                position.X += direction.X;
                position.Y += direction.Y;
            }     
        }

        Pieces flipPiece(Pieces piece)
        {
            return (piece == Pieces.Black) ? Pieces.White : Pieces.Black;
        }

        public static ReversiState initial()
        {
            ReversiState initial = new ReversiState();
            initial.game[3, 4] = Pieces.Black;
            initial.game[4, 3] = Pieces.Black;
            initial.game[3, 3] = Pieces.White;
            initial.game[4, 4] = Pieces.White;

            return initial;
        }

        public int evaluate()
        {
            int count = 0;

            foreach (Piece p in getPieces())
            {
                if (p.type == turn)
                {
                    count++;
                }
            }

            return count;
        }
        
        public List<Point> validMoves()
        {
            List<Point> list = new List<Point>();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Point point = new Point(i, j);
                    if (canPlacePiece(point, turn))
                    {
                        list.Add(point);
                    }
                }
            }

            return list;
        }

        public List<ReversiState> expand()
        {
            List<ReversiState> list = new List<ReversiState>();

            foreach (Point p in validMoves())
            {
                list.Add(this.placePiece(p));
            }

            return list;
        }

        public Boolean isGameOver()
        {
            return (validMoves().Count < 1);
        }

        public List<Piece> getPieces()
        {
            List<Piece> list = new List<Piece>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game[i,j] != Pieces.None)
                    {
                        list.Add(new Piece(new Point(i,j), game[i,j]));
                    }
                }
            }

            return list;
        }

        public ReversiState placePiece(Point position, Pieces type = Pieces.None)
        {
            type = (type == Pieces.None) ? turn : type;

            if (!canPlacePiece(position, type))
            {
                return this;
                //throw new Exception("Placing piece there violates the rules");
            }
            ReversiState newState = new ReversiState(this,type, position);
   
            return newState;
        }

        public Boolean canPlacePiece(Point position, Pieces type)
        {
            if (game[position.X, position.Y] == Pieces.None) {
                foreach (Point p in directions)
                {
                    Point endPos = endOfFlip(position, p, type);
                    if (endPos.X != -1 && endPos.Y != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string serialize()
        {
            string boardString = "";
            boardString += (int)turn;
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    boardString += ",";
                    boardString += (int)game[j, i];
                }
            }

            return boardString;
        }

        public static ReversiState unserialize(string stateString)
        {
            ReversiState state = new ReversiState();

            stateString = stateString.Substring(stateString.IndexOf("STATE:") + 6);

            string[] stateArray = stateString.Split(',');

            if (stateArray.Length != 65)
            {
                throw new Exception("Strengen overholder ikke kravet!");
            }
            state.turn = (Pieces)Enum.Parse(typeof(Pieces), stateArray[0]);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    state.game[j, i] = (Pieces)Enum.Parse(typeof(Pieces), stateArray[1+(i*8+j)]);
                }
            }

            return state;
        }
    }
}
