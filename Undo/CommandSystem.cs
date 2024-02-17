using Engine;
using ImGuiNET;
using Raylib_cs;

namespace Undo
{
    public class CommandSystem : Component,ICustomInspectorImgui
    {
        public interface ICommand
        {
            void Execute();
            void Undo();
            void Redo();
        }

        List<ICommand> _commands = new List<ICommand>();
        int _commandIndex = 0;
        public int Count => _commands.Count;
        /// <summary>
        /// Repeat the action in the history
        /// </summary>
        /// <returns><see cref="true"/> on successful redo, <see cref="false"/> when out of bound</returns>
        public bool SendRedoCommand()
        {
            if (_commandIndex > _commands.Count - 1) return false;

            _commands[_commandIndex].Redo();
            _commandIndex++;
            return true;
        }
        /// <summary>
        /// Reverse the action in the history
        /// </summary>
        /// <returns><see cref="true"/> on successful redo, <see cref="false"/> when out of bound</returns>
        public bool SendUndoCommand()
        {
            if (_commandIndex <= 0) return false;

            --_commandIndex;
            _commands[_commandIndex].Undo();
            return true;

        }
        /// <summary>
        /// Run the command and it will be saved into history wait for undo and redo
        /// </summary>
        /// <param name="move"></param>
        public void ExecuteCommand(ICommand move)
        {
            if (_commandIndex < _commands.Count)
                _commands.RemoveRange(_commandIndex, _commands.Count - _commandIndex);

            _commands.Add(move);
            move.Execute();
            _commandIndex++;
        }

        public void ClearCommand()
        {
            _commands.Clear();
            _commandIndex = 0;
        }

        void ICustomInspectorImgui.OnInspectorGUI()
        {
            var tableColumnCnt = 2;
            var tableRowCnt = _commands.Count;
            ImGui.Text($"command Index: {_commandIndex}");
            ImGui.BeginTable($"##{Entity.ID} Table",tableColumnCnt);
            for (int y = 0; y < tableRowCnt; y++)
            {
                ImGui.TableNextRow(); //Next row
                ImGui.TableNextColumn();

                if(_commandIndex == y) ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, (uint)Raylib.ColorToInt(Color.SKYBLUE));
                ImGui.Text(_commands[y].ToString()) ;
                ///Delete button
                
                ImGui.TableNextColumn();
                if (_commandIndex == y) ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, (uint)Raylib.ColorToInt(Color.BLANK));
                if (ImGui.Button($"delete###{y}"))
                {
                    tableRowCnt--;
                }
            }
            
            ImGui.EndTable();
        }
    }
}
