using System;
using System.Collections.Generic;

class Program
{
    //Temporary constants for map dimensions
    const int mapWidth = 20;
    const int mapHeight = 10;
    
    // The map is a grid of Tile objects
    static Tile[,] map = new Tile[mapWidth, mapHeight];

    static void Main()
    {
        // Hide the cursor and clear the console
        Console.CursorVisible = false;
        Console.Clear();

        InitializeMap();
        DrawMap();

        // Create the player entity
        Entity player = new(1, "Player", '@', 30, 30, 2, 2);
        
        // Any new types of enemies are added here
        List<Entity> enemies = new List<Entity>();
        enemies.Add(new Entity(2, "Goblin", 'G', 15, 15, 5, 5));
        enemies.Add(new Entity(3, "Orc", 'O', 25, 25, 8, 3));
        enemies.Add(new Entity(4, "Troll", 'T', 40, 40, 2, 8));

        // That is temporary, in the future enemies will be generated randomly and placed on the map
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

            // Read the player's input
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            // Get the tile under the player and redraw it to erase the player symbol
            Tile tileUnderPlayer = map[player.PositionX, player.PositionY];
            Console.SetCursorPosition(player.PositionX, player.PositionY);
            Console.ForegroundColor = tileUnderPlayer.Color;
            Console.Write(tileUnderPlayer.Symbol);

            int nextX = player.PositionX;
            int nextY = player.PositionY;
            switch (keyInfo.Key)
            {
                // arrow key controls (probably will be removed in the future)
                case ConsoleKey.UpArrow:    nextY--; break;
                case ConsoleKey.DownArrow:  nextY++; break;
                case ConsoleKey.LeftArrow:  nextX--; break;
                case ConsoleKey.RightArrow: nextX++; break;
                // wsad controls
                case ConsoleKey.W:          nextY--; break;
                case ConsoleKey.S:          nextY++; break;
                case ConsoleKey.A:          nextX--; break;
                case ConsoleKey.D:          nextX++; break;
                // interact (with E key)
                case ConsoleKey.E:          
                    // Get the player's current position
                    int interactX = player.PositionX;
                    int interactY = player.PositionY;
                    //interaction is made in every direction up, down, left, right at the same time by
                    //checking if the tile up, down, left, right is a interactable tile (like a door)
                    if (map[interactX, interactY - 1].Interactable) // up
                    {
                        CloseOpenDoor(interactX, interactY - 1);
                    }
                    if (map[interactX, interactY + 1].Interactable) // down
                    {
                        CloseOpenDoor(interactX, interactY + 1);
                    }
                    if (map[interactX - 1, interactY].Interactable) // left
                    {
                        CloseOpenDoor(interactX - 1, interactY);
                    }
                    if (map[interactX + 1, interactY].Interactable) // right
                    {
                        CloseOpenDoor(interactX + 1, interactY);
                    }
                    break;
                case ConsoleKey.Escape:     return; 
            }

            // bump logic
            Entity? bumpedEnemy = GetEnemyAt(nextX, nextY, enemies);

            if (bumpedEnemy != null)
            {
                // Attack the enemy!
                bumpedEnemy.CurrentHealth -= 5; // For now, the player always does 5 damage
                Console.SetCursorPosition(0, mapHeight + 1);
                
                if (bumpedEnemy.CurrentHealth > 0) // Check if the enemy is still alive
                {
                    Console.WriteLine($"{bumpedEnemy.Name} otrzymał 5 obrażeń!                 ");
                }
                else // Enemy is dead
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
            else if (map[nextX, nextY].IsWalkable) // Check if the next tile is walkable
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
                    map[x, y] = new Tile("Wall", '#', ConsoleColor.Gray, false, false); // Create walls around the edges of the map
                }
                else
                {
                    map[x, y] = new Tile("Floor", '.', ConsoleColor.DarkGray, true, false); // Create walkable floor tiles for the rest of the map
                }
            }
        }

        // Draw an internal wall down the middle (Column 10)
        for (int y = 1; y < mapHeight - 1; y++)
        {
            map[10, y] = new Tile("Wall", '#', ConsoleColor.Gray, false, false);
        }

        // Replace one of those wall tiles with a closed door
        map[10, 5] = new Tile("Closed Door", '+', ConsoleColor.DarkYellow, false, true);
    }

    static void DrawMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Draw each tile on the console
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

    static void CloseOpenDoor(int x, int y)
    {
        if (map[x, y].Name == "Open Door")
        {
            map[x, y].Name = "Closed Door";
            map[x, y].Symbol = '+';
            map[x, y].IsWalkable = false;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = map[x, y].Color;
            Console.Write('+');
        } else if (map[x, y].Name == "Closed Door")
        {
            map[x, y].Name = "Open Door";
            map[x, y].Symbol = '-';
            map[x, y].IsWalkable = true;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = map[x, y].Color;
            Console.Write('-');
        }
    }
}