namespace NullDescent;

public class Item(int id, string name, char symbol, int positionX, int positionY)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public char Symbol { get; set; } = symbol;
    public int PositionX { get; set; } = positionX;
    public int PositionY { get; set; } = positionY;
    public string Description { get; set; } = string.Empty;
    public string Stats { get; set; } = string.Empty;
}