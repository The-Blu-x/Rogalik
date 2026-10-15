using System;
using System.Collections.Generic;

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
        
        // Any new types of enemies add here
        List<Entity> enemies = new List<Entity>();
        enemies.Add(new Entity(2, "Goblin", 'G', 15, 15, 5, 5));
        enemies.Add(new Entity(3, "Orc", 'O', 25, 25, 8, 3));
        enemies.Add(new Entity(4, "Troll", 'T', 40, 40, 2, 8));

        // Drawing the enemies for the first time before the loop starts
        foreach (Entity enemy in enemies)
        {
            Console.SetCursorPosition(enemy.PositionX, enemy.PositionY);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(enemy.Symbol);
        }

        while (true)
        {
            // Drawing player
            Console.SetCursorPosition(player.PositionX, player.PositionY);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(player.Symbol);

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // Eraseing player 
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

            // bump logic
            Entity? bumpedEnemy = GetEnemyAt(nextX, nextY, enemies);

            if (bumpedEnemy != null)
            {
                // Attack the enemy!
                bumpedEnemy.CurrentHealth -= 5;
                Console.SetCursorPosition(0, mapHeight + 1);
                
                if (bumpedEnemy.CurrentHealth > 0)
                {
                    Console.WriteLine($"{bumpedEnemy.Name} otrzymał 5 obrażeń!                 ");
                }
                else
                {
                    Console.WriteLine($"{bumpedEnemy.Name} został zabity!                      ");
                    
                    // Erase the dead enemy
                    Tile tileUnderEnemy = map[bumpedEnemy.PositionX, bumpedEnemy.PositionY];
                    Console.SetCursorPosition(bumpedEnemy.PositionX, bumpedEnemy.PositionY);
                    Console.ForegroundColor = tileUnderEnemy.Color;
                    Console.Write(tileUnderEnemy.Symbol);
                    
                    // Move corpse off map
                    bumpedEnemy.PositionX = -1;
                    bumpedEnemy.PositionY = -1;
                }
            }
            else if (map[nextX, nextY].Name == "Closed Door")
            {
                // Open the door
                map[nextX, nextY].Name = "Open Door";
                map[nextX, nextY].Symbol = '-';
                map[nextX, nextY].IsWalkable = true;
                
                Console.SetCursorPosition(nextX, nextY);
                Console.ForegroundColor = map[nextX, nextY].Color;
                Console.Write('-');
            }
            else if (map[nextX, nextY].IsWalkable)
            {
                // Move the player
                player.PositionX = nextX;
                player.PositionY = nextY;
                Console.SetCursorPosition(0, mapHeight + 1);
                Console.WriteLine("                                                   ");
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
                    map[x, y] = new Tile("Wall", '#', ConsoleColor.Gray, false);
                }
                else
                {
                    map[x, y] = new Tile("Floor", '.', ConsoleColor.DarkGray, true);
                }
            }
        }

        // Draw an internal wall down the middle (Column 10)
        for (int y = 1; y < mapHeight - 1; y++)
        {
            map[10, y] = new Tile("Wall", '#', ConsoleColor.Gray, false);
        }

        // Replace one of those wall tiles with a closed door
        map[10, 5] = new Tile("Closed Door", '+', ConsoleColor.DarkYellow, false);
    }

    static void DrawMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = map[x, y].Color;
                Console.Write(map[x, y].Symbol);
            }
        }
    }

    static Entity? GetEnemyAt(int x, int y, List<Entity> enemies)
    {
        foreach (Entity enemy in enemies)
        {
            if (enemy.PositionX == x && enemy.PositionY == y && enemy.CurrentHealth > 0)
            {
                return enemy;
            }
        }
        return null; 
    }
}