import bpy,pathlib,json
from mathutils import Vector
root=pathlib.Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
results=[]
for i,name in enumerate(['Turquoise','Coral','Gold','Purple']):
    before=set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Characters/Models'/('Player_'+name+'.fbx')))
    added=set(bpy.data.objects)-before
    mesh=[o for o in added if o.type=='MESH']
    assert len(mesh)==14,(name,len(mesh))
    bpy.context.view_layer.update()
    points=[o.matrix_world@Vector(v) for o in mesh for v in o.bound_box]
    height=max(p.z for p in points)-min(p.z for p in points)
    assert 1.85<height<2.1,(name,height)
    assert all(o.data.materials for o in mesh),name
    results.append({'character':name,'mesh_parts':len(mesh),'height_m':round(height,3),'materials':True})
    for obj in added:
        if obj.parent is None:obj.location.x+=(i-1.5)*1.35
    print('VALIDATED',name,flush=True)
(root/'docs/character-asset-validation.json').write_text(json.dumps(results,indent=2))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=16
scene.render.threads_mode='FIXED';scene.render.threads=2
scene.render.resolution_x=1200;scene.render.resolution_y=700;scene.render.resolution_percentage=100
scene.world.color=(.22,.22,.22)
bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.location.z=.01
mat=bpy.data.materials.new('Floor');mat.diffuse_color=(.15,.19,.23,1);bpy.context.object.data.materials.append(mat)
bpy.ops.object.light_add(type='AREA',location=(1,3,6));bpy.context.object.data.energy=850;bpy.context.object.data.size=5
bpy.ops.object.light_add(type='AREA',location=(-4,-1,4));bpy.context.object.data.energy=650;bpy.context.object.data.size=4
bpy.ops.object.camera_add(location=(3,7,3.3));cam=bpy.context.object
cam.rotation_euler=(Vector((0,0,1))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=6.6;scene.camera=cam
scene.render.filepath=str(root/'docs/previews/character-assets.png')
print('RENDER_START',flush=True)
bpy.ops.render.render(write_still=True)
print('CHARACTER_ASSET_VALIDATION_PASS',flush=True)
