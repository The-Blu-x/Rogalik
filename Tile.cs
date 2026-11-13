namespace NullDescent;

public class Tile(string name, char symbol, ConsoleColor color, bool isOcupied, bool isWalkable, bool interactable)
{
    public string Name { get; set; } = name;
    public char Symbol { get; set; } = symbol;
    public ConsoleColor Color { get; set; } = color;
    public bool IsOcupied { get; set; } = isOcupied;
    public bool IsWalkable { get; set; } = isWalkable;
    public bool Interactable { get; set; } = interactable;
}