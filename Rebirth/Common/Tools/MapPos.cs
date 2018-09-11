using Common.Packets;

namespace Common
{
    public class MapPos
    {
        public TagPoint Position { get; set; }
        public short Foothold { get; set; }
        public byte Stance { get; set; }

        public MapPos()
        {
            Position = new TagPoint();
        }
        public MapPos(TagPoint position)
        {
            Position = position;
        }

        public void DecodeMovePath(byte[] movePath)
        {
            var iPacket = new CInPacket(movePath);

            Position.X = iPacket.Decode2();
            Position.X = iPacket.Decode2();
            var vx = iPacket.Decode2();
            var vy = iPacket.Decode2();

            var size = iPacket.Decode1();
            
            for (int i = 0; i < size; i++)
            {
                var cmd = iPacket.Decode1();

                if (cmd == 0)
                {
                    Position.X = iPacket.Decode2();
                    Position.Y = iPacket.Decode2();
                    var xwob = iPacket.Decode2();
                    var ywob = iPacket.Decode2();
                    Foothold = iPacket.Decode2();
                    var xoff = iPacket.Decode2();
                    var yoff = iPacket.Decode2();
                    Stance = iPacket.Decode1();
                    var duration = iPacket.Decode2();
                }
                else if (cmd == 1)
                {
                    var xmod = iPacket.Decode2();
                    var ymod = iPacket.Decode2();
                    Stance = iPacket.Decode1();
                    var duration = iPacket.Decode2();
                }
                //else if (cmd == 11)
                //{
                //    Position.X = iPacket.Decode2();
                //    Position.Y = iPacket.Decode2();
                //    var unk = iPacket.Decode2();
                //    Stance = iPacket.Decode1();
                //    var duration = iPacket.Decode2();
                //    Logger.Write(LogLevel.Debug, "SubMov 11 {0}", unk);
                //}
                //else if (cmd == 12) //jump down
                //else if (cmd == 2) //the knockback ( by mob after 27 )
                else if (cmd == 27) //When u get hit by a mob ur stance changes
                {
                    Stance = iPacket.Decode1();
                    var unk = iPacket.Decode2();
                }
                else
                {
                    //Logger.Write(LogLevel.Warning, "Unparsed Movement SubOp {0}", cmd);
                    break; //break loop because we didnt parse subop
                }
            }
        }

        public override string ToString()
        {
            return $"Position: {Position.X},{Position.Y} - Fh: {Foothold} - Stance: {Stance}";
        }
    }
}
