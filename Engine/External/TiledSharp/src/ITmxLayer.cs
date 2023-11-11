namespace Engine.TiledSharp
{
	public interface ITmxLayer: ITmxElement
	{
		double OffsetX { get; }
		double OffsetY { get; }
		double Opacity { get; }
		double ParallaxFactorX { get; }
		double ParallaxFactorY { get; }
		TmxColor Color { get; }
		PropertyDict Properties { get; }
		bool Visible { get; }

	}
}