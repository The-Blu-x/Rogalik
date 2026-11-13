namespace NullDescent;

internal static class Program
{
    // Temporary constants for map dimensions
    private const int MapWidth = 20;
    private const int MapHeight = 10;
    private static readonly Random Rnd = new Random();

    private static void Main()
    {
        // Hide the cursor and clear the console
        Console.CursorVisible = false;
        Console.Clear();

        // Initializing Map using new Map class
        Map gameMap = new Map(MapWidth, MapHeight);
        BuildDefaultLayout(gameMap);

        // Loading templates from CSV files
        List<Entity> entityTemplates = LoadEntityTemplates("entities.csv");
        List<Item> itemTemplates = LoadItemTemplates("items.csv");
        List<Tile> tileTemplates = LoadTileTemplates("tiles.csv");

        // Createing player and spawning initial random entities
        Entity player = new(1, "Player", '@', 30, 30, 2, 2, false, EntityBehavior.Friendly);
        SpawnRandomEntities(4, entityTemplates, gameMap, player);

        // Initial full draw of the map
        DrawFullMap(gameMap);

        while (true)
        {
            // Draw items, entities, and player
            DrawDynamicObjects(gameMap, player);

            // Read input
            var keyInfo = Console.ReadKey(true);

            // Check '=' by KeyChar so it works regardless of keyboard layout
            if (keyInfo.KeyChar == '=')
            {
                OpenBuilderPrompt(gameMap, player, entityTemplates, itemTemplates, tileTemplates);
                DrawFullMap(gameMap);
                continue;
            }

            // Erase player's old position with the underlying tile
            var tileUnderPlayer = gameMap.GetTile(player.PositionX, player.PositionY);
            Console.SetCursorPosition(player.PositionX, player.PositionY);
            Console.ForegroundColor = tileUnderPlayer.Color;
            Console.Write(tileUnderPlayer.Symbol);

            var nextX = player.PositionX;
            var nextY = player.PositionY;

            // Better writen switch than last one
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W: nextY--; break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S: nextY++; break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A: nextX--; break;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D: nextX++; break;

                case ConsoleKey.E:
                    InteractAdjacent(gameMap, player.PositionX, player.PositionY);
                    break;

                case ConsoleKey.Escape:
                    return;
            }

            // Bump logic using gameMap.GetEntityAt
            var bumpedEntity = gameMap.GetEntityAt(nextX, nextY);

            if (bumpedEntity != null)
            {
                if (bumpedEntity.Behavior == EntityBehavior.Friendly)
                {
                    PrintStatus($"{bumpedEntity.Name}: \"Witaj wędrowcze!\"");
                }
                else
                {
                    // If Neutral or PassiveAggressive gets hit, it becomes Hostile
                    if (bumpedEntity.Behavior == EntityBehavior.PassiveAggressive)
                        bumpedEntity.Behavior = EntityBehavior.Hostile;

                    bumpedEntity.CurrentHealth -= 5;

                    if (bumpedEntity.CurrentHealth > 0)
                    {
                        PrintStatus($"{bumpedEntity.Name} otrzymał 5 obrażeń!");
                    }
                    else
                    {
                        PrintStatus($"{bumpedEntity.Name} został zabity!");

                        // Erase dead entity and remove from map
                        var tileUnderEntity = gameMap.GetTile(bumpedEntity.PositionX, bumpedEntity.PositionY);
                        Console.SetCursorPosition(bumpedEntity.PositionX, bumpedEntity.PositionY);
                        Console.ForegroundColor = tileUnderEntity.Color;
                        Console.Write(tileUnderEntity.Symbol);

                        gameMap.RemoveEntity(bumpedEntity);
                    }
                }
            }
            else if (IsWithinBounds(gameMap, nextX, nextY) && gameMap.GetTile(nextX, nextY).IsWalkable)
            {
                player.PositionX = nextX;
                player.PositionY = nextY;
                PrintStatus(""); // Clear status line
            }
        }
    }

    // --- BUILDER / DEBUG COMMAND MENU ('=') ---

    private static void OpenBuilderPrompt(
        Map gameMap,
        Entity player,
        List<Entity> entityTemplates,
        List<Item> itemTemplates,
        List<Tile> tileTemplates)
    {
        int menuX = MapWidth + 5;
        int menuWidth = 50;
        Console.CursorVisible = false;

        // Wipes the right side of the screen clean
        void ClearRightPanel()
        {
            for (int y = 0; y < 20; y++)
            {
                Console.SetCursorPosition(menuX, y);
                Console.Write(new string(' ', menuWidth));
            }
        }

        // Draws a scrollable list menu
        int RunMenu(string title, string[] options)
        {
            int selectedIndex = 0;
            while (true)
            {
                ClearRightPanel();
                Console.SetCursorPosition(menuX, 0);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(title);

                for (int i = 0; i < options.Length; i++)
                {
                    Console.SetCursorPosition(menuX, i + 2);
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    
                    string text = options[i];
                    if (text.Length > menuWidth) text = text.Substring(0, menuWidth);
                    Console.Write(text.PadRight(menuWidth));
                }
                Console.ResetColor();

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length) selectedIndex = 0;
                }
                else if (key == ConsoleKey.Enter)
                {
                    ClearRightPanel();
                    return selectedIndex;
                }
                else if (key == ConsoleKey.Escape)
                {
                    ClearRightPanel();
                    return -1;
                }
            }
        }

        // Visual targeting cursor on the map
        bool RunTargeting(out int targetX, out int targetY)
        {
            int cursorX = player.PositionX;
            int cursorY = player.PositionY;

            ClearRightPanel();
            Console.SetCursorPosition(menuX, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("TARGETING MODE");
            Console.SetCursorPosition(menuX, 2);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Use Arrows to move cursor.");
            Console.SetCursorPosition(menuX, 3);
            Console.Write("Press ENTER to place.");
            Console.SetCursorPosition(menuX, 4);
            Console.Write("Press ESC to cancel.");

            while (true)
            {
                // Redraw map and objects so cursor moves cleanly
                DrawFullMap(gameMap);
                DrawDynamicObjects(gameMap, player);

                // Draw targeting cursor (White block with black X)
                Console.SetCursorPosition(cursorX, cursorY);
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write('X');
                Console.ResetColor();

                var key = Console.ReadKey(true).Key;
                int nextX = cursorX;
                int nextY = cursorY;

                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W) nextY--;
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S) nextY++;
                else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A) nextX--;
                else if (key == ConsoleKey.RightArrow || key == ConsoleKey.D) nextX++;
                else if (key == ConsoleKey.Enter)
                {
                    targetX = cursorX;
                    targetY = cursorY;
                    ClearRightPanel();
                    
                    // Final map cleanup
                    DrawFullMap(gameMap);
                    DrawDynamicObjects(gameMap, player);
                    return true;
                }
                else if (key == ConsoleKey.Escape)
                {
                    targetX = -1;
                    targetY = -1;
                    ClearRightPanel();
                    DrawFullMap(gameMap);
                    DrawDynamicObjects(gameMap, player);
                    return false;
                }

                if (IsWithinBounds(gameMap, nextX, nextY))
                {
                    cursorX = nextX;
                    cursorY = nextY;
                }
            }
        }

        // --- Main Builder Flow ---

        string[] mainOptions = { "Place Entity", "Place Item", "Place Tile", "Delete Object", "View Report", "Cancel" };
        int mainChoice = RunMenu("BUILDER MENU", mainOptions);

        if (mainChoice == 0) // Entity
        {
            if (entityTemplates.Count == 0) { PrintStatus("No entities loaded!"); return; }
            string[] opts = new string[entityTemplates.Count];
            for (int i = 0; i < entityTemplates.Count; i++) opts[i] = $"{entityTemplates[i].Name} ({entityTemplates[i].Symbol})";
            
            int selected = RunMenu("SELECT ENTITY", opts);
            if (selected != -1 && RunTargeting(out int tx, out int ty))
            {
                if (gameMap.CanPlaceEntity(tx, ty) && !(tx == player.PositionX && ty == player.PositionY))
                {
                    var t = entityTemplates[selected];
                    gameMap.PlaceEntity(new Entity(t.Id, t.Name, t.Symbol, t.DefaultHealth, t.CurrentHealth, tx, ty, t.IsWalkable, t.Behavior), tx, ty);
                    PrintStatus($"Placed {t.Name}.");
                }
                else PrintStatus("Cannot place entity there!");
            }
        }
        else if (mainChoice == 1) // Item
        {
            if (itemTemplates.Count == 0) { PrintStatus("No items loaded!"); return; }
            string[] opts = new string[itemTemplates.Count];
            for (int i = 0; i < itemTemplates.Count; i++) opts[i] = $"{itemTemplates[i].Name} ({itemTemplates[i].Symbol})";
            
            int selected = RunMenu("SELECT ITEM", opts);
            if (selected != -1 && RunTargeting(out int tx, out int ty))
            {
                if (gameMap.CanPlaceItem(tx, ty))
                {
                    var t = itemTemplates[selected];
                    gameMap.PlaceItem(new Item(t.Id, t.Name, t.Symbol, tx, ty) { Description = t.Description, Stats = t.Stats }, tx, ty);
                    PrintStatus($"Placed {t.Name}.");
                }
                else PrintStatus("Cannot place item there!");
            }
        }
        else if (mainChoice == 2) // Tile
        {
            if (tileTemplates.Count == 0) { PrintStatus("No tiles loaded!"); return; }
            string[] opts = new string[tileTemplates.Count];
            for (int i = 0; i < tileTemplates.Count; i++) opts[i] = $"{tileTemplates[i].Name} ({tileTemplates[i].Symbol})";
            
            int selected = RunMenu("SELECT TILE", opts);
            if (selected != -1 && RunTargeting(out int tx, out int ty))
            {
                var t = tileTemplates[selected];
                if (!t.IsWalkable && ((tx == player.PositionX && ty == player.PositionY) || gameMap.GetEntityAt(tx, ty) != null))
                {
                    PrintStatus("Cannot place solid tile on top of an entity!");
                }
                else
                {
                    gameMap.SetTile(tx, ty, new Tile(t.Name, t.Symbol, t.Color, t.IsOcupied, t.IsWalkable, t.Interactable));
                    PrintStatus($"Changed tile to {t.Name}.");
                }
            }
        }
        else if (mainChoice == 3) // Delete Object
        {
            if (RunTargeting(out int tx, out int ty))
            {
                var entityToDelete = gameMap.GetEntityAt(tx, ty);
                var itemToDelete = gameMap.GetItemAt(tx, ty);
                var currentTile = gameMap.GetTile(tx, ty);

                // Hierarchy of deletion: Entity -> Item -> Tile
                if (entityToDelete != null)
                {
                    gameMap.RemoveEntity(entityToDelete);
                    PrintStatus($"Deleted entity: {entityToDelete.Name}.");
                }
                else if (itemToDelete != null)
                {
                    gameMap.RemoveItem(itemToDelete);
                    PrintStatus($"Deleted item: {itemToDelete.Name}.");
                }
                else if (currentTile.Name != "Floor")
                {
                    // Reset to default floor
                    gameMap.SetTile(tx, ty, new Tile("Floor", '.', ConsoleColor.DarkGray, false, true, false));
                    PrintStatus("Reset tile to Floor.");
                }
                else
                {
                    PrintStatus("Nothing to delete here.");
                }
            }
        }
        else if (mainChoice == 4) // Report
        {
            Console.Clear();
            Console.WriteLine(gameMap.GenerateReport());
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            Console.Clear();
            DrawFullMap(gameMap); 
        }
    }

    // --- MAP SETUP & RENDERING ---

    private static void BuildDefaultLayout(Map gameMap)
    {
        for (int x = 0; x < gameMap.Width; x++)
        {
            for (int y = 0; y < gameMap.Height; y++)
            {
                if (x == 0 || x == gameMap.Width - 1 || y == 0 || y == gameMap.Height - 1)
                {
                    gameMap.SetTile(x, y, new Tile("Wall", '#', ConsoleColor.Gray, true, false, false));
                }
            }
        }

        for (int y = 1; y < gameMap.Height - 1; y++)
        {
            gameMap.SetTile(10, y, new Tile("Wall", '#', ConsoleColor.Gray, true, false, false));
        }

        gameMap.SetTile(10, 5, new Tile("Closed Door", '+', ConsoleColor.DarkYellow, true, false, true));
    }

    private static void DrawFullMap(Map gameMap)
    {
        for (int y = 0; y < gameMap.Height; y++)
        {
            for (int x = 0; x < gameMap.Width; x++)
            {
                var tile = gameMap.GetTile(x, y);
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = tile.Color;
                Console.Write(tile.Symbol);
            }
        }
    }

    private static void DrawDynamicObjects(Map gameMap, Entity player)
    {
        foreach (var item in gameMap.Items)
        {
            Console.SetCursorPosition(item.PositionX, item.PositionY);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(item.Symbol);
        }

        foreach (var entity in gameMap.Entities)
        {
            if (entity.CurrentHealth <= 0) continue;
            Console.SetCursorPosition(entity.PositionX, entity.PositionY);
            Console.ForegroundColor = entity.Behavior switch
            {
                EntityBehavior.Friendly => ConsoleColor.Blue,
                EntityBehavior.Neutral => ConsoleColor.Yellow,
                EntityBehavior.PassiveAggressive => ConsoleColor.Magenta,
                _ => ConsoleColor.Red
            };
            Console.Write(entity.Symbol);
        }

        Console.SetCursorPosition(player.PositionX, player.PositionY);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(player.Symbol);
    }

    private static void PrintStatus(string message)
    {
        Console.SetCursorPosition(0, MapHeight + 1);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(message.PadRight(65));
    }

    private static bool IsWithinBounds(Map gameMap, int x, int y) =>
        x >= 0 && x < gameMap.Width && y >= 0 && y < gameMap.Height;

    // --- INTERACTIONS & SPAWNING ---

    private static void InteractAdjacent(Map gameMap, int x, int y)
    {
        int[] dx = [0, 0, -1, 1];
        int[] dy = [-1, 1, 0, 0];

        for (int i = 0; i < 4; i++)
        {
            int tx = x + dx[i];
            int ty = y + dy[i];
            if (IsWithinBounds(gameMap, tx, ty) && gameMap.GetTile(tx, ty).Interactable)
            {
                CloseOpenDoor(gameMap, tx, ty);
            }
        }
    }

    private static void CloseOpenDoor(Map gameMap, int x, int y)
    {
        var tile = gameMap.GetTile(x, y);
        switch (tile.Name)
        {
            case "Opened Door":
                tile.Name = "Closed Door";
                tile.Symbol = '+';
                tile.IsWalkable = false;
                break;
            case "Closed Door":
                tile.Name = "Opened Door";
                tile.Symbol = '-';
                tile.IsWalkable = true;
                break;
        }

        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = tile.Color;
        Console.Write(tile.Symbol);
    }

    private static void SpawnRandomEntities(int count, List<Entity> templates, Map gameMap, Entity player)
    {
        if (templates.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Entity t = templates[Rnd.Next(templates.Count)];
            int spawnX, spawnY;
            int attempts = 0;

            do
            {
                spawnX = Rnd.Next(0, gameMap.Width);
                spawnY = Rnd.Next(0, gameMap.Height);
                attempts++;
            }
            while ((!gameMap.CanPlaceEntity(spawnX, spawnY) || (spawnX == player.PositionX && spawnY == player.PositionY)) && attempts < 100);

            if (attempts < 100)
            {
                gameMap.PlaceEntity(new Entity(t.Id, t.Name, t.Symbol, t.DefaultHealth, t.CurrentHealth, spawnX, spawnY, t.IsWalkable, t.Behavior), spawnX, spawnY);
            }
        }
    }

    // --- CSV LOADERS ---

    private static List<Entity> LoadEntityTemplates(string filePath)
    {
        var list = new List<Entity>();
        if (!File.Exists(filePath)) return list;

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] p = lines[i].Split(',');
            if (p.Length >= 5)
            {
                EntityBehavior behavior = p.Length >= 6 && Enum.TryParse(p[5].Trim(), true, out EntityBehavior b)
                    ? b
                    : EntityBehavior.Hostile;

                list.Add(new Entity(int.Parse(p[0]), p[1].Trim(), p[2].Trim()[0], int.Parse(p[3]), int.Parse(p[4]), 0, 0, false, behavior));
            }
        }
        return list;
    }

    private static List<Item> LoadItemTemplates(string filePath)
    {
        var list = new List<Item>();
        if (!File.Exists(filePath)) return list;

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] p = lines[i].Split(',');
            if (p.Length >= 3)
            {
                list.Add(new Item(int.Parse(p[0]), p[1].Trim(), p[2].Trim()[0], 0, 0)
                {
                    Description = p.Length >= 4 ? p[3].Trim() : string.Empty,
                    Stats = p.Length >= 5 ? p[4].Trim() : string.Empty
                });
            }
        }
        return list;
    }

    private static List<Tile> LoadTileTemplates(string filePath)
    {
        var list = new List<Tile>();
        if (!File.Exists(filePath)) return list;

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] p = lines[i].Split(',');
            if (p.Length >= 6)
            {
                Enum.TryParse(p[2].Trim(), true, out ConsoleColor color);
                list.Add(new Tile(p[0].Trim(), p[1].Trim()[0], color, bool.Parse(p[3]), bool.Parse(p[4]), bool.Parse(p[5])));
            }
        }
        return list;
    }
}