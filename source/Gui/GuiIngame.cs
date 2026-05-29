using Godot;
using TestClient.Source.Gui;

namespace TestClient.Source.Gui;

public partial class GuiIngame : Screen
{
	private Texture2D _iconsTexture;
	
	public override void Init(int width, int height)
	{
		base.Init(width, height);
		_iconsTexture = ResourceLoader.Load<Texture2D>("res://assets/gui/icons.png");
	}
	
	protected override void DrawScreen(GuiRenderer guiRenderer, float deltaTime)
	{
		guiRenderer.Bind(_iconsTexture);
			
		float centerX = Width / 2.0f;
		float centerY = Height / 2.0f;
			
		float iconSize = 16.0f;
		float u0 = 0.0f;
		float v0 = 0.0f;
		float u1 = iconSize / _iconsTexture.GetWidth();
		float v1 = iconSize / _iconsTexture.GetHeight();

		iconSize *= Game.Singleton.GetGuiScale();
			
		float iconX = centerX - iconSize / 2.0f;
		float iconY = centerY - iconSize / 2.0f;
			
		guiRenderer.DrawTexturedRect(iconX, iconY, iconSize, iconSize, u0, v0, u1, v1);
	}
}
