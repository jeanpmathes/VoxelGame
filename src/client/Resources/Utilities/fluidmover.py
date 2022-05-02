"""
    Takes four fluid textures (single step) and creates the animated strips for the fluid.
"""

import math
import sys

from PIL import Image

anim_frames = 16
resolution = 32
r2a = resolution // anim_frames


def move_image(original, amount) -> Image:
    moved = original.copy()
    for x in range(0, original.width):
        for y in range(0, original.height):
            moved.putpixel((x, y), original.getpixel((x, (y - amount) % original.height)))

    return moved


def create_strip(base_image, spread_fn):
    strip = Image.new("RGBA", (base_image.width * anim_frames, base_image.height))
    for x in range(0, anim_frames):
        strip.paste(move_image(base_image, spread_fn(x) * r2a), (x * base_image.width, 0))
    return strip


fluid_name = sys.argv[1]

textures = [f'{fluid_name}_moving.png',
            f'{fluid_name}_moving_side.png',
            f'{fluid_name}_static.png',
            f'{fluid_name}_static_side.png']


def create_spread_function(max_spread):
    return lambda y: int(math.sin(y * 1 / (anim_frames - 1) * math.pi) * max_spread)


spread_functions = [create_spread_function(6),
                    lambda y: y,
                    create_spread_function(3),
                    create_spread_function(3)]

for texture, fn in zip(textures, spread_functions):
    with Image.open(texture) as image:
        if (image.width, image.height) != (resolution, resolution):
            image = image.crop((0, 0, resolution, resolution))

        result = create_strip(image, fn)
        result.save(texture)
