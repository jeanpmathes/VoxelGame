# Exports a selected object from blender into the file format that is used by the BlockModel class.
# Current supported Blender version: 2.83.5

# Currently only quads are supported.

import bpy
import json
import os
from json import JSONEncoder

PATH = os.path.expanduser("~\\Desktop\\")


class Vertex:
    def __init__(self, world_coordinates, uv_coordinates):
        self.X = round(world_coordinates.x, 5)
        self.Y = round(world_coordinates.z, 5)
        self.Z = round(world_coordinates.y, 5)
        self.U = round(abs(1 - uv_coordinates.x), 4)
        self.V = round(uv_coordinates.y, 4)


class Quad:
    def __init__(self, texture_id, vertices):
        self.TextureId = texture_id
        self.Vert0 = vertices[0]
        self.Vert1 = vertices[1]
        self.Vert2 = vertices[2]
        self.Vert3 = vertices[3]


class Model:
    def __init__(self, texture_names, all_quads):
        self.TextureNames = texture_names
        self.Quads = all_quads


class ModelEncoder(JSONEncoder):
    def default(self, o):
        return o.__dict__


obj = bpy.context.view_layer.objects.active
mesh = obj.data

tex_names = []
quads = []

for face in mesh.polygons:

    if len(face.vertices) != 4:
        raise Exception("Only quads are supported!")

    mat = obj.material_slots[face.material_index].material
    if mat is not None:
        matName = mat.name
    else:
        matName = "none"

    if matName not in tex_names:
        tex_names.append(matName)

    verts = []

    for vertex_index, loop_index in zip(face.vertices, face.loop_indices):
        xyz = mesh.vertices[vertex_index].co
        uv = mesh.uv_layers.active.data[loop_index].uv
        norm = face.normal
        verts.append(Vertex(xyz, uv))

    quads.append(Quad(tex_names.index(matName), verts))

model = Model(tex_names, quads)
json = json.dumps(model, indent=4, cls=ModelEncoder)

file = open(PATH + obj.name + ".json", "w+")
file.write(json)
file.close()
