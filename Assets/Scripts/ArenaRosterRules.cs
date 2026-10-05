using System;
using System.Collections.Generic;

// Appearance only. No per-character speed, hitbox, reach or physics values exist here.
public static class ArenaRosterRules {
    public const int Count=15;
    public static int Clamp(int id){return Math.Max(0,Math.Min(Count-1,id));}
    public static int[] AssignKits(int[] characters,bool[] present,int[] previous,int seed){
        var reserved=new HashSet<int>();var used=new HashSet<int>();var seen=new HashSet<int>();var rng=new Random(seed);
        for(int i=0;i<4;i++)if(present[i])reserved.Add(Clamp(characters[i]));
        var result=new int[4];
        for(int i=0;i<4;i++){
            int id=Clamp(characters[i]);result[i]=id;if(!present[i])continue;
            if(seen.Add(id)){used.Add(id);continue;}
            int old=previous!=null&&previous.Length==4?previous[i]:-1;
            if(old>=0&&old<Count&&!reserved.Contains(old)&&!used.Contains(old))result[i]=old;
            else {var choices=new System.Collections.Generic.List<int>();for(int k=0;k<Count;k++)if(!reserved.Contains(k)&&!used.Contains(k))choices.Add(k);result[i]=choices[rng.Next(choices.Count)];}
            used.Add(result[i]);
        }
        return result;
    }
}
