namespace NullDescent;
using System.Text;

public class Map
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    private readonly Tile[,] _tiles;
    
    public List<Entity> Entities { get; private set; } = new();
    public List<Item> Items { get; private set; } = new();

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _tiles[x, y] = new Tile("Floor", '.', ConsoleColor.DarkGray, false, true, false);
            }
        }
    }

    public Map(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        Height = lines.Length;
        Width = lines[0].Length;
        _tiles = new Tile[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char c = lines[y][x];
                _tiles[x, y] = CharToTile(c);
            }
        }
    }

    private Tile CharToTile(char c)
    {
        return c switch
        {
            '#' => new Tile("Wall", '#', ConsoleColor.Gray, true, false, false),
            '+' => new Tile("Closed Door", '+', ConsoleColor.DarkYellow, true, false, true),
            '-' => new Tile("Open Door", '-', ConsoleColor.DarkYellow, false, true, true),
            _ => new Tile("Floor", '.', ConsoleColor.DarkGray, false, true, false),
        };
    }

    public void SetTile(int x, int y, Tile newTile)
    {
        if (IsWithinBounds(x, y)) _tiles[x, y] = newTile;
    }

    public Tile GetTile(int x, int y) => _tiles[x, y];
    private bool IsWithinBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public bool CanPlaceItem(int x, int y)
    {
        if (!IsWithinBounds(x, y) || !_tiles[x, y].IsWalkable) return false;
        return !Items.Any(i => i.PositionX == x && i.PositionY == y);
    }
    
    public bool CanPlaceEntity(int x, int y)
    {
        if (!IsWithinBounds(x, y) || !_tiles[x, y].IsWalkable) return false;
        return !Entities.Any(e => e.PositionX == x && e.PositionY == y && e.CurrentHealth > 0);
    }

    public bool PlaceItem(Item item, int x, int y)
    {
        if (!CanPlaceItem(x, y)) return false;
        item.PositionX = x;
        item.PositionY = y;
        Items.Add(item);
        return true;
    }
    
    public bool PlaceEntity(Entity entity, int x, int y)
    {
        if (!CanPlaceEntity(x, y)) return false;
        entity.PositionX = x;
        entity.PositionY = y;
        Entities.Add(entity);
        return true;
    }

    public void RemoveItem(Item item) => Items.Remove(item);
    
    public void RemoveEntity(Entity entity) => Entities.Remove(entity);
    
    public Entity? GetEntityAt(int x, int y) => Entities.FirstOrDefault(e => e.PositionX == x && e.PositionY == y && e.CurrentHealth > 0);
    public Item? GetItemAt(int x, int y) => Items.FirstOrDefault(i => i.PositionX == x && i.PositionY == y);

    public string GenerateReport()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== MAP REPORT ===");
        
        sb.AppendLine($"Entities on map ({Entities.Count}):");
        foreach (var e in Entities)
        {
            sb.AppendLine($"- {e.Name} [{e.Symbol}] ({e.Behavior}) (HP: {e.CurrentHealth}/{e.DefaultHealth}) -> Pos: X:{e.PositionX}, Y:{e.PositionY}");
        }

        sb.AppendLine($"\nItems on map ({Items.Count}):");
        foreach (var i in Items)
        {
            sb.AppendLine($"- {i.Name} [{i.Symbol}] -> Pos: X:{i.PositionX}, Y:{i.PositionY}");
        }
        sb.AppendLine("===================");
        
        return sb.ToString();
    }
}