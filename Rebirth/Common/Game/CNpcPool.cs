using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Provider;
using reWZ.WZProperties;

namespace Common.Game
{
    public class CNpcPool : CObjectPool<int,CNpc>
    {
        //public List<CNpc> Spawns { get; }

        public CNpcPool()
        {
            //Spawns = new List<CNpc>();
        }

        public void Load(int mapId, WzManager wzMan)
        {
            var wz = wzMan["Map.wz"];
            var path = $"Map/Map{mapId / 100000000}/{mapId}.img/life";
            var life = wz.ResolvePath(path);

            foreach (WZObject node in life)
            {
                var type = node["type"].ValueOrDie<string>();

                if (type != "n")
                    continue;

                var id = node["id"].ValueOrDie<string>();

                var fh = node["fh"].ValueOrDie<int>();
                var x = node["x"].ValueOrDie<int>();
                var y = node["y"].ValueOrDie<int>();

                var cy = node["cy"].ValueOrDie<int>();
                //var f = node["f"].ValueOrDie<int>();
                //var hide = node["hide"].ValueOrDie<int>();
                var rx0 = node["rx0"].ValueOrDie<int>();
                var rx1 = node["rx1"].ValueOrDie<int>();
                //var mobTime = node["mobTime"].ValueOrDie<int>();

                var uid = GetUniqueId();

                var cl = new CNpc
                {
                    dwNpcId = uid,
                    Id = Convert.ToInt32(id),
                    Type = type,
                    Foothold = fh,
                    X = x,
                    Y = y,
                    Cy = cy,
                    //F = f,
                    //Hide = hide,
                    Rx0 = rx0,
                    Rx1 = rx1,
                    //MobTime = mobTime
                };

                Add(uid, cl);
            }
        }
        public void Load(WZObject mapNode)
        {
            var life = mapNode["life"];

            foreach (WZObject node in life)
            {
                var type = node["type"].ValueOrDie<string>();

                if (type != "n")
                    continue;

                var id = node["id"].ValueOrDie<string>();

                var fh = node["fh"].ValueOrDie<int>();
                var x = node["x"].ValueOrDie<int>();
                var y = node["y"].ValueOrDie<int>();

                var cy = node["cy"].ValueOrDie<int>();
                //var f = node["f"].ValueOrDie<int>();
                //var hide = node["hide"].ValueOrDie<int>();
                var rx0 = node["rx0"].ValueOrDie<int>();
                var rx1 = node["rx1"].ValueOrDie<int>();
                //var mobTime = node["mobTime"].ValueOrDie<int>();

                var uid = GetUniqueId();

                var cl = new CNpc
                {
                    dwNpcId = uid,
                    Id = Convert.ToInt32(id),
                    Type = type,
                    Foothold = fh,
                    X = x,
                    Y = y,
                    Cy = cy,
                    //F = f,
                    //Hide = hide,
                    Rx0 = rx0,
                    Rx1 = rx1,
                    //MobTime = mobTime
                };

                Add(uid, cl);
            }
        }
    }
}
