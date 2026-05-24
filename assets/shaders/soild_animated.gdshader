shader_type spatial;
render_mode unshaded, cull_back, depth_draw_always;

uniform sampler2D albedo_tex : source_color, filter_nearest;
uniform float atlas_width = 8.0;
uniform int first_frame_index = 0;
uniform int frame_count = 1;
uniform float frame_time = 0.1;
uniform bool pingpong = false;
uniform int frame_offset = 0;

void fragment() {
    // Calculate current animation frame
    int frame;
    if (pingpong) {
        // Ping-pong: frame_count = total frames (e.g. lava = 38 = 20 fwd + 18 rev)
        // Derive unique: total = 2*unique - 2  =>  unique = (total + 2) / 2
        int unique = (frame_count + 2) / 2;
        int raw = int(mod(TIME / frame_time, float(frame_count)));
        frame = raw < unique ? raw : frame_count - raw;
    } else {
        frame = int(mod(TIME / frame_time, float(frame_count)));
    }

    // Vertex UV points to first frame's tile in atlas.
    // Extract local UV within tile, then offset to current frame.
    float tile_uv = 1.0 / atlas_width;
    vec2 local_uv = fract(UV * atlas_width); // [0,1] within tile

    int fi = first_frame_index + ((frame_offset + frame) % frame_count);
    int col = fi % int(atlas_width);
    int row = fi / int(atlas_width);
    vec2 frame_origin = vec2(float(col), float(row)) * tile_uv;
    vec2 final_uv = frame_origin + local_uv * tile_uv;

    ALBEDO = texture(albedo_tex, final_uv).rgb * COLOR.rgb;
    ALPHA = texture(albedo_tex, final_uv).a;
}
