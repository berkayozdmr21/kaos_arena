import bpy,pathlib,json,sys
from mathutils import Vector
root=pathlib.Path(__file__).resolve().parents[1]
spec=json.loads((root/'Assets/Resources/Roster.json').read_text(encoding='utf-8-sig'))['characters']
selected=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if selected:spec=[c for c in spec if c['id'] in selected]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
report=[]
for i,c in enumerate(spec):
    old=set(bpy.data.objects);bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Resources/RosterModels'/(c['id']+'.fbx')))
    added=set(bpy.data.objects)-old;bpy.context.view_layer.update();meshes=[o for o in added if o.type=='MESH']
    points=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box]
    height=max(v.z for v in points)-min(v.z for v in points)
    assert abs(height-1.95)<.001,(c['id'],height)
    for joint in ['Leg_Left','Leg_Right','Arm_Left','Arm_Right','Knee_Left','Knee_Right','Elbow_Left','Elbow_Right']:
        assert any(o.name.split('.')[0]==joint for o in added),(c['id'],joint)
    report.append({'id':c['id'],'height':round(height,5),'meshes':len(meshes),'joints':8})
    for o in added:
        if o.parent is None:o.location.x+=(i%5-2)*1.6;o.location.y+=(i//5)*3.4
    font=bpy.data.curves.new('Label','FONT');font.body=c['name'];font.align_x='CENTER';font.size=.24
    label=bpy.data.objects.new('Label',font);bpy.context.collection.objects.link(label);label.location=((i%5-2)*1.6,(i//5)*3.4-.5,.02)
(root/'docs').mkdir(exist_ok=True)
(root/('docs/women-validation.json' if selected else 'docs/roster-validation.json')).write_text(json.dumps(report,indent=2))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=24;scene.render.threads_mode='FIXED';scene.render.threads=4
scene.render.resolution_x=1800;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.world.color=(.23,.23,.23)
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.location.z=-.01
m=bpy.data.materials.new('Stage');m.diffuse_color=(.12,.16,.20,1);floor.data.materials.append(m)
for pos,power,size in [((1,-4,8),1900,7),((-6,1,6),1300,6),((4,7,6),1000,5)]:
 bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=power;o.data.size=size;o.rotation_euler=(Vector((0,1.5,1))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(2,-11,8));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.9) if selected else (0,1.6,.8))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=(8.7 if selected else max(10.4, len(spec)*.85));scene.camera=cam
scene.render.filepath=str(root.parent/('KaosArena-5-Kadin-Karakter.png' if selected else 'KaosArena-15-Karakter.png'));bpy.ops.render.render(write_still=True)
print('CHARACTER_ASSETS_PASS',flush=True)
