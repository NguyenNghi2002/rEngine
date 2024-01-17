public class FloorCell
{
    public readonly int X,Y;

    public bool Walkable;
    public HashSet<GridObject> Objects = new HashSet<GridObject>();

    public override string ToString()
    {
        return Objects == null ? String.Empty :  Objects.Count().ToString();
    }
}
