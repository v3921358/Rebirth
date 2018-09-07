using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Provider;
using reWZ.WZProperties;

namespace Common.Game
{
    public class CFootholdMan
    {
        public List<Foothold> Footholds { get; }

        public CFootholdMan()
        {
            Footholds = new List<Foothold>();
        }

        public void Load(int mapId, WzManager wzMan)
        {
            var wz = wzMan["Map.wz"];
            var path = $"Map/Map{mapId / 100000000}/{mapId}.img/foothold";
            var footholds = wz.ResolvePath(path);

            foreach (WZObject wz1 in footholds)
            {
                foreach (WZObject wz2 in wz1)
                {
                    foreach (WZObject fh in wz2)
                    {
                        var f = new Foothold
                        {
                            Id = Convert.ToInt32(fh.Name),
                            Next = fh["next"].ValueOrDie<int>(),
                            Prev = fh["prev"].ValueOrDie<int>(),
                            X1 = (short)fh["x1"].ValueOrDie<int>(),
                            Y1 = (short)fh["y1"].ValueOrDie<int>(),
                            X2 = (short)fh["x2"].ValueOrDie<int>(),
                            Y2 = (short)fh["y2"].ValueOrDie<int>(),
                        };

                        Footholds.Add(f);
                    }
                }

            }
        }
        public void Load(WZObject mapNode)
        {
            var footholds = mapNode["foothold"];

            foreach (WZObject wz1 in footholds)
            {
                foreach (WZObject wz2 in wz1)
                {
                    foreach (WZObject fh in wz2)
                    {
                        var f = new Foothold
                        {
                            Id = Convert.ToInt32(fh.Name),
                            Next = fh["next"].ValueOrDie<int>(),
                            Prev = fh["prev"].ValueOrDie<int>(),
                            X1 = (short)fh["x1"].ValueOrDie<int>(),
                            Y1 = (short)fh["y1"].ValueOrDie<int>(),
                            X2 = (short)fh["x2"].ValueOrDie<int>(),
                            Y2 = (short)fh["y2"].ValueOrDie<int>(),
                        };

                        Footholds.Add(f);
                    }
                }

            }
        }


        public Foothold FindBelow(TagPoint p)
        {
            List<Foothold> xMatches = new List<Foothold>();

            foreach (Foothold fh in Footholds) // find fhs with matching x coordinates
            {
                if (fh.X1 <= p.X && fh.X2 >= p.X)
                    xMatches.Add(fh);
            }

            xMatches.Sort();

            foreach (Foothold fh in xMatches)
            {
                if (!fh.Wall && fh.Y1 != fh.Y2)
                {
                    int calcY;
                    double s1 = Math.Abs(fh.Y2 - fh.Y1);
                    double s2 = Math.Abs(fh.X2 - fh.X1);
                    double s4 = Math.Abs(p.X - fh.X1);
                    double alpha = Math.Atan(s2 / s1);
                    double beta = Math.Atan(s1 / s2);
                    double s5 = Math.Cos(alpha) * (s4 / Math.Cos(beta));

                    if (fh.Y2 < fh.Y1)
                    {
                        calcY = fh.Y1 - ((int)s5);
                    }
                    else
                    {
                        calcY = fh.Y1 + ((int)s5);
                    }

                    if (calcY >= p.Y)
                    {
                        return fh;
                    }
                }
                else if (!fh.Wall && fh.Y1 >= p.Y)
                {
                    return fh;
                }
            }
            return null;
        }

    }
}
