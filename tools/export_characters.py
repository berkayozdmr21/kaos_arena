"""Blender 4.4: blender --background --python tools/export_characters.py
Reconstructs the existing ArenaGame primitive characters as editable mesh assets.
Unity (x,y,z) maps to Blender (x,-z,y). No rig or baked animation is claimed.
"""
import bpy, math, pathlib, json
from mathutils import Vector

ROOT = pathlib.Path(__file__).resolve().parents[1]
MODELS = ROOT / 'Assets/Characters/Models'
SOURCE = ROOT / 'SourceArt/Characters'
PREVIEW = ROOT / 'docs/previews'
for folder in (MODELS, SOURCE, PREVIEW): folder.mkdir(parents=True, exist_ok=True)
COLORS = [(0.1,.82,.76),(1,.38,.27),(.98,.77,.23),(.48,.43,.95)]
NAMES = ['Turquoise','Coral','Gold','Purple']
def xyz(p): return (p[0],-p[2],p[1])
def material(name, color):
    mat=bpy.data.materials.new(name);mat.diffuse_color=(*color,1);mat.use_nodes=True
    bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bsdf.inputs['Base Color'].default_value=(*color,1);bsdf.inputs['Roughness'].default_value=.88
    return mat
def empty(name, pos, parent=None):
    obj=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(obj);obj.parent=parent;obj.location=xyz(pos);return obj
def shape(name, kind, pos, scale, mat, parent):
    if kind=='cube':bpy.ops.mesh.primitive_cube_add(size=1)
    else:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,radius=.5)
        if kind=='capsule':
            # Unity capsule primitive: radius .5, total height 2, hemisphere centres +/- .5.
            for vertex in bpy.context.object.data.vertices:
                vertex.co.z += .5 if vertex.co.z>=0 else -.5
    obj=bpy.context.object;obj.name=name;obj.data.name=name+'_Mesh';obj.parent=parent
    obj.location=xyz(pos);obj.scale=(scale[0],scale[2],scale[1]);obj.data.materials.append(mat)
    if kind!='cube':
        for face in obj.data.polygons:face.use_smooth=True
    return obj
def create(index):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    for mat in list(bpy.data.materials):bpy.data.materials.remove(mat)
    root=empty('Player_'+NAMES[index],(0,0,0));body=empty('Body_Pose',(0,0,0),root)
    jersey=material('Jersey_'+NAMES[index],COLORS[index]);dark=material('Dark',(.055,.085,.12));white=material('OffWhite',(.88,.92,.86))
    skin=material('Skin_'+NAMES[index],tuple(a+(b-a)*index/3 for a,b in zip((.36,.19,.12),(.85,.61,.43))))
    shape('Jersey','capsule',(0,1.14,0),(.62,.38,.36),jersey,body)
    shape('Head','sphere',(0,1.73,0),(.4,.45,.4),skin,body)
    shape('Hair','sphere',(0,1.89,-.025),(.41,.2,.38),dark,body)
    for side in (-1,1):
        suffix='Left' if side<0 else 'Right'
        leg=empty('Leg_'+suffix,(side*.16,.8,0),body)
        shape('Shorts_'+suffix,'capsule',(0,-.13,0),(.25,.22,.28),dark,leg)
        shape('Sock_'+suffix,'capsule',(0,-.46,0),(.18,.22,.18),white,leg)
        shape('Boot_'+suffix,'cube',(0,-.69,.08),(.22,.15,.39),dark,leg)
        arm=empty('Arm_'+suffix,(side*.37,1.38,0),body)
        shape('Sleeve_'+suffix,'capsule',(0,-.11,0),(.22,.17,.22),jersey,arm)
        shape('Forearm_'+suffix,'capsule',(0,-.35,0),(.15,.16,.15),skin,arm)
    # Baked jersey number is mesh geometry, so it does not depend on a font at runtime.
    font=bpy.data.curves.new('Jersey_Number','FONT');font.body=str(index+1);font.align_x='CENTER';font.align_y='CENTER';font.size=.27;font.extrude=.001
    num=bpy.data.objects.new('Jersey_Number',font);bpy.context.collection.objects.link(num);num.parent=body
    num.location=xyz((0,1.2,-.205));num.rotation_euler=(math.pi/2,0,math.pi);num.data.materials.append(white)
    bpy.ops.object.select_all(action='DESELECT');num.select_set(True);bpy.context.view_layer.objects.active=num;bpy.ops.object.convert(target='MESH')
    bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
    bpy.ops.object.select_all(action='SELECT');bpy.context.view_layer.update()
    filename='Player_'+NAMES[index]
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(filename+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(MODELS/(filename+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
    return root

for i in range(4):create(i)
print('CHARACTER_EXPORT_PASS: four FBX files and four BLEND sources generated.',flush=True)
