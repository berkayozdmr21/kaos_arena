using System;
using System.Linq;
class RosterRulesTest {
 static void Main(){int cases=0;
 for(int a=0;a<ArenaRosterRules.Count;a++)for(int b=0;b<ArenaRosterRules.Count;b++)for(int c=0;c<ArenaRosterRules.Count;c++)for(int d=0;d<ArenaRosterRules.Count;d++){
  int[] ids={a,b,c,d};bool[] active={true,true,true,true};var kits=ArenaRosterRules.AssignKits(ids,active,null,123);
  if(kits.Distinct().Count()!=4)throw new Exception("Duplicate uniforms");
  for(int i=0;i<4;i++)if(Array.IndexOf(ids,ids[i])==i&&kits[i]!=ids[i])throw new Exception("First choice loses default uniform");
  var stable=ArenaRosterRules.AssignKits(ids,active,kits,999);
  if(!kits.SequenceEqual(stable))throw new Exception("Uniforms change without roster change");cases++;
 }
 for(int mask=0;mask<16;mask++){var active=Enumerable.Range(0,4).Select(i=>(mask&(1<<i))!=0).ToArray();var kits=ArenaRosterRules.AssignKits(new[]{2,2,2,2},active,null,4);if(kits.Where((k,i)=>active[i]).Distinct().Count()!=active.Count(x=>x))throw new Exception("Partial lobby conflict");}
 Console.WriteLine("PASS: "+cases+" four-player selection combinations; 16 partial lobbies; stable unique uniforms.");
 }
}
