using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SuperReversi
{
    class AI
    {
        public int maxDepth = 6;
        ReversiState root;


        public AI(ReversiState gameState)
        {
            root = gameState;      
        }

        public Point getBestMove()
        {
            int max = System.Int32.MinValue;
            Point bestMove = new Point();

            foreach (Point p in root.validMoves())
            {
                if (negamax(root.placePiece(p)) > max) {
                    bestMove = p;
                }
            }
            return bestMove;
        }

        private int negamax(ReversiState state, int currentDepth = 0)
        {
            if (state.isGameOver() || currentDepth == maxDepth)
            {
                return state.evaluate();
            }

            int max = System.Int32.MinValue;
            int recursedScore, currentScore;

            foreach (ReversiState move in state.expand())
            {
                recursedScore = negamax(move, currentDepth + 1);
                currentScore = -recursedScore;

                if (currentScore > max)
                {
                    max = currentScore;
                }
            }

            return max;
        }
    }
}
