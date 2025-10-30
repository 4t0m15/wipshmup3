extends ParallaxBackground

@export var SpeedX: float = -100.0
@export var TargetPath: NodePath
@export var VerticalFollowFactor: float = 1.0
@export var ClampYMin: float = -180.0
@export var ClampYMax: float = 180.0
@export var SkyOverscan: float = 2.0

var _target: Node2D = null
var _base_offset: Vector2 = Vector2.ZERO

func _ready() -> void:
	var viewport_w: float = get_viewport().get_visible_rect().size.x
	var viewport_h: float = get_viewport().get_visible_rect().size.y
	var vertical_margin: float = maxf(absf(ClampYMin), absf(ClampYMax)) + 8.0
	for child in get_children():
		if child is ParallaxLayer:
			var parallax_layer: ParallaxLayer = child
			var sprite: Sprite2D = parallax_layer.get_node_or_null("Sprite2D")
			if not sprite:
				for n in parallax_layer.get_children():
					if n is Sprite2D:
						sprite = n
						break
			if sprite and sprite.texture:
				var tex: Texture2D = sprite.texture
				sprite.centered = false
				sprite.offset = Vector2.ZERO
				var layer_name: String = String(parallax_layer.name)
				var is_top: bool = layer_name.findn("Sky") != -1 or layer_name.findn("Cloud") != -1 or layer_name.findn("Moon") != -1
				if is_top:
					var size := tex.get_size()
					var scale_x: float = viewport_w / float(size.x)
					var desired_h: float = (viewport_h + vertical_margin * 2.0) * SkyOverscan
					var scale_y: float = desired_h / float(size.y)
					if layer_name.findn("Sky") != -1:
						sprite.scale = Vector2(scale_x, scale_y)
					sprite.position = Vector2(0.0, -vertical_margin)
					if layer_name.findn("Cloud") != -1:
						var shader := Shader.new()
						shader.code = "shader_type canvas_item; uniform float thresh = 0.08; void fragment(){ vec4 c = texture(TEXTURE, UV); if (c.r < thresh && c.g < thresh && c.b < thresh) discard; COLOR = c; }"
						var mat := ShaderMaterial.new()
						mat.shader = shader
						sprite.material = mat
				else:
					var y: float = viewport_h - float(tex.get_size().y) + vertical_margin
					if layer_name.findn("RiverFront") != -1:
						y += 10.0
					elif layer_name.findn("River") != -1:
						y += 6.0
					sprite.position = Vector2(0.0, y)
				var tex_size := tex.get_size()
				parallax_layer.motion_mirroring = Vector2(float(tex_size.x) * sprite.scale.x, 0.0)

	if str(TargetPath) != "":
		_target = get_node_or_null(TargetPath)

	_base_offset = scroll_base_offset

func _process(delta: float) -> void:
	_base_offset.x += SpeedX * delta
	var viewport_h: float = get_viewport().get_visible_rect().size.y
	var center_y: float = viewport_h * 0.5
	var target_y: float = _target.global_position.y if _target else center_y
	var parallax_y: float = (target_y - center_y) * VerticalFollowFactor
	parallax_y = clampf(parallax_y, ClampYMin, ClampYMax)
	scroll_base_offset = Vector2(_base_offset.x, parallax_y)
