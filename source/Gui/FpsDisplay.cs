using Godot;
using TestClient.Source.Render.Font;

namespace TestClient.Source.Gui;

public class FpsDisplay
{
	private const int MarginLeft = 4;
	private const int MarginTop = 4;

	private readonly CanvasLayer _layer;
	private readonly FontLabel2D _label;
	private string _lastText;

	private static readonly FontLabel2D.FontStyle Style = FontLabel2D.FontStyle.Default
		.WithColor(new Color(0.27f, 1.0f, 0.27f))
		.WithShadow(true);

	public FpsDisplay(Node root)
	{
		_layer = new CanvasLayer
		{
			Name = "FpsLayer",
			Layer = 100
		};

		_label = new FontLabel2D
		{
			Name = "FpsLabel",
			Position = new Vector2(MarginLeft, MarginTop)
		};

		_layer.AddChild(_label);
		root.AddChild(_layer);
	}

	public void Update(string fpsText)
	{
		if (fpsText == _lastText)
			return;

		_lastText = fpsText;
		_label.SetText(fpsText, Style);
	}

	public void Free()
	{
		_layer?.QueueFree();
	}
}
