using Godot;
using System;

public partial class ParallaxController : Node2D
{
	// Optional target (player or camera) to follow. If empty, uses viewport center.
	[Export] public NodePath TargetPath { get; set; } = new NodePath("");

	// Default parallax scale for layers that don't have an explicit scale set (0..1).
	//0 = layer doesn't move,1 = moves exactly with the target.
	[Export] public Vector2 DefaultParallax { get; set; } = new Vector2(0.5f,0.5f);

	// Layers to be moved. Assign child Node2D (Sprite2D) nodes here in the editor.
	[Export] public NodePath[] Layers { get; set; } = new NodePath[0];

	// Per-layer parallax scales. If shorter than Layers, DefaultParallax is used.
	[Export] public Vector2[] Scales { get; set; } = new Vector2[0];

	private Node2D? _target = null;
	private Node2D?[] _layerNodes = new Node2D?[0];
	private Vector2[] _initialPositions = new Vector2[0];

	public override void _Ready()
	{
		try
		{
			GD.Print("ParallaxController: _Ready start");
			// Resolve target if set
			if (!string.IsNullOrEmpty(TargetPath.ToString()))
			{
				_target = GetNodeOrNull<Node2D>(TargetPath);
				GD.Print($"ParallaxController: target path='{TargetPath}', resolved={_target != null}");
			}

			int n = Layers?.Length ??0;
			GD.Print($"ParallaxController: Layers count={n}");
			_layerNodes = new Node2D?[n];
			_initialPositions = new Vector2[n];

			for (int i =0; i < n; i++)
			{
				var path = Layers[i];
				if (path == null || string.IsNullOrEmpty(path.ToString()))
				{
					GD.Print($"ParallaxController: Layers[{i}] empty, skipping");
					continue;
				}
				var node = GetNodeOrNull<Node2D>(path);
				if (node == null)
				{
					GD.PrintErr($"ParallaxController: Failed to resolve layer node at path '{path}'");
					continue;
				}
				_layerNodes[i] = node;
				_initialPositions[i] = node.GlobalPosition;
				GD.Print($"ParallaxController: Resolved layer[{i}]='{path}' at {node.GlobalPosition}");
			}

			GD.Print("ParallaxController: _Ready complete");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ParallaxController: Exception in _Ready: {ex}");
		}
	}

	public override void _Process(double delta)
	{
		try
		{
			Vector2 targetPos = _target?.GlobalPosition ?? GetViewport().GetVisibleRect().Size *0.5f;

			for (int i =0; i < _layerNodes.Length; i++)
			{
				var node = _layerNodes[i];
				if (node == null) continue;

				Vector2 scale = DefaultParallax;
				if (Scales != null && i < Scales.Length)
					scale = Scales[i];

				// Parallax formula: layers with smaller scale move less relative to the target.
				// final position = initial + (target * (1 - scale)).
				node.GlobalPosition = _initialPositions[i] + (targetPos * (Vector2.One - scale));
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"ParallaxController: Exception in _Process: {ex}");
		}
	}
}
