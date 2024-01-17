using Engine;
using ImGuiNET;
using System.Numerics;

public class GridObject : Component, ICustomInspectorImgui
{
    public Grid<FloorCell> Grid;
    VectorInt2 location;
    

    public override void OnAddedToEntity()
    {
        Scene.TryFindComponent(out Grid);
        SetLocation(location.X,location.Y);
    }

    public void SnapTransform(Vector2 offset = default)
    {
        Transform.LocalPosition2 = Grid.CellToWorld(location);
        SetLocation(location);
        Transform.LocalPosition2 += offset;
    }

    public void SnapLocation(Vector2 position)
    {
        var positionCell = Grid.WorldToCell(position);
        if (Grid.IsInside(positionCell.X, positionCell.Y))
            SetLocation(positionCell);
    }

    public void Shift(int dx, int dy)
        => MoveToCell(location.X + dx, location.Y + dy);
    public void MoveToCell(int x, int y)
    {
        if (Grid.IsInside(x,y))
        {
            SetLocation(x,y);
        }
    }

    public VectorInt2 GetLocation()
    {
        return location;
    }

    void SetLocation(VectorInt2 location)
        => SetLocation(location.X,location.Y);
    void SetLocation(int x,int y)
    {
        var newLocation = new VectorInt2(x,y);
        if (location == newLocation) return;

        ///Remove previous location
        var cell = Grid.Cells[location.X, location.Y];
        cell.Objects?.Remove(this);

        //Update location
        location = newLocation;

        Grid.Cells[location.X,location.Y].Objects.Add(this);
    }


    void ICustomInspectorImgui.OnInspectorGUI()
    {
        ImGui.Text($"Location: {GetLocation()}");
    }
}
