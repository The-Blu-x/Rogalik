using System;

class Program
{
    static void Main()
    {
        // Hide the blinking terminal cursor for a cleaner look
        Console.CursorVisible = false;
        Console.Clear();

        // Player starting coordinates
        int playerX = 10;
        int playerY = 10;

        while (true)
        {
            // 1. Draw the player at the current position
            Console.SetCursorPosition(playerX, playerY);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("@");

            // 2. Pause and wait for an instant keystroke (true hides the key output)
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // 3. Erase the player's old position so they don't leave a trail
            Console.SetCursorPosition(playerX, playerY);
            Console.Write(" ");

            // 4. Update the coordinates based on which arrow key was pressed
            // 4. Update coordinates only if the next step is within bounds
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:    
                    if (playerY > 0) playerY--; 
                    break;
                case ConsoleKey.DownArrow:  
                    // -1 prevents stepping onto the absolute bottom edge, which can cause scrolling
                    if (playerY < Console.WindowHeight - 1) playerY++; 
                    break;
                case ConsoleKey.LeftArrow:  
                    if (playerX > 0) playerX--; 
                    break;
                case ConsoleKey.RightArrow: 
                    if (playerX < Console.WindowWidth - 1) playerX++; 
                    break;
                case ConsoleKey.Escape:     
                    return; 
            }
        }
    }
}