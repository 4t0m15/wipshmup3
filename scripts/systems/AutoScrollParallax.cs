using Godot;

public partial class AutoScrollParallax : ParallaxBackground
{
    [Export] public float SpeedX = -100f;
    [Export] public NodePath TargetPath { get; set; } = new NodePath("");
    [Export] public float VerticalFollowFactor = 1.0f; // 1 = full offset from screen center
    [Export] public float ClampYMin = -180f; // limit how far up backgrounds can scroll
    [Export] public float ClampYMax = 180f;  // limit how far down backgrounds can scroll
    [Export] public float SkyOverscan = 2.0f; // scale sky height by this factor beyond viewport

    private Node2D? _target;
    private Vector2 _baseOffset;

    public override void _Ready()
    {
        // Normalize all layer children so mirroring lines up to texture width,
        // and align vertical positions to avoid visible horizontal gaps.
        float viewportW = GetViewport().GetVisibleRect().Size.X;
        float viewportH = GetViewport().GetVisibleRect().Size.Y;
        float verticalParallaxMargin = Mathf.Max(Mathf.Abs(ClampYMin), Mathf.Abs(ClampYMax)) + 8f;
        foreach (Node child in GetChildren())
        {
            if (child is not ParallaxLayer layer)
                continue;

            // Find the first Sprite2D in this layer
            Sprite2D sprite = layer.GetNodeOrNull<Sprite2D>(".");
            if (sprite == null)
            {
                // Try any direct child sprite
                foreach (Node n in layer.GetChildren())
                {
                    if (n is Sprite2D s) { sprite = s; break; }
                }
            }

            if (sprite?.Texture is Texture2D tex)
            {
                // Ensure top-left anchoring to avoid half-width offsets
                sprite.Centered = false;
                sprite.Offset = Vector2.Zero;

                // Decide vertical anchoring: most bands (mountains, village, rivers)
                // should sit on the bottom of the screen; skies, clouds, moon stay near top.
                string layerName = layer.Name.ToString();
                bool isTopElement = layerName.Contains("Sky", System.StringComparison.OrdinalIgnoreCase)
                    || layerName.Contains("Cloud", System.StringComparison.OrdinalIgnoreCase)
                    || layerName.Contains("Moon", System.StringComparison.OrdinalIgnoreCase);
                if (isTopElement)
                {
                    // Scale the sky to fill the viewport plus vertical parallax margin
                    Vector2 size = tex.GetSize();
                    float scaleX = viewportW / size.X;
                    float desiredHeight = (viewportH + verticalParallaxMargin * 2f) * SkyOverscan;
                    float scaleY = desiredHeight / size.Y;
                    if (layerName.Contains("Sky", System.StringComparison.OrdinalIgnoreCase))
                    {
                        sprite.Scale = new Vector2(scaleX, scaleY);
                    }
                    // Offset upward so extra height sits above
                    sprite.Position = new Vector2(0f, -verticalParallaxMargin);
                    
                    // For clouds with dark key background, apply a simple chroma-key-like shader to drop near-black
                    if (layerName.Contains("Cloud", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var shader = new Shader();
                        shader.Code = @"shader_type canvas_item; uniform float thresh = 0.08; void fragment(){ vec4 c = texture(TEXTURE, UV); if (c.r < thresh && c.g < thresh && c.b < thresh) discard; COLOR = c; }";
                        var mat = new ShaderMaterial();
                        mat.Shader = shader;
                        sprite.Material = mat;
                    }
                }
                else
                {
                    float y = viewportH - tex.GetSize().Y + verticalParallaxMargin;
                    // Ensure water touches the screen bottom (push slightly down)
                    if (layerName.Contains("RiverFront", System.StringComparison.OrdinalIgnoreCase))
                        y += 10f;
                    else if (layerName.Contains("River", System.StringComparison.OrdinalIgnoreCase))
                        y += 6f;
                    sprite.Position = new Vector2(0f, y);
                }

                // Use the texture's pixel width for seamless mirroring
                Vector2 texSize = tex.GetSize();
                layer.MotionMirroring = new Vector2(texSize.X * sprite.Scale.X, 0f);
            }
        }

        if (!string.IsNullOrEmpty(TargetPath.ToString()))
        {
            _target = GetNodeOrNull<Node2D>(TargetPath);
        }

        _baseOffset = ScrollBaseOffset;
    }

    public override void _Process(double delta)
    {
        // horizontal auto-scroll accumulates in base X offset
        _baseOffset.X += SpeedX * (float)delta;

        float viewportH = GetViewport().GetVisibleRect().Size.Y;
        float centerY = viewportH * 0.5f;
        float targetY = _target?.GlobalPosition.Y ?? centerY;

        float parallaxY = (targetY - centerY) * VerticalFollowFactor;
        parallaxY = Mathf.Clamp(parallaxY, ClampYMin, ClampYMax);

        ScrollBaseOffset = new Vector2(_baseOffset.X, parallaxY);
    }
}


