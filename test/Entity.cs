using System;

class Entity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public char Symbol { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public bool IsWalkable { get; set; }

    public Entity(int id, string name, char symbol, int positionX = 0, int positionY = 0, bool iswalkable = false)
    {
        Id = id;
        Name = name;
        Symbol = symbol;
        PositionX = positionX;
        PositionY = positionY;
        IsWalkable = iswalkable;
    }
}