using System;

class Program
{
    const int mapWidth = 20;
    const int mapHeight = 10;
    
    // The map is a grid of Tile objects
    static Tile[,] map = new Tile[mapWidth, mapHeight];

    static void Main()
    {
        Console.CursorVisible = false;
        Console.Clear();

        InitializeMap();
        DrawMap();

        Entity player = new(1, "Player", '@', 30, 30, 2, 2);
        Entity goblin = new(2, "Goblin" , 'G', 15, 15, 5, 5);

        while (true)
        {
            // Draw player
            Console.SetCursorPosition(player.PositionX, player.PositionY);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(player.Symbol);
            if(goblin.CurrentHealth>0)
            {
                Console.SetCursorPosition(goblin.PositionX, goblin.PositionY);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(goblin.Symbol);
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // Erase player by drawing the tile's specific symbol and color underneath them
            Tile tileUnderPlayer = map[player.PositionX, player.PositionY];
            Console.SetCursorPosition(player.PositionX, player.PositionY);
            Console.ForegroundColor = tileUnderPlayer.Color;
            Console.Write(tileUnderPlayer.Symbol);

            int nextX = player.PositionX;
            int nextY = player.PositionY;

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:    nextY--; break;
                case ConsoleKey.DownArrow:  nextY++; break;
                case ConsoleKey.LeftArrow:  nextX--; break;
                case ConsoleKey.RightArrow: nextX++; break;
                case ConsoleKey.Escape:     return; 
            }

            // collision detection
            if (map[nextX, nextY].IsWalkable && !(nextX == goblin.PositionX && nextY == goblin.PositionY))
            {
                player.PositionX = nextX;
                player.PositionY = nextY;
                Console.SetCursorPosition(0, mapHeight+1);
                Console.WriteLine("                                                   ");
            } 
            else if (nextX == goblin.PositionX && nextY == goblin.PositionY)
            {
              if (goblin.CurrentHealth > 0)
                {
                    goblin.CurrentHealth -= 5;
                    Console.SetCursorPosition(0, mapHeight+1);
                    System.Console.WriteLine("Goblin otrzymał 5 obrażeń");
                }
                else
                {
                    player.PositionX = nextX;
                    player.PositionY = nextY;
                    Console.SetCursorPosition(0, mapHeight+1);
                    Console.WriteLine("Goblin został zabity          ");
                    goblin.PositionX = 0;
                    goblin.PositionY = 0;
                }
            }
        }
    }

    static void InitializeMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    // Create a Wall tile (Not walkable)
                    map[x, y] = new Tile("Wall", '#', ConsoleColor.Gray, false);
                }
                else
                {
                    // Create a Floor tile (Walkable)
                    map[x, y] = new Tile("Floor", '.', ConsoleColor.DarkGray, true);
                }
            }
        }
    }

    static void DrawMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Console.SetCursorPosition(x, y);
                // The tile knows its own color and symbol now
                Console.ForegroundColor = map[x, y].Color;
                Console.Write(map[x, y].Symbol);
            }
        }
    }
}