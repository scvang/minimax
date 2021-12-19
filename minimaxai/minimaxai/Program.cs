using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/**
 * References used:
 * https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/
 * https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-4-alpha-beta-pruning/
 * https://www.youtube.com/watch?v=trKjYdBASyQ
 * https://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-of-an-object-in-net
 * 
 * Game of Reversi.
 * Rules:
 * Black always moves first.
 * Can capture if piece is adjacent or jumping over as long as same piece is not blocking it.
 * If there are no moves for the player to make, their turn is skipped.
 * The player or opponents wins when they have no more pieces left or the board is full and they have the most pieces at the end.
 */

namespace minimaxai
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();

            Console.WriteLine("Choose your difficulty (1 to 10):");
            String difficulty = Console.ReadLine();

            int depth = checkDifficulty(difficulty);

            Console.WriteLine("Go first (y/n)?");
            Boolean hasFirstMove;
            hasFirstMove = getTurn(Console.ReadLine());

            if (!hasFirstMove)
            {
                board.display();
                board.player = 'X'; // X represents white
                board.opponent = 'O';
                // AI makes moves first.
                Console.WriteLine("It is " + board.opponent + "'s turn. Choose a move (x,y):");
                decision(board, board.opponent, depth);
            }
            else
            {
                board.player = 'O'; // O represents black
                board.opponent = 'X';
            }

            while (!board.isGameOver())
            {
                // Display current board state
                board.display();

                Console.WriteLine("It is " + board.player + "'s turn. Choose a move (x,y):");
                String input = Console.ReadLine();

                // Coordinates for moving
                int[] move = getMove(input);
                move = board.validateMove(move);

                // After move valids, player makes the move
                board.makeMove(move,board.player,false);
                Console.WriteLine("You moved to.. x:" + move[0] + ",y:" +move[1]);

                // Update the score
                board.updateScore();

                board.display();
                // Opponent makes a move
                Console.WriteLine("It is " + board.opponent + "'s turn. Choose a move (x,y):");
                decision(board,board.opponent,depth);
            }

            board.display();
            
            Console.WriteLine("Game over!");
            Console.WriteLine("Winning player: " + board.hasWon());
        }

        public static int checkDifficulty(String difficulty)
        {
            int diff = 0;
            // Check if numeric.
            for(int i = 0; i < difficulty.Length; i++)
            {
                if(!Char.IsDigit(difficulty[i]))
                {
                    Console.WriteLine("Needs to be an numeric value, try again.");
                    difficulty = Console.ReadLine();
                    i = -1;
                }
            }
            // Check if in range.
            for (int i = 0; i < difficulty.Length; i++)
            {
                int num = Int32.Parse(difficulty);
                if (num < 0 || num > 10)
                {
                    Console.WriteLine("Select a difficulty between 1 to 10.");
                    difficulty = Console.ReadLine();
                    i = -1;
                }
            }
            diff = Int32.Parse(difficulty);
            return diff;
        }

        public static int minimax(Board b, char player, int depth, bool isMax,int alpha, int beta)
        {
            char opponent;
            if (player == 'O') opponent = 'X';
            else opponent = 'O';

            // get the max score
            if (depth <= 0 || b.isGameOver())
            {
                int score = b.outCome(player);
                return score;
            }

            if(isMax)
            {
                int bestScore = Int32.MinValue;
                for (int i = 1; i <= 8; i++)
                {
                    int[] move = new int[2];
                    for (int j = 1; j <= 8; j++)
                    {
                        move[0] = i;
                        move[1] = j;
                        Board board = new Board(b);
                        if (board.makeMove(move, player, true))
                        {
                            board.updateScore();
                            int currentScore = minimax(board, player, depth - 1, false, alpha, beta);
                            bestScore = Math.Max(bestScore, currentScore);
                            alpha = Math.Max(alpha, bestScore);
                        }
                        if (beta <= alpha) break;
                    }
                    
                }
                return bestScore;
            }
            else
            {
                int bestScore = Int32.MaxValue;
                for (int i = 1; i <= 8; i++)
                {
                    int[] move = new int[2];
                    for (int j = 1; j <= 8; j++)
                    {
                        move[0] = i;
                        move[1] = j;
                        Board board = new Board(b);
                        if (board.makeMove(move, opponent, true))
                        {
                            board.updateScore();
                            int currentScore = minimax(board, opponent, depth - 1, true, alpha, beta);
                            bestScore = Math.Min(bestScore, currentScore);
                            beta = Math.Min(beta, bestScore);
                        }
                        if (beta <= alpha) break;
                    }
                    

                }
                return bestScore;
            }
        }

        public static void decision(Board b, char player, int depth)
        {
            int bestScore = Int32.MinValue;
            int[] move = new int[2];
            int[] bestMove = new int[2];
            for(int i = 1; i <= 8; i++)
            {
                for(int j = 1; j <= 8; j++)
                {
                    move[0] = i;
                    move[1] = j;
                    Board board = new Board(b);
                    if (board.makeMove(move, player, true)){
                        board.updateScore();
                        int currentScore = minimax(board, player, depth - 1, true, Int32.MinValue, Int32.MaxValue);

                        if (currentScore > bestScore)
                        {
                            bestScore = currentScore;
                            bestMove[0] = i;
                            bestMove[1] = j;
                        }
                    }
                }
            }
            int posx = bestMove[0];
            int posy = bestMove[1];

            b.makeMove(bestMove, player, true);
            b.updateScore();

            Console.WriteLine("AI moved to.. x:" + posx + ",y:"+ posy);
        }

        public static bool getTurn(String input)
        {
            input = input.ToLower();
            if (input.Equals("y")) return true;
            else return false;
        }
        public static int[] getMove(String input)
        {
            int[] move = new int[2];

            int moveIndex = 0;
            for(int i = 0; i < input.Length; ++i)
            {
                if(Char.IsDigit(input[i]))
                {
                    if (moveIndex >= 2) break;
                    move[moveIndex++] = input[i] - '0';
                }
            }

            return move;
        }

        [Serializable]
        internal class Board
        {
            int scoreWhite;
            int scoreBlack;
            public char[,] grid = new char[8, 8];
            public char player;
            public char opponent;
            public List<int[]> coords = new List<int[]>();

            public Board()
            {
                // Initialize the board grid
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        grid[i, j] = '.';
                    }
                }
                // place 4 pieces in the middle.
                this.grid[3, 3] = 'X';
                this.grid[3, 4] = 'O';
                this.grid[4, 3] = 'O';
                this.grid[4, 4] = 'X';


                // testing stuff
                //this.grid[2, 4] = 'O';
                //this.grid[3, 5] = 'X';
                //this.grid[4, 5] = 'X';
                //this.grid[5, 5] = 'O';
                //this.grid[1, 6] = 'X';

                this.scoreWhite = 2;
                this.scoreBlack = 2;
            }

            // Copy constructor
            public Board(Board b)
            {
                this.scoreWhite = b.scoreWhite;
                this.scoreBlack = b.scoreBlack;
                //this.grid = b.grid;
                for(int i = 0; i < 8; i++)
                {
                    for(int j = 0; j < 8; j++)
                    {
                        this.grid[i, j] = b.grid[i, j];
                    }
                }
                this.player = b.player;
                this.opponent = b.opponent;
                //this.coords = b.coords;
                if (b.coords.Count != 0)
                {
                    for (int i = 0; i < b.coords.Count; i++)
                    {
                        this.coords.Add(b.coords[i]);
                    }
                }
        }

            public int outCome(char player)
            {
                int[] score = this.getScore();
                int blackScore = score[0] - score[1];
                int whiteScore = score[1] - score[0];

                if (player == 'O')
                {
                    return blackScore;
                }
                else
                {
                    return whiteScore;
                }
            }

            public char hasWon()
            {
                if(this.isGameOver())
                {

                    this.updateScore();
                    if (this.scoreBlack > this.scoreWhite)
                    {
                        return 'O';
                    }
                    if (this.scoreWhite > this.scoreBlack)
                    {
                        return 'X';
                    }
                }
                return 'T';
            }
            public Boolean isGameOver()
            {
                int emptySlots = 0;

                // If an opponent or player has no more remaining pieces
                for (int i = 0; i < 8; i++)
                {
                    for(int j = 0; j < 8; j++)
                    {
                        if(grid[i,j] == '.' && (scoreBlack <= 0 || scoreWhite <= 0))
                        {
                            return true;
                        }
                        // Count empty slots
                        if(grid[i,j] == '.')
                        {
                            emptySlots++;
                        }

                    }
                }

                // If the board is full
                if(emptySlots == 0)
                {
                    return true;
                }

                return false;
            }

            public void capturePiece(char currentPlayer,int[] move)
            {
                // Capture all pieces from position (a,b) to (b,c)
                int count = coords.Count;
                int i = 0;
                while (count-- > 0)
                {
                    int minX = Math.Min(move[0] - 1, this.coords[i][0] - 1);
                    int maxX = Math.Max(move[0] - 1, this.coords[i][0] - 1);
                    int minY = Math.Min(move[1] - 1, this.coords[i][1] - 1);
                    int maxY = Math.Max(move[1] - 1, this.coords[i][1] - 1);

                    // Current move position
                    int x1 = move[0] - 1;
                    int y1 = move[1] - 1;
                    int x2 = coords[i][0] - 1;
                    int y2 = coords[i][1] - 1;
                    int m = 0;

                    // Find diagonal
                    if (x2 != x1)
                    {
                        m = (y2 - y1) / (x2 - x1);
                        int x3 = minX;
                        int y3 = 0;

                        for (int j = minX; j <= maxX; ++j)
                        {
                            y3 = m * (j - x1) + y1;

                            // Capture the piece
                            this.grid[j, y3] = currentPlayer;
                        }
                    }

                    // horizontal
                    if (minX == maxX)
                    {
                        
                        for (int col = minY; col <= maxY; ++col)
                        {
                            this.grid[minX, col] = currentPlayer;
                        }
                    }

                    // vertical
                    if (minY == maxY)
                    {
                        
                        for (int row = minX; row <= maxX; ++row)
                        {
                            this.grid[row, minY] = currentPlayer;
                        }
                    }
                        ++i;
                 }
            }

            // Recursively search for end pieces and get their coordinates
            public void searchPiece(char currentPlayer,int row, int col,int[] move, string direction)
            {
                // current player is 'O' for example
                char opponent;
                if (currentPlayer == 'O') opponent = 'X';
                else opponent = 'O';

                int x = move[0] - 1;
                int y = move[1] - 1;

                // If gone out of bounds
                if(row < 0 || row > 7 || col < 0 || col > 7)
                {
                    return;
                }

                if(this.grid[row,col] == currentPlayer)
                {

                    //Console.WriteLine("Found end at: " + "x:"+(row+1) + ",y:"+(col+1));
                    coords.Add(new int[] {(row+1),(col+1)});
                    return;
                }

                // search for ending piece
                // Keep searching in only 1 direction
                // ww
                if ((direction.Equals("any") || direction.Equals("ww")) && 
                    y - 1 >= 0 && y - 1 <= 7 && 
                    this.grid[x, y-1] == opponent) 
                    searchPiece(currentPlayer, row, col - 1, move,"ww");

                // nw
                if ((direction.Equals("any") || direction.Equals("nw")) && 
                    x - 1 >= 0 && x - 1 <= 7 &&
                    y - 1 >= 0 && y - 1 <= 7 && 
                    this.grid[x- 1, y - 1] == opponent) 
                    searchPiece(currentPlayer, row - 1, col - 1, move,"nw");
                // nn
                if ((direction.Equals("any") || direction.Equals("nn")) && 
                    x - 1 >= 0 && x - 1 <= 7 && 
                    this.grid[x - 1,y] == opponent) 
                    searchPiece(currentPlayer, row-1, col, move,"nn");
                // ne
                if ((direction.Equals("any") || direction.Equals("ne")) && 
                    x - 1 >= 0 && x - 1 <= 7 && 
                    y + 1 >= 0 && y + 1 <= 7 && 
                    this.grid[x- 1, y + 1] == opponent) 
                    searchPiece(currentPlayer, row-1, col+1, move,"ne");
                // ee
                if ((direction.Equals("any") || direction.Equals("ee")) && 
                    y + 1 >= 0 && y + 1 <= 7 &&
                    this.grid[x, y+ 1 ] == opponent) 
                    searchPiece(currentPlayer, row, col +1, move,"ee");
                // se
                if ((direction.Equals("any") || direction.Equals("se")) && 
                    x + 1 >= 0 && x + 1 <= 7 && 
                    y + 1 >= 0 && y + 1 <= 7 && 
                    this.grid[x+ 1, y+ 1] == opponent) 
                    searchPiece(currentPlayer, row+1, col + 1, move,"se");
                // ss
                if ((direction.Equals("any") || direction.Equals("ss")) && 
                    x + 1 >= 0 && x + 1 <= 7 && 
                    this.grid[x + 1, y] == opponent)
                    searchPiece(currentPlayer, row+1, col, move,"ss");
                // sw
                if ((direction.Equals("any") || direction.Equals("sw")) && 
                    x + 1 >= 0 && x + 1 <= 7 && 
                    y - 1 >= 0 && y - 1 <= 7 && 
                    this.grid[x + 1, y - 1] == opponent) 
                    searchPiece(currentPlayer, row+1, col - 1, move,"sw");
            }

            /**
             * Returns true if a valid move was made.
             */
            public bool makeMove(int[] move, char currentPlayer, bool ai)
            {
                while(isValidCoords(move))
                {
                    // If the spot is empty
                    if (this.grid[move[0] - 1, move[1] - 1] == '.')
                    {
                        this.coords.Clear();
                        // Search for ending piece
                        this.searchPiece(currentPlayer,move[0]-1,move[1]-1,move,"any");
                        
                        // Check if the player is making the move and is valid.
                        if (this.coords.Count == 0 && !this.isGameOver())
                        {
                            if (ai) return false ;
                            this.display();
                            Console.WriteLine("Invalid move. Choose again (x,y):");
                            String input = Console.ReadLine();
                            move = getMove(input);
                            move = validateMove(move);
                        }
                        // Make the move.
                        else
                        {
                            this.capturePiece(currentPlayer,move);
                            this.grid[move[0] - 1, move[1] - 1] = currentPlayer;
                            return true;
                        }
                    }
                    // Choose again if the move wasn't valid.
                    else
                    {
                        if (ai) return false;
                        this.display();
                        Console.WriteLine("There is already a piece there. Choose again (x,y):");
                        String input = Console.ReadLine();
                        move = getMove(input);
                        move = validateMove(move);
                    }
                }
                return false;
            }
            public int[] validateMove(int[] move)
            {
                while (!isValidCoords(move))
                {
                    Console.WriteLine("The move was out of bounds, try again (x,y).");
                    String input = Console.ReadLine();

                    move = new int[2];

                    int moveIndex = 0;
                    for (int i = 0; i < input.Length; ++i)
                    {
                        if (Char.IsDigit(input[i]))
                        {
                            if (moveIndex >= 2) break;
                            move[moveIndex++] = input[i] - '0';
                        }
                    }
                }
                return move;
            }
            public bool isValidCoords(int[] move)
            {
                for (int i = 0; i < 2; ++i)
                {
                    // Check if the move is inbounds
                    if (move[i] > 8 || move[i] < 1)
                    {
                        return false;
                    }
                }

                return true;
            }
            public void display()
            {
                Console.WriteLine("Current Board:");
                Console.WriteLine("Score " + this.scoreBlack + " vs " + this.scoreWhite);
                Console.WriteLine("   1 2 3 4 5 6 7 8");
                Console.WriteLine("  +-+-+-+-+-+-+-+-+");
                for(int i = 0; i < 8; i++)
                {
                    Console.Write((i+1)+" |");
                    for(int j = 0; j < 8; j++)
                    {
                        Console.Write(grid[i,j] + "|");
                    }
                    Console.WriteLine("");
                    Console.WriteLine("  +-+-+-+-+-+-+-+-+");
                }
            }

            public int[] getScore()
            {
                this.updateScore();
                int[] score = new int[2];
                score[0] = this.scoreBlack;
                score[1] = this.scoreWhite;
                return score;
            }

            public void updateScore()
            {
                int scoreB = 0;
                int scoreW = 0;

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if(this.grid[i,j] == 'O')
                        {
                            scoreB++;
                        }
                        if(this.grid[i,j] == 'X')
                        {
                            scoreW++;
                        }
                    }
                }
                this.scoreBlack = scoreB;
                this.scoreWhite = scoreW;
            }
        }

    }
}