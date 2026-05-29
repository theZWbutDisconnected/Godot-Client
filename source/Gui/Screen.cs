using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.Render.Font;

namespace TestClient.Source.Gui;

public abstract partial class Screen : Control
{
	protected int Width;
	protected int Height;
	
	public virtual void Init(int width, int height)
	{
		Width = width;
		Height = height;
	}
	
	public virtual void Render(GuiRenderer guiRenderer, float deltaTime)
	{
		DrawScreen(guiRenderer, deltaTime);
	}
	
	protected abstract void DrawScreen(GuiRenderer guiRenderer, float deltaTime);
	
	public virtual void OnClose()
	{
	}
	
	public virtual void HandleMouseClick(int mouseX, int mouseY, MouseButton mouseButton)
	{
	}
	
	public virtual void HandleKeyPressed(Key key)
	{
	}
	
	public virtual void HandleTextInput(string text)
	{
	}
	
	protected void DrawBackground(GuiRenderer guiRenderer, float x, float y, float width, float height)
	{
		guiRenderer.DrawGradientRectV(x, y, width, height, 
			new Color(0.1f, 0.1f, 0.1f, 0.8f), 
			new Color(0.2f, 0.2f, 0.2f, 0.8f));
	}
	
	protected void CenterString(GuiRenderer guiRenderer, string text, float y, int color = 0xFFFFFF)
	{
		float width = guiRenderer.GetStringWidth(text);
		guiRenderer.DrawString(text, (Width - width) / 2, y, color);
	}
}
