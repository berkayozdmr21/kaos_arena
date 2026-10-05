import bpy,math,pathlib
from mathutils import Vector
root=pathlib.Path(__file__).resolve().parents[1];out=root.parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Roughness'].default_value=.65;return m
sand=mat('Sand',(.78,.64,.40));nt=sand.node_tree;noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=150;bump=nt.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.2;nt.links.new(noise.outputs['Fac'],bump.inputs['Height']);nt.links.new(bump.outputs['Normal'],nt.nodes.get('Principled BSDF').inputs['Normal'])
sea=mat('Sea',(.055,.51,.64));foam=mat('Foam',(.85,.96,.96));wall=mat('Barrier',(.23,.27,.30));white=mat('Lines',(.9,.94,.9));wood=mat('Wood',(.39,.25,.12));leaf=mat('Palm',(.16,.38,.16))
def box(n,p,size,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=n;o.scale=size;o.data.materials.append(m);return o
def line(a,b,w,m):
 a=Vector(a);b=Vector(b);o=box('Frame or stripe',(a+b)/2,(w,w,(b-a).length),m);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
box('Beach sand',(-250,0,-.24),(538,1800,.45),sand);box('Distant sea',(700,0,-.65),(1200,1800,.1),sea)
v=[(-10,-14,0),(10,-14,0),(14,-10,0),(14,10,0),(10,14,0),(-10,14,0),(-14,10,0),(-14,-10,0)]
mesh=bpy.data.meshes.new('Pitch');mesh.from_pydata(v,[],[tuple(range(8))]);o=bpy.data.objects.new('Sand court',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(sand)
colors=[(.1,.82,.76),(1,.38,.27),(.98,.77,.23),(.48,.43,.95)]
for i in range(8):
 a=Vector(v[i]);b=Vector(v[(i+1)%8]);axis=(b-a).normalized()
 if i%2==0:line(a+Vector((0,0,.55)),b+Vector((0,0,.55)),1.1,wall) # corrected rectangular barriers below
 else:
  mid=(a+b)/2
  for x,y in [(a,mid-axis*1.4),(mid+axis*1.4,b)]:
   d=y-x;o=box('Fixed barrier',(x+y)/2+Vector((0,0,.55)),(.25,d.length,1.1),wall);o.rotation_euler.z=-math.atan2(d.x,d.y)
# Replace overthick main walls with matching 0.25m thickness.
for o in list(bpy.data.objects):
 if o.name.startswith('Frame or stripe') and o.scale.x>.5:bpy.data.objects.remove(o,do_unlink=True)
for a,b in [(v[0],v[1]),(v[2],v[3]),(v[4],v[5]),(v[6],v[7])]:
 a=Vector(a);b=Vector(b);d=b-a;o=box('Fixed barrier',(a+b)/2+Vector((0,0,.55)),(.25,d.length,1.1),wall);o.rotation_euler.z=-math.atan2(d.x,d.y)
for i,(x,y) in enumerate([(-12,-12),(12,-12),(12,12),(-12,12)]):
 m=mat('Goal '+str(i),colors[i]);forward=Vector((-x,-y,0)).normalized();right=Vector((forward.y,-forward.x,0));origin=Vector((x,y,0))
 def pt(a,b,h):return origin+right*a-forward*b+Vector((0,0,h))
 for a in [-1.4,1.4]:line(pt(a,0,0),pt(a,0,2.2),.13,m);line(pt(a,0,2.2),pt(a,1.8,2.2),.055,m)
 line(pt(-1.4,0,2.2),pt(1.4,0,2.2),.13,m)
 for n in range(15):
  a=-1.4+n*2.8/14;line(pt(a,1.8,0),pt(a,1.8,2.2),.018,white);line(pt(a,0,2.2),pt(a,1.8,2.2),.018,white)
 for n in range(11):
  h=n*.22;line(pt(-1.4,1.8,h),pt(1.4,1.8,h),.018,white)
  for a in [-1.4,1.4]:line(pt(a,0,h),pt(a,1.8,h),.018,white)
for n in range(64):
 a=n*math.tau/64;b=(n+1)*math.tau/64;line((3*math.sin(a),3*math.cos(a),.025),(3*math.sin(b),3*math.cos(b),.025),.045,white)
line((-13.7,0,.025),(13.7,0,.025),.045,white)
verts=[];faces=[]
for j in range(81):
 for i in range(65):
  x=19+i*1.4;y=-65+j*1.6;h=-.38+math.sin(x*.58-4*1.8)*.17+math.sin(x*.24-y*.15-4*.9)*.1;verts.append((x,y,h))
for j in range(80):
 for i in range(64):a=j*65+i;faces.append((a,a+1,a+66,a+65))
me=bpy.data.meshes.new('Wavy sea');me.from_pydata(verts,[],faces);o=bpy.data.objects.new('Wavy sea',me);bpy.context.collection.objects.link(o);o.data.materials.append(sea)
for i in range(7):
 x=19+(i*3-4*1.3)%21
 for j in range(80):
  y=-60+j*1.5;z=-.2+math.sin(x*.58-7.2)*.17+math.sin(x*.24-y*.15-3.6)*.1;line((x,y,z),(x+.08*math.sin(j),y+1.4,z),.06,foam)
for i in range(16):
 y=-43+(i%8)*12;px=-21-(i//8)*8
 box('Palm trunk',(px,y,2.5),(.38,.38,5),wood)
 for n in range(6):a=n*math.tau/6;line((px,y,5),(px+3*math.sin(a),y+3*math.cos(a),4.4),.35,leaf)
for name,pos in [('Ada',(-7,-7)),('Atlas',(7,-7)),('Nehir',(7,7)),('Kaya',(-7,7))]:
 old=set(bpy.data.objects);bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Resources/RosterModels'/(name+'.fbx')))
 for ob in set(bpy.data.objects)-old:
  if ob.parent is None:ob.location.x+=pos[0];ob.location.y+=pos[1]
bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,radius=.23,location=(0,0,.23));bpy.context.object.data.materials.append(white)

# Same cosmetic festival layout used by ArenaBeachFestival.
festival_colors=[(.98,.36,.23),(.05,.63,.76),(.99,.77,.16),(.39,.23,.65)]
festival_kits=[mat('Festival kit '+str(i),c) for i,c in enumerate(festival_colors)]
ivory=mat('Umbrella ivory',(.95,.94,.85));timber=mat('Festival wood',(.58,.36,.18))
def ball_part(name,p,size,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=.5,location=p);ob=bpy.context.object;ob.name=name;ob.scale=size;ob.data.materials.append(m)
 for f in ob.data.polygons:f.use_smooth=True
 return ob
for i in range(8):
 y=-31+(i%4)*20;x=-25-(i//4)*9;c=festival_kits[i%4]
 box('Umbrella pole',(x,y,1.3),(.07,.07,2.6),ivory)
 vv=[(x,y,3.1)]+[(x+1.9*math.cos(n*math.tau/16),y+1.9*math.sin(n*math.tau/16),2.6) for n in range(16)]
 me=bpy.data.meshes.new('Striped canopy');me.from_pydata(vv,[],[(0,1+n,1+(n+1)%16) for n in range(16)]);ob=bpy.data.objects.new('Beach umbrella',me);bpy.context.collection.objects.link(ob);ob.data.materials.append(c);ob.data.materials.append(ivory)
 for n,f in enumerate(ob.data.polygons):f.material_index=n%2
 for n in range(2):
  xx=x-1+n*2;yy=y-1.3;box('Sun lounger',(xx,yy,.38),(.72,1.9,.12),c);back=box('Lounger back',(xx,yy+.9,.76),(.72,.12,.95),ivory);back.rotation_euler.x=math.radians(-25)
  for leg in [-1,1]:box('Lounger legs',(xx,yy+leg*.65,.2),(.66,.08,.35),timber)
 box('Beach towel',(x+2.6,y,.015),(1.2,2,.025),c)
for side in [-1,1]:
 for row in range(2):box('Spectator bench',(-2,side*(18+row*1.8),.4+row*.35),(13,.65,.2),timber)
for i in range(28):
 x,y=(-16.6,-9+(i%12)*1.65) if i<12 else (-8+(i%8)*1.7,-17 if i<20 else 17)
 skin=mat('Fan skin '+str(i),[(.42,.23,.12),(.86,.59,.39),(.66,.39,.23)][i%3]);kit=festival_kits[i%4]
 forward=Vector((-x,-y,0)).normalized();right=Vector((forward.y,-forward.x,0));origin=Vector((x,y,0))
 def fp(a,z,f=0):return origin+right*a+forward*f+Vector((0,0,z))
 ball_part('Fan shirt',fp(0,.97),(.43,.25,.47),kit);ball_part('Fan face',fp(0,1.4),(.35,.32,.39),skin);ball_part('Fan hair',fp(0,1.55,-.015),(.36,.32,.12),wood)
 for side in [-1,1]:
  ball_part('Fan eye',fp(side*.075,1.43,.151),(.03,.015,.045),wall)
  box('Fan shorts',fp(side*.12,.65),(.18,.23,.25),wall);box('Fan leg',fp(side*.12,.31),(.13,.14,.46),skin);box('Fan sandal',fp(side*.12,.075,.03),(.17,.24,.08),ivory)
  line(fp(side*.25,1.12),fp(side*.39,1.49),.12,skin);ball_part('Fan hand',fp(side*.39,1.54),(.13,.13,.13),skin)
box('Beach kiosk',(-30,16,1.1),(4.5,3,2.2),timber);box('Kiosk counter',(-27.65,16,1.05),(.25,3.3,.18),ivory);box('Kiosk awning',(-29,16,2.45),(6,3.8,.18),festival_kits[0])
data=bpy.data.curves.new('Cafe sign','FONT');data.body='PLAJ KAFE';data.size=.42;data.align_x='CENTER';ob=bpy.data.objects.new('Cafe sign',data);bpy.context.collection.objects.link(ob);ob.location=(-27.45,16,2);ob.rotation_euler=(math.pi/2,0,math.pi/2);ob.data.materials.append(ivory)
for i in range(6):
 y=-27+i*10;box('Festival pole',(-19,y,1.8),(.065,.065,3.6),ivory);box('Festival flag',(-18.5,y,3.2),(1,.03,.45),festival_kits[i%4])
for i in range(3):ball_part('Surfboard',(-28+i*.65,19,1.1),(.46,.12,2.1),festival_kits[i])

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=20;scene.render.threads_mode='FIXED';scene.render.threads=4;scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.world.color=(.5,.6,.7)
scene.world.use_nodes=True
world=scene.world.node_tree;world.nodes.clear();bg=world.nodes.new('ShaderNodeBackground');sky=world.nodes.new('ShaderNodeTexSky');sky.sky_type='NISHITA';sky.sun_elevation=math.radians(28);sky.sun_rotation=math.radians(20);sky.sun_size=math.radians(2);sky.altitude=.1;sky.air_density=.8;sky.dust_density=.35;bg.inputs['Strength'].default_value=.4;output=world.nodes.new('ShaderNodeOutputWorld');world.links.new(sky.outputs['Color'],bg.inputs['Color']);world.links.new(bg.outputs['Background'],output.inputs['Surface'])
bpy.ops.object.light_add(type='SUN',location=(20,20,30));bpy.context.object.data.energy=2;bpy.context.object.data.angle=.12;bpy.context.object.rotation_euler=(math.radians(35),math.radians(-20),math.radians(-40))
cloud=mat('Soft white clouds',(.97,.98,1))
for i in range(9):
 for n in range(4):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=1,location=(-65+i*17+n*5,32+(i%3)*14,23+math.sin(n*2+i)*1.3));o=bpy.context.object;o.scale=(6.5,3.5,1.6);o.data.materials.append(cloud)
sunmat=mat('Visible sun',(1,.85,.5));bs=sunmat.node_tree.nodes.get('Principled BSDF');bs.inputs['Emission Color'].default_value=(1,.8,.42,1);bs.inputs['Emission Strength'].default_value=5;bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,radius=2.2,location=(72,25,30));bpy.context.object.data.materials.append(sunmat)
scene.view_settings.exposure=-1.3;scene.view_settings.view_transform='AgX' 
bpy.ops.object.camera_add(location=(-30,-35,32));camera=bpy.context.object;scene.camera=camera
for name,loc,target in [('Sahil-Saha-Genel.png',(-30,-35,32),(7,0,0)),('Sahil-Deniz-Yandan.png',(-13,-20,8),(20,1,0)),('Sahil-Gokyuzu.png',(-23,-25,7),(22,4,10))]:
 camera.location=loc;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.lens=28;scene.render.filepath=str(out/name);bpy.ops.render.render(write_still=True)
print('BEACH_PREVIEWS_PASS')

