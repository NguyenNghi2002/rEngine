using Engine;
using Engine.SceneManager;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using Raylib_cs.Extension;
using rlImGui_cs;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using static ImGuiNET.ImGui;
public interface ICustomInspectorImgui
{
    /// <summary>
    /// Using <see cref="ImGui"/> to show value.
    /// </summary>
    void OnInspectorGUI();
}

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ShowAttribute : Attribute
{
    // See the attribute guidelines at 
    //  http://go.microsoft.com/fwlink/?LinkId=85236

    // This is a positional argument
    public ShowAttribute()
    {

    }

}
public class ImguiEntityManager : GlobalManager
{
    ImGuiIOPtr _io;
    internal ImGuiStylePtr _style;

    public ImguiEntityManager()
    {
        rlImGui.Setup(true);
        _io = ImGui.GetIO();
        _style = ImGui.GetStyle();
        SetGreenStyle();
    }
    public static void SetGreenStyle()
    {
        var style = ImGui.GetStyle();
        var cols = style.Colors;

        var PhthaloGreen = new Vector4(1, 38, 34, 255) / 255;
        var RichBlack = new Vector4(0, 59, 54, 255) / 255;
        var Magnolia = new Vector4(236, 229, 240, 255) / 255;
        var Fulvous = new Vector4(233, 138, 21, 255) / 255;
        var PalatinatePurple = new Vector4(89, 17, 77, 255) / 255;
        var lightGreen = new Vector4(199, 249, 204, 255) / 255;

        style.FrameBorderSize = 1f;
        style.WindowTitleAlign.X = 0.5f;
        style.FramePadding.Y = 5f; 

        cols[(int)ImGuiCol.WindowBg] = new Vector4(1,38,34,130)/255 ;
        cols[(int)ImGuiCol.Border] = Magnolia;
        cols[(int)ImGuiCol.Text] = Magnolia;
        cols[(int)ImGuiCol.TitleBg] = RichBlack;
        cols[(int)ImGuiCol.CheckMark] = lightGreen;
        cols[(int)ImGuiCol.Tab] = Fulvous;
        cols[(int)ImGuiCol.FrameBg] = PhthaloGreen;
        cols[(int)ImGuiCol.PlotLines] = Fulvous;

        cols[(int)ImGuiCol.TabActive] = RichBlack;
        cols[(int)ImGuiCol.TitleBgActive] = Fulvous;
        cols[(int)ImGuiCol.FrameBgActive] = Fulvous;
    }
    protected internal override void OnDrawDebug()
    {
        rlImGui.Begin();
        HirachyWindow.ShowEntityHirachy();
        rlImGui.End();
    }
    public static class HirachyWindow
    {
        static List<Entity> selectedEntities = new List<Entity>();
        const string WindowTitle = "Entities Hirachy";
        static bool showInspector = false;
        public static void ShowEntityHirachy()
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(200, 100), new Vector2(uint.MaxValue));
            if (ImGui.Begin(WindowTitle, ImGuiWindowFlags.None))
            {
                ImGui.Checkbox("Show Debug",ref Debugging.EnableDrawDebug);
                ImGui.Checkbox("ConsoleLog",ref Debugging.EnableConsoleLog);

                var rootEntities = Core.Scene.SceneEntitiesList.Where(e => e.Transform.Parent == null);
                AddEntitiesListWidget(rootEntities, ref selectedEntities, InputUtils.IsShiftDown());
                ShowInspectorWindow(selectedEntities.FirstOrDefault());
                HightLightSelectedEntity(ref selectedEntities);

                ImGui.End();
            }
        }

        #region Entity Hirachy
        public static void HightLightSelectedEntity(ref List<Entity> selectedItems)
        {
            var c = (uint)Raylib.ColorToInt(new Color(255, 0, 0, 255));

            foreach (var e in selectedEntities)
            {
                var p = Core.Scene.ViewSpaceToWindowSpace(e.Transform.Position2 - (Core.Scene.Camera.target) + Core.Scene.Camera.offset);
                var v1 = p - Vector2.UnitY * 20;
                var v2 = p + new Vector2(0.5f,0.5f)*40;
                var v3 = p + new Vector2(-0.5f,0.5f)* 40;
                ImGui.GetForegroundDrawList().AddTriangle(v1,v2,v3, c,1);  // Red color
                ImGui.GetForegroundDrawList().AddCircleFilled(p,5,c);
            }
        }
        static void AddEntitiesListWidget(IEnumerable<Entity> entities, ref List<Entity> selectedItems, bool multiSelect)
        {
            if (ImGui.CollapsingHeader(Core.Scene.SceneName))
            {
                selectedItems = selectedItems.Where(e => e.Scene != null).ToList();
                //ImGui.BeginChild("Dsfds",ImGui.GetContentRegionAvail());
                ImGui.BeginGroup();
                //ImGui.SetNextWindowContentSize(new Vector2(500));

                ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, 0));
                foreach (var e in entities)
                {
                    //if (e.Scene == null) selectedEntities.Remove(e);
                    if (Selectable($"{e.Name}##{e.ID}", selectedItems.Contains(e), ImGuiSelectableFlags.None))
                    {
                        if (!multiSelect) selectedItems.Clear();
                        if (e.Scene != null && !selectedItems.Remove(e)) selectedItems.Add(e);
                    }
                    ChildEntitiesRecursion(e, ref selectedItems, multiSelect);
                }
                ImGui.EndGroup();
                //ImGui.EndChild();

                HandleHirachyActions("actions");
            }

            ImGui.ShowDemoWindow();
        }
        static void ChildEntitiesRecursion(Entity entity, ref List<Entity> selectedItems,bool multiSelect)
        {
            if (!entity.Transform.HasChilds) return;

            ImGui.Indent();
            foreach (var tf in entity.Transform.Childs)
            {
                var e = tf.Entity;
                if (Selectable($"{e.Name}##{e.ID}", selectedItems.Contains(e), ImGuiSelectableFlags.None))
                {
                    if (!multiSelect) selectedItems.Clear();
                    if (!selectedItems.Remove(e)) selectedItems.Add(e);
                }
                ChildEntitiesRecursion(e, ref selectedItems,multiSelect) ;
            }
            ImGui.Unindent();
        }
        static void HandleHirachyActions(string name)
        {
            ImGui.OpenPopupOnItemClick(name, ImGuiPopupFlags.MouseButtonRight);
            ImGui.SetNextWindowContentSize(new Vector2(300, 300));
            if (ImGui.BeginPopup(name, ImGuiWindowFlags.NoMove))
            {
                if (ImGui.Button("[+]", ImGui.GetWindowSize() / 4))
                {
                    if (selectedEntities.Count == 0)
                        Core.Scene?.CreateEntity("Untitled");
                    else
                        selectedEntities.ForEach(en => Core.Scene?.CreateChildEntity(en, $"Untitled from {en.Name}"));
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.Button("[-]", ImGui.GetWindowSize() / 4) && selectedEntities.Count()>0)
                {
                    selectedEntities.ForEach(en => Entity.Destroy(en));
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

        }
        static void HandleAddingComponents(string name)
        {
            ImGui.OpenPopupOnItemClick(name, ImGuiPopupFlags.MouseButtonRight);
            //if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsItemHovered(ImGuiHoveredFlags.RectOnly))
            // ImGui.OpenPopup(name,ImGuiPopupFlags.MouseButtonRight);

            if (ImGui.BeginPopup(name, ImGuiWindowFlags.NoMove))
            {
                var entryAss = Assembly.GetEntryAssembly();
                var refAss = entryAss?.GetReferencedAssemblies().Select(a => Assembly.Load(a));
                Debug.Assert(refAss != null);
                foreach (var a in refAss)
                {
                    if (ImGui.CollapsingHeader(a.GetName().Name))
                    {
                        foreach (var c in a.GetTypes().Where(t => t.IsClass))
                        {
                            ImGui.Button(c.Name);
                        }
                    }
                }
                ImGui.Text("knfdshfdsjhfksd");
                ImGui.EndPopup();
            }
        }
        #endregion

        #region Inspector Window
        static void ShowInspectorWindow(Entity select)
        {

            ImGui.SetNextWindowSizeConstraints(new Vector2(200, 300), new Vector2(1000, 1000));
            var entryAss = Assembly.GetEntryAssembly();
            var refAss = entryAss?.GetReferencedAssemblies();

            var inputHints = ImGuiInputTextFlags.AutoSelectAll;
            var inspectorHints = ImGuiWindowFlags.None;
            var treenodeHints = ImGuiTreeNodeFlags.DefaultOpen;

            /// Show Inspector window frame
            if (ImGui.Begin($"Inspector", inspectorHints))
            {
                if (select != null)
                {
                    /// Name Input Widget
                    ImGui.InputText("Name", ref select.Name, 100, inputHints);
                    ImGui.SameLine();

                    /// Enable Button
                    var e = select.Enable;
                    if (ImGui.Checkbox("Enable", ref e) && e != select.Enable) {
                        select.SetEnable(e);
                    }

                    /// ID 
                    ImGui.TextUnformatted($"ID :{select.ID.ToString()}");



                    /// Transform Widget
                    if (ImGui.TreeNodeEx("Tranform", treenodeHints))
                    {
                        TranformWidget(select);
                        ImGui.TreePop();
                    }

                    /// all custome component Widget
                    foreach (var component in select.Components)
                    {
                        ImGui.Separator();
                        if (ImGui.TreeNodeEx($"{component.GetType().Name}##{component.GetHashCode()}", treenodeHints))
                        {
                            ComponentWiget(component);
                            ImGui.TreePop();
                        }
                    }
                }
                ImGui.End();
            }

        }
        static void TranformWidget(Entity select)
        {
            var style = ImGui.GetStyle();
            var inspectorWidth = ImGui.GetWindowWidth();
            ImGui.PushItemWidth(inspectorWidth / 3 - style.IndentSpacing - style.WindowPadding.X - style.ItemSpacing.X);
            var tf = select.Transform;
            
            //position
            var prevPos = tf.LocalPosition;
            var pos = prevPos;

            ImGui.Text("Local Position");
            ImGui.DragFloat("X##p", ref pos.X); SameLine();
            ImGui.DragFloat("Y##p", ref pos.Y); SameLine();
            ImGui.DragFloat("Z##p", ref pos.Z);
            if (pos != prevPos)
                tf.LocalPosition = pos;

            //scale
            var prevSca = tf.LocalScale;
            var sca = prevSca;

            ImGui.Text("Local Scale");

            ImGui.DragFloat("X##s", ref sca.X); SameLine();
            ImGui.DragFloat("Y##s", ref sca.Y); SameLine();
            ImGui.DragFloat("Z##s", ref sca.Z);

            if (sca != prevSca)
                tf.LocalScale = sca;

            //rotation
            var prevRot = tf.EulerLocalRotation * Raylib.RAD2DEG;
            var rot = prevRot;

            ImGui.Text("Local Rotation");

            ImGui.DragFloat("X##r", ref rot.X); SameLine();
            ImGui.DragFloat("Y##r", ref rot.Y); SameLine();
            ImGui.DragFloat("Z##r", ref rot.Z);

            if (rot != prevRot)
                tf.EulerLocalRotation = rot * Raylib.DEG2RAD;

            ImGui.PopItemWidth();
        }
        static void ComponentWiget(Component component)
        {
            ImGui.Indent();
            ImGui.BeginGroup();
            RecursiveObjectWidget(component,null,null);
            ImGui.EndGroup();
            ImGui.Unindent();
        }
        #endregion
    }


    static void RecursiveObjectWidget( object obj,FieldInfo? objField, object? owner)
    {
        Debug.Assert((objField != null && owner != null) || (objField == null && owner == null), "second and third parameter must be either both null or not null");
        

        //TODO: Handling deadlock where component have fields that point in circle
        //TODO: check for self causing infinite loop
        if (obj is ICustomInspectorImgui cpn)
            cpn.OnInspectorGUI();
        else // If object doesn't have Interface
        {
            var sliderFlags = ImGuiSliderFlags.AlwaysClamp;
            Type objType = obj.GetType();


            try
            {
                //Non- class
                if (objType.IsPrimitive) PrimitiveWidget(objField, obj, owner, sliderFlags);
                else if (objType.IsEnum) EnumWidget(objField, obj, owner);
                else if (objType.IsSZArray) ArrayWidget(objField, obj, owner);
                else if (objType == typeof(Transformation) || objType == typeof(Entity))
                    ImGui.Text($"[{objField.FieldType}] {objField.Name}");
                // Check For class and struct type
                else if (!typeof(Delegate).IsAssignableFrom(objType) &&
                    !objType.IsGenericType && (objType.IsClass || (objType.IsValueType && !objType.IsPrimitive && !objType.IsEnum)))
                {
                    //Is class and contain field
                    foreach (FieldInfo field in objType.GetFields())
                    {
                        //TODO: NOt yet support Delegate type
                        //if (field.Name.Contains("speedDamping")) 
                        //  Console.WriteLine() ;
                        var fieldValue = field.GetValue(obj);
                        if (!field.FieldType.IsPublic || fieldValue.Equals(obj))
                            continue;

                            RecursiveObjectWidget(fieldValue, field, obj);

                    }
                }
                else ImGui.Text($"[{objField.FieldType}] {objField.Name}");
            }
            catch (Exception e) 
            {
                ImGui.TextColored(Raylib.ColorNormalize(Color.RED),$"[ImguiException][{objField.FieldType}] {objField.Name}");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(e.ToString());
                    ImGui.EndTooltip();
                }
            }
        }
    }


    static void PrimitiveWidget(FieldInfo field,object obj,object parentObj,ImGuiSliderFlags sliderFlags)
    {
        switch (Type.GetTypeCode(field.FieldType))
        {
            /**
             * Int Input
             */
            case TypeCode.Int16:
                var int16 = (int)obj;
                if (ImGui.DragInt(field.Name, ref int16, 1, Int16.MinValue, Int16.MaxValue, "%d", sliderFlags))
                    field.SetValue(parentObj, int16);
                break;
            /**
             * Int Input
             */
            case TypeCode.Int32:
                var int32 = (int)obj;
                if (ImGui.DragInt(field.Name, ref int32, 1, Int32.MinValue, Int32.MaxValue, "%d", sliderFlags))
                    field.SetValue(parentObj, int32);
                break;
            /**
             * Float Input
             */
            case TypeCode.Single:
                var single = (Single)obj;
                if (ImGui.DragFloat(field.Name, ref single, 1, Single.MinValue, Single.MaxValue, "%.3f", sliderFlags))
                    field.SetValue(parentObj, single);
                break;
            default:
                ImGui.Text($"[{field.FieldType}] {field.Name}");
                break;
        }
    }
    static void EnumWidget(FieldInfo field, object obj, object parentObj)
    {
        // List of enums name
        var names = Enum.GetNames(field.FieldType);

        // current chosen enum in combo box
        int chosen = Array.FindIndex(names, t => (t) == obj.ToString());

        if (ImGui.Combo($"{field.Name}", ref chosen, names, names.Length))
        {
            field.SetValue(parentObj, Enum.Parse(field.FieldType, names[chosen]));
        }
    }
    static void ArrayWidget(FieldInfo field, object obj, object parentObj)
    {
        if (ImGui.BeginListBox(field.Name))
        {
            var o = (Array)obj;
            for (int i = 0; i < o.Length; i++)
            {
                ImGui.Selectable($"{i}###{field.Name}{i}", false);
                RecursiveObjectWidget(o.GetValue(i),field,parentObj);
            }
            ImGui.EndListBox();
        }
    }
#if false
    void a(Type a, object valueObj)
    {
        if (a.IsEnum)
        {
            // List of enums name
            var names = Enum.GetNames(a);

            // current chosen enum in combo box
            int chosen = Array.FindIndex(names, t => (t) == ValueObj.ToString());

            if (ImGui.Combo($"{field.Name}", ref chosen, names, names.Length))
            {
                field.SetValue(component, Enum.Parse(field.FieldType, names[chosen]));
            }
        }
    } 
#endif

    void ItemDebugLine()
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddRect(min, max, (uint)Raylib.ColorToInt(new Color(255, 0, 0, 255)));  // Red color
    }


    
    IEnumerable<Type> GetAllComponentType(Assembly assembly)
    {
        return assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).Select(t => t);
    }




    ~ImguiEntityManager()
    {
        rlImGui.Shutdown();
    }
}

public abstract class DebuggableVarible
{

    public abstract void DrawWidget(object instance);
}