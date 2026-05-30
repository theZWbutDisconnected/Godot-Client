shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_always;

uniform sampler2D albedo_tex : source_color, filter_nearest;
uniform vec4 entity_color : source_color = vec4(0.0, 0.0, 0.0, 0.0);

void fragment() {
    vec4 tex = texture(albedo_tex, UV);
    if (tex.a == 0.0) {
        discard;
    }
    ALBEDO = mix(tex.rgb, entity_color.rgb, entity_color.a);
    ALPHA = tex.a;
}
