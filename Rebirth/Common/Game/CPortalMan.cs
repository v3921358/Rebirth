using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Provider;
using Common.Server;
using reWZ.WZProperties;

namespace Common.Game
{
    public class CPortalMan
    {
        public List<Portal> Portals { get; }

        public CPortalMan()
        {
            Portals = new List<Portal>();
        }
        
        public void Load(int mapId,WzManager wzMan)
        {
            var wz = wzMan["Map.wz"];
            var path = $"Map/Map{mapId / 100000000}/{mapId}.img/portal";
            var portals = wz.ResolvePath(path);

            foreach (WZObject x in portals)
            {
                var p = new Portal
                {
                    nIdx = Convert.ToInt32(x.Name),
                    sName = x["pn"].ValueOrDie<string>(),
                    nType = x["pt"].ValueOrDie<int>(),
                    nTMap = x["tm"].ValueOrDie<int>(),
                    sTName = x["tn"].ValueOrDie<string>(),
                    ptPos =
                    {
                        X = (short)x["x"].ValueOrDie<int>(),
                        Y = (short)x["y"].ValueOrDie<int>()
                    }
                };

                Portals.Add(p);
            }
        }
        public void Load(WZObject mapNode)
        {
            var portals = mapNode["portal"];

            foreach (WZObject x in portals)
            {
                var p = new Portal
                {
                    nIdx = Convert.ToInt32(x.Name),
                    sName = x["pn"].ValueOrDie<string>(),
                    nType = x["pt"].ValueOrDie<int>(),
                    nTMap = x["tm"].ValueOrDie<int>(),
                    sTName = x["tn"].ValueOrDie<string>(),
                    ptPos =
                    {
                        X = (short)x["x"].ValueOrDie<int>(),
                        Y = (short)x["y"].ValueOrDie<int>()
                    }
                };

                Portals.Add(p);
            }
        }

        public Portal GetByName(string name) => Portals.FirstOrDefault(p => p.sName == name);

        public byte GetRandomSpawn()
        {
            var list = Portals.Where(p => p.sName == "sp").ToArray();

            if (list.Length == 0)
                return 0;

            return (byte)list.Random().nIdx;
        }
    }
}
