using System;

class Tile
{
    public string Name { get; set; }
    public char Symbol { get; set; }
    public ConsoleColor Color { get; set; }
    public bool IsWalkable { get; set; }

    public Tile(string name, char symbol, ConsoleColor color, bool isWalkable)
    {
        Name = name;
        Symbol = symbol;
        Color = color;
        IsWalkable = isWalkable;
    }
}