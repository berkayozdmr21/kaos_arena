"""Blender 4.4 --background --factory-startup --python tools/build_roster.py
Ten equally sized original stylized footballers. No gameplay colliders in model assets.
"""
import bpy, math, json, pathlib, sys
from mathutils import Vector
ROOT=pathlib.Path(__file__).resolve().parents[1]
spec=json.loads((ROOT/'Assets/Resources/Roster.json').read_text(encoding='utf-8-sig'))['characters']
MODELS=ROOT/'Assets/Resources/RosterModels';SOURCE=ROOT/'SourceArt/Roster'
MODELS.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True)
def xyz(v):return (v[0],-v[2],v[1])
def mat(name,color):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=.72
    return m
def pivot(name,p,parent=None):
    o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.parent=parent;o.location=xyz(p);return o
def part(name,p,size,m,parent,kind='sphere',angle=0):
    if kind=='box':
        bpy.ops.mesh.primitive_cube_add(size=1)
        o=bpy.context.object;o.scale=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        mod=o.modifiers.new('Soft tailored edges','BEVEL');mod.width=min(size)*.24;mod.segments=3
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    else:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,radius=.5);o=bpy.context.object
        o.scale=(size[0],size[2],size[1])
    o.name=name;o.data.name=name+'_Mesh';o.parent=parent;o.location=xyz(p);o.rotation_euler.y=-angle
    o.data.materials.append(m)
    for face in o.data.polygons:face.use_smooth=True
    return o
