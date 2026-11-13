namespace NullDescent;

public enum EntityBehavior
{
    Friendly,          // NPCs (e.g., Merchants, Townsfolk)
    Neutral,           // Animals (e.g., Deer, stray dogs)
    Hostile,           // Enemies (e.g., Orcs, Goblins)
    PassiveAggressive  // Neutral until attacked (e.g., Wolves, guards)
}

public class Entity(int id, string name, char symbol, int defaulthealth, int currenthealth, int positionX = 0, int positionY = 0, bool iswalkable = false, EntityBehavior behavior = EntityBehavior.Neutral)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public char Symbol { get; set; } = symbol;
    public int DefaultHealth { get; set; } = defaulthealth;
    public int CurrentHealth { get; set; } = currenthealth;
    public int PositionX { get; set; } = positionX;
    public int PositionY { get; set; } = positionY;
    public bool IsWalkable { get; set; } = iswalkable;
    
    public EntityBehavior Behavior { get; set; } = behavior;
}