using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SuperReversi
{
    class ReversiState
    {
        public enum Pieces { None, White, Black };

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
        Pieces turn = Pieces.White;

        public ReversiState()
        {
        }

        public ReversiState(ReversiState oldState)
        {
            Array.Copy(oldState.game, this.game, this.game.Length);
            this.turn = (oldState.turn == Pieces.White) ? Pieces.Black : Pieces.White;
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
            if (!canPlacePiece(position, type))
            {
                throw new Exception("Placing piece there violates the rules");
            }
            ReversiState newState = new ReversiState(this);
            newState.game[position.X, position.Y] = (type == Pieces.None) ? turn : type;
            return newState;
        }

        public Boolean canPlacePiece(Point position, Pieces type)
        {
            return true;
        }
    }
}