def text_mesh(text,p,size,m,parent,back=False):
    data=bpy.data.curves.new('Number','FONT');data.body=text;data.size=size;data.align_x='CENTER';data.align_y='CENTER';data.extrude=.001
    o=bpy.data.objects.new('Number',data);bpy.context.collection.objects.link(o);o.parent=parent;o.location=xyz(p)
    o.rotation_euler=(math.pi/2,0,math.pi if back else 0);o.data.materials.append(m)
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.convert(target='MESH')
def make(c,index):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    root=pivot(c['id'],(0,0,0));body=pivot('Body_Pose',(0,0,0),root)
    skin=mat('Skin',c['skin']);hair=mat('Hair',c['hair']);jersey=mat('Jersey_Main',c['jersey']);accent=mat('Jersey_Accent',c['accent'])
    dark=mat('Dark',(.025,.035,.05));white=mat('White',(.95,.97,.95));iris=mat('Iris',[(.09,.15,.08),(.08,.17,.26),(.21,.10,.03)][index%3])
    # Same skeleton, shoulder span and leg dimensions for the entire roster.
    part('Torso',(0,1.07,0),(.57,.49,.34),jersey,body,'box')
    part('Waist',(0,.84,0),(.50,.11,.32),jersey,body,'box')
    part('Collar',(0,1.31,0),(.26,.055,.24),accent,body)
    part('Neck',(0,1.36,0),(.19,.16,.19),skin,body)
    # Face variants change cheeks and expression, never the top of the head or collision shape.
    head=pivot('Head_Pivot',(0,1.62,0),body)
    part('Head',(0,0,0),(.57+(index%3)*.016,.59,.51),skin,head)
    part('Jaw',(0,-.16,.02),(.40+(index%2)*.04,.20,.39),skin,head)
    for side in (-1,1):
        part('Ear',(side*.29,-.015,0),(.105,.17,.10),skin,head)
        part('EarInset',(side*.322,-.016,.02),(.028,.085,.055),accent if index==8 else skin,head)
        eyeX=.116+(index%3)*.008;ey=.035+(index%2)*.01
        part('EyeWhite',(side*eyeX,ey,.236),(.127,.108,.036),white,head)
        part('Iris',(side*eyeX,ey,.254),(.064,.080,.019),iris,head)
        part('Pupil',(side*eyeX,ey,.265),(.036,.055,.012),dark,head)
        part('EyeHighlight',(side*eyeX-.009,ey+.018,.272),(.014,.019,.009),white,head)
        part('Eyebrow',(side*eyeX,.116,.228),(.154,.035,.043),hair,head,'box',side*(.09+(index%4)*.045))
    part('Nose',(0,-.032,.274),(.09+(index%3)*.012,.12,.075),skin,head)
    part('Mouth',(0,-.142,.211),(.13+(index%3)*.012,.016,.024),dark,head)
    if index in (1,3,7):
        part('Beard',(0,-.203,.14),(.31,.115,.22),hair,head)
        part('Moustache',(0,-.117,.238),(.18,.028,.025),hair,head)
    if index in (2,4,8):
        blush=mat('Cheek',tuple(v*.88 for v in c['skin']))
        for s in (-1,1):part('Cheek',(s*.20,-.073,.202),(.055,.028,.015),blush,head)
    # A common 1.95m crown envelope; ten distinct silhouettes inside it.
    part('HairCap',(0,.241,-.025),(.59,.178,.49),hair,head)
    style=c['style']
    if style in (0,5,9):
        for n in range(7):
            x=(n-3)*.063;part('SweptLock',(x,.252,.08),(.14,.15,.33),hair,head,angle=(-.18 if style==0 else .30))
        if style==5:part('SideFringe',(-.19,.155,.21),(.16,.18,.075),hair,head,angle=.4)
    elif style in (1,6):
        for row in range(3):
            for n in range(7):
                x=(n-3)*.079;part('Curl',(x,.215+row*.018,-.12+row*.10),(.13,.17,.14),hair,head)
        if style==6:
            for s in (-1,1):part('SidePuff',(s*.24,.16,-.08),(.17,.22,.24),hair,head)
    elif style==2:
        for s in (-1,1):
            for n in range(5):part('Braid',(s*.29,.04-n*.068,-.14),(.09,.11,.10),hair,head)
        part('Parting',(0,.266,.19),(.025,.026,.08),skin,head)
    elif style==3:
        for n in range(7):part('CropRidge',((n-3)*.069,.234,.09),(.065,.125,.33),hair,head)
        part('BrowBand',(0,.185,.17),(.54,.055,.13),accent,head)
    elif style==4:
        part('BobBack',(0,.035,-.19),(.59,.43,.25),hair,head)
        for s in (-1,1):part('BobSide',(s*.257,.025,0),(.13,.38,.27),hair,head)
        part('Fringe',(0,.158,.213),(.48,.10,.075),hair,head)
    elif style==7:
        part('Band',(0,.185,.17),(.54,.068,.15),accent,head)
        for n in range(5):part('CentreSpikes',(0,.26,-.10+n*.06),(.15,.14,.105),hair,head)
    else:
        for n in range(6):part('SideSweep',(-.17+n*.065,.24,.10),(.17,.16,.29),hair,head,angle=-.25)
        part('SideClip',(.25,.16,.10),(.022,.15,.08),accent,head)
    # Panel patterns, sleeve piping, embroidered crest, socks and laces.
    pattern=index%5
    if pattern==0:
        for x in (-.12,.12):part('KitStripe',(x,1.06,.177),(.04,.40,.015),accent,body,'box')
    elif pattern==1:part('KitSash',(0,1.075,.18),(.055,.46,.015),accent,body,'box',-.55)
    elif pattern==2:
        for y in (.96,1.1):part('KitHoop',(0,y,.18),(.50,.042,.012),accent,body,'box')
    elif pattern==3:
        part('KitYoke',(0,1.245,.16),(.48,.07,.06),accent,body,'box')
    else:
        for s in (-1,1):part('KitSidePanel',(s*.245,1.06,.02),(.045,.38,.31),accent,body,'box')
    part('ClubCrest',(-.16,1.2,.186),(.055,.065,.018),white,body,'box')
    text_mesh(str(index+1),(0,1.065,.192),.18,white,body)
    text_mesh(str(index+1),(0,1.065,-.185),.24,white,body,True)
    for s in (-1,1):
        suffix='Left' if s<0 else 'Right'
        leg=pivot('Leg_'+suffix,(s*.155,.77,0),body)
        part('Shorts_'+suffix,(0,-.10,0),(.27,.29,.30),jersey,leg,'box')
        part('ShortHem_'+suffix,(0,-.235,0),(.269,.04,.302),accent,leg,'box')
        part('Thigh_'+suffix,(0,-.28,0),(.20,.19,.20),skin,leg)
        knee=pivot('Knee_'+suffix,(0,-.34,0),leg)
        part('Sock_'+suffix,(0,-.16,0),(.19,.33,.19),accent,knee)
        part('SockBand_'+suffix,(0,-.045,0),(.195,.04,.195),white,knee)
        part('Boot_'+suffix,(0,-.35,.06),(.24,.16,.38),white,knee,'box')
        part('Sole_'+suffix,(0,-.414,.06),(.24,.025,.38),dark,knee,'box')
        for n in range(3):part('Lace_'+suffix,(0,-.291,.085+n*.043),(.14,.009,.016),dark,knee,'box')
        arm=pivot('Arm_'+suffix,(s*.35,1.24,0),body)
        part('Sleeve_'+suffix,(0,-.075,0),(.235,.22,.25),jersey,arm,'box')
        part('SleeveCuff_'+suffix,(0,-.17,0),(.24,.045,.253),accent,arm,'box')
        part('UpperArm_'+suffix,(0,-.235,0),(.16,.18,.17),skin,arm)
        elbow=pivot('Elbow_'+suffix,(0,-.30,0),arm)
        part('Forearm_'+suffix,(0,-.09,.01),(.15,.20,.16),skin,elbow)
        part('WristTape_'+suffix,(0,-.177,.01),(.152,.036,.163),white,elbow)
        part('Hand_'+suffix,(0,-.215,.02),(.16,.13,.17),skin,elbow,'box')
    # Normalize every visible vertex to exactly the same standing 1.95m height.
    bpy.context.view_layer.update();meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    pts=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box];lo=min(v.z for v in pts);hi=max(v.z for v in pts)
    factor=1.95/(hi-lo);root.scale=(factor,)*3;root.location.z=-lo*factor;bpy.context.view_layer.update()
    bpy.ops.object.select_all(action='SELECT');bpy.context.scene.unit_settings.system='METRIC'
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(c['id']+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(MODELS/(c['id']+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
    print('ROSTER_EXPORT',c['id'],len(meshes),flush=True)

for i,c in enumerate(spec):make(c,i)
print('ROSTER_EXPORT_PASS',flush=True)
