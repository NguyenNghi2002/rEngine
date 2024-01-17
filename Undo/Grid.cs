using Engine;
using ImGuiNET;
using System.Numerics;

public class Grid<TCellObject> : Component,ICustomInspectorImgui
{
    const int WIDTH_DIMESION = 0;
    const int HEIGHT_DIMESION = 1;

    public Vector2 CellSize;
    public TCellObject[,] Cells;

    public Grid(int width, int height,float cellWidth, float cellHeight)
    {
        CellSize = new Vector2(cellWidth,cellHeight);

        Cells = new TCellObject[width, height];

#if false
        for (int x = 0; x < _cells.GetLength(WIDTH_DIMESION); x++)
        {
            for (int y = 0; y < _cells.GetLength(HEIGHT_DIMESION); y++)
            {
            }
        } 
#endif
    }

    public void HandleValues(Func<VectorInt2, TCellObject> valueHandler)
    {
        for (int x = 0; x < Cells.GetLength(WIDTH_DIMESION); x++)
        {
            for (int y = 0; y < Cells.GetLength(HEIGHT_DIMESION); y++)
            {
                Cells[x, y] = valueHandler.Invoke(new VectorInt2(x,y));
            }
        }
    }
    public void SetValue(int x, int y,TCellObject value)
    {
        if (IsInside(x, y))
            Cells[x, y] = value;
    }

    public TCellObject GetCell(VectorInt2 cell)
        => GetCell(cell.X, cell.Y);
    public TCellObject GetCell(int x,int y)
    {
        if (IsInside(x, y))
             return Cells[x, y] ;
        else
        {
            Debugging.Log($"Out of bound");
            return default;
        }
    }
    public Vector2 CellToWorld(VectorInt2 location)
        => CellToWorld(location.X,location.Y);
    public Vector2 CellToWorld(int x,int y)
        => new Vector2(x, y) * CellSize;
    public VectorInt2 WorldToCell(Vector2 position)
        => WorldToCell(position.X,position.Y);
    public VectorInt2 WorldToCell(float x,float y)
        => new VectorInt2((int)MathF.Floor(x / CellSize.X), (int)MathF.Floor(y / CellSize.Y)) ;

    public bool IsInside(int x, int y)
        => x >= 0 && x < Cells.GetLength(WIDTH_DIMESION) &&
        y >= 0 && y < Cells.GetLength(HEIGHT_DIMESION);
    public bool IsInside(VectorInt2 location)
        => IsInside(location.X,location.Y);
    void ICustomInspectorImgui.OnInspectorGUI()
    {
        var width = Cells.GetLength(WIDTH_DIMESION);
        var height = Cells.GetLength(HEIGHT_DIMESION);
        if (ImGui.BeginTable("grid", width,ImGuiTableFlags.Borders))
        {
            for (int y = 0; y < width; y++)
            {
                ImGui.TableNextRow();
                for (int x = 0; x < height; x++)
                {
                    ImGui.TableSetColumnIndex(x);

                    ImGui.Text($"{Cells[x,y]}");
                }
            }
            
        }
        ImGui.EndTable();

        
    }
}