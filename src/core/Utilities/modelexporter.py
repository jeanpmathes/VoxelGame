# Exports a selected object from blender into the file format that is used by the BlockModel class.

# Currently only quads are supported.

import os
import bpy
import math
import json
from json import JSONEncoder

PATH = os.path.expanduser("~\Desktop\\")

class Vertex:
    def __init__(self, co, uv, nm):
        self.X = round(co.x, 5)
        self.Y = round(co.z, 5)
        self.Z = round(co.y, 5)
        self.U = round(abs(1 - uv.x), 4)
        self.V = round(uv.y, 4)
        self.N = round(nm.x, 4)
        self.O = round(nm.z, 4)
        self.P = round(nm.y, 4)
    
class Quad:
    def __init__(self, tex_id, verts):
        self.TextureId = tex_id
        self.Vert0 = verts[0]
        self.Vert1 = verts[1]
        self.Vert2 = verts[2]
        self.Vert3 = verts[3]
        
class Model:
    def __init__(self, tex_names, quads):
        self.TextureNames = tex_names
        self.Quads = quads
        
class ModelEncoder(JSONEncoder):
    def default(self, o):
        return o.__dict__

obj = bpy.context.view_layer.objects.active
mesh = obj.data

tex_names = []
quads = []

for face in mesh.polygons:
    
    if len(face.vertices) != 4:
        continue
    
    mat = obj.material_slots[face.material_index].material
    if mat is not None:
        matName = mat.name
    else:
        matName = "none"
        
    if matName not in tex_names:
        tex_names.append(matName)
        
    verts = []
    
    for vertIndx, loopIndx in zip(face.vertices, face.loop_indices):
        cord = mesh.vertices[vertIndx].co
        uv = mesh.uv_layers.active.data[loopIndx].uv
        norm = face.normal
        verts.append(Vertex(cord, uv, norm))
    
    quads.append(Quad(tex_names.index(matName), verts))

model = Model(tex_names, quads)
json = json.dumps(model, indent=4, cls=ModelEncoder)

file = open(PATH + obj.name + ".json", "w+")
file.write(json)
file.close()