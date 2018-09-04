using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Network;

namespace Common.Client
{
    public class Character
    {
        public static Character Default()
        {
            var x = new Character
            {
                Uid = 1337,
                Name = "Rajan",
                Gender = 0,
                SkinColor = 0,
                Face = 20000,
                Hair = 30000,
                //Pets will be done by ctor
                Level = 10,
                Job = 0,

                StatStr = 10,
                StatDex = 10,
                StatInt = 10,
                StatLuk = 10,
                StatCurHp =  25,
                StatMaxHp =  50,
                StatCurMp = 25,
                StatMaxMp =  50,
                Ap = 1,
                Sp = 1,
                Exp = 0,
                Fame = 100,
                MapId = 180000000,
                MapSpawn = 1
            };

            return x;
        }

        public Character()
        {
            Pets = new long[3];
        }

        public int Uid { get; set; }
        public string Name { get; set; }
        public byte Gender { get; set; }
        public byte SkinColor { get; set; }
        public int Face { get; set; }
        public int Hair { get; set; }
        public long[] Pets { get; set; }
        public byte Level { get; set; }
        public short Job { get; set; }

        public short StatStr { get; set; }
        public short StatDex { get; set; }
        public short StatInt { get; set; }
        public short StatLuk { get; set; }
        public short StatCurHp { get; set; }
        public short StatMaxHp { get; set; }
        public short StatCurMp { get; set; }
        public short StatMaxMp { get; set; }

        public short Ap { get; set; }
        public short Sp { get; set; }

        public int Exp { get; set; }
        public short Fame { get; set; }
        //public int GachExp { get; set; }
        public int MapId { get; set; }
        public byte MapSpawn { get; set; }

        public void EncodeStats(COutPacket p)
        {
            p.Encode4(Uid);

            if(Name.Length > 12)
                throw new InvalidOperationException();

            for (int i = 0; i < 13; i++)
            {
                if (i < Name.Length)
                {
                    p.Encode1((byte)Name[i]);
                }
                else
                {
                    p.Encode1(0);
                }
            }

            p.Encode1(Gender);
            p.Encode1(SkinColor);
            p.Encode4(Face);
            p.Encode4(Hair);

            for(int i = 0;i < Pets.Length;i++)
                p.Encode8(Pets[i]);

            p.Encode1(Level);
            p.Encode2(Job);

            //
            p.Encode2(StatStr);
            p.Encode2(StatDex);
            p.Encode2(StatInt);
            p.Encode2(StatLuk);
            p.Encode2(StatCurHp);
            p.Encode2(StatMaxHp);
            p.Encode2(StatCurMp);
            p.Encode2(StatMaxMp);
            p.Encode2(Ap);
            //

            //if(isEvan || isResistance
            ////p.Encode1((byte)Sp);
            //else
            p.Encode2(Sp);
            //


            /*
                  v17 = v3->_ZtlSecureTear_nJob_CS;
                  v3->_ZtlSecureTear_nAP_CS = v16;
                  v18 = _ZtlSecureFuse<short>(v3->_ZtlSecureTear_nJob, v17);
                  if ( v18 / 1000 != 3 && v18 / 100 != 22 && v18 != 2001 )
                  {
                    v19 = CInPacket::Decode2(iPacket);
                    v3->_ZtlSecureTear_nSP_CS = _ZtlSecureTear<short>(v19, v3->_ZtlSecureTear_nSP);
                    ZList<SPSet>::RemoveAll(&v3->extendSP.lSPSet, (int)v3);
                  }
                  else
                  {
                    v3->_ZtlSecureTear_nSP_CS = _ZtlSecureTear<short>(0, v3->_ZtlSecureTear_nSP);
                    ExtendSP::Decode(&v3->extendSP, (int)v3, iPacket);
                  }
                  v20 = CInPacket::Decode4(iPacket);
                  v3->_ZtlSecureTear_nEXP_CS = _ZtlSecureTear<long>(v20, v3->_ZtlSecureTear_nEXP);
                  v21 = CInPacket::Decode2(iPacket);
                  v3->_ZtlSecureTear_nPOP_CS = _ZtlSecureTear<short>(v21, v3->_ZtlSecureTear_nPOP);
                  v22 = CInPacket::Decode4(iPacket);
                  v3->_ZtlSecureTear_nTempEXP_CS = _ZtlSecureTear<long>(v22, v3->_ZtlSecureTear_nTempEXP);
                  v23 = CInPacket::Decode4(iPacket);
                  v3->_ZtlSecureTear_dwPosMap_CS = _ZtlSecureTear<unsigned long>(v23, v3->_ZtlSecureTear_dwPosMap);
                  v3->nPortal = CInPacket::Decode1(iPacket);
                  v3->nPlaytime = CInPacket::Decode4(iPacket);
                  v3->nSubJob = CInPacket::Decode2(iPacket);
                }
             */

            p.Encode4(Exp); //nEXP
            p.Encode2(Fame); //nPOP
            p.Encode4(0); //nTempEXP | Gachapon?
            p.Encode4(MapId); //dwPosMap_CS
            p.Encode1(MapSpawn); //v3->nPortal
            p.Encode4(0); //v3->nPlaytime
            p.Encode2(0); //v3->nSubJob | 1 here for dual blade?
        }
        public void EncodeLook(COutPacket p)
        {
            p.Encode1(Gender);
            p.Encode1(SkinColor);
            p.Encode4(Face);
            p.Encode1(false);//p.Encode1((byte)(mega ? 0 : 1));
            p.Encode4(Hair);

            //Normal Equips
            p.Encode1(0xFF);

            //Cash Equips
            p.Encode1(0xFF);

            p.Encode4(0);//Cash Weapon

            for(int i = 0;i < Pets.Length;i++)
                p.Encode4(0);
        }
    }
}
