

using Raylib_cs;

namespace Engine.UI
{
	public interface ICullable
	{
		void SetCullingArea(Rectangle cullingArea);
	}
}