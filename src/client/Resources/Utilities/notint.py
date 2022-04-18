"""
    Takes an image and converts it to a variant that will not have tint applied.
    To achieve this, the alpha of every pixel is set to 25%.
"""

import sys

from PIL import Image

alpha = int(255 * 0.25)


def process_image(current_image):
    for x in range(current_image.size[0]):
        for y in range(current_image.size[1]):
            pixel = current_image.getpixel((x, y))
            current_image.putpixel((x, y), (pixel[0], pixel[1], pixel[2], alpha))


for arg in sys.argv[1:]:
    try:
        with Image.open(arg) as image:
            process_image(image)
            image.save(arg)
    except IOError:
        print(f"Error: Could not open file {arg}")
