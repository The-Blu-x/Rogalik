using System;

class Tile(string name, char symbol, ConsoleColor color, bool isWalkable)
{
    public string Name { get; set; } = name;
    public char Symbol { get; set; } = symbol;
    public ConsoleColor Color { get; set; } = color;
    public bool IsWalkable { get; set; } = isWalkable;
}