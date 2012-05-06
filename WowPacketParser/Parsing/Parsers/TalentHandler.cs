using System;
using WowPacketParser.Misc;
using WowPacketParser.Enums;
using WowPacketParser.Enums.Version;

namespace WowPacketParser.Parsing.Parsers
{
    public static class TalentHandler
    {
        public static void ReadTalentInfo(ref Packet packet)
        {
            packet.ReadUInt32("Free Talent count");
            var speccount = packet.ReadByte("Spec count");
            packet.ReadByte("Active Spec");
            for (var i = 0; i < speccount; ++i)
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                    packet.ReadUInt32("TalentBranchSpec", i);
                var count2 = packet.ReadByte("Spec Talent Count ", i);
                for (var j = 0; j < count2; ++j)
                {
                    packet.ReadUInt32("Talent Id", i, j);
                    packet.ReadByte("Rank", i, j);
                }

                var glyphs = packet.ReadByte("Glyph count");
                for (var j = 0; j < glyphs; ++j)
                    packet.ReadUInt16("Glyph", i, j);
            }
        }

        [Parser(Opcode.SMSG_TALENTS_INVOLUNTARILY_RESET)]
        public static void HandleTalentsInvoluntarilyReset(Packet packet)
        {
            packet.ReadByte("Unk Byte");
        }

        [Parser(Opcode.SMSG_INSPECT_TALENT)]
        [Parser(Opcode.SMSG_INSPECT_RESULTS_UPDATE)]
        public static void HandleInspectTalent(Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                packet.ReadGuid("GUID");
            else
                packet.ReadPackedGuid("GUID");

            ReadTalentInfo(ref packet);

            var slotMask = packet.ReadUInt32("Slot Mask");
            var slot = 0;
            while (slotMask > 0)
            {
                if ((slotMask & 0x1) > 0)
                {
                    var name = "[" + (EquipmentSlotType)slot + "] ";
                    packet.ReadEntryWithName<UInt32>(StoreNameType.Item, name + "Item Entry");
                    var enchantMask = packet.ReadUInt16();
                    if (enchantMask > 0)
                    {
                        var enchantName = name + "Item Enchantments: ";
                        while (enchantMask > 0)
                        {
                            if ((enchantMask & 0x1) > 0)
                            {
                                enchantName += packet.ReadUInt16();
                                if (enchantMask > 1)
                                        enchantName += ", ";
                            }
                            enchantMask >>= 1;
                        }
                        packet.WriteLine(enchantName);
                    }
                    packet.ReadUInt16(name + "Unk Uint16");
                    packet.ReadPackedGuid(name + "Creator GUID");
                    packet.ReadUInt32(name + "Unk Uint32");
                }
                ++slot;
                slotMask >>= 1;
            }
        }

        [Parser(Opcode.MSG_TALENT_WIPE_CONFIRM)]
        public static void HandleTalent(Packet packet)
        {
            packet.ReadGuid("GUID");
            if (packet.Direction == Direction.ServerToClient)
                packet.ReadUInt32("Gold");
        }

        [Parser(Opcode.SMSG_TALENTS_INFO)]
        public static void HandleTalentsInfo(Packet packet)
        {
            var pet = packet.ReadBoolean("Pet Talents");
            if (pet)
            {
                packet.ReadUInt32("Unspent Talent");
                var count = packet.ReadByte("Talent Count");
                for (var i = 0; i < count; ++i)
                {
                    packet.ReadUInt32("Talent ID", i);
                    packet.ReadByte("Rank", i);
                }
            }
            else
                ReadTalentInfo(ref packet);
        }

        [Parser(Opcode.CMSG_LEARN_PREVIEW_TALENTS)]
        [Parser(Opcode.CMSG_LEARN_PREVIEW_TALENTS_PET)]
        public static void HandleTalentPreviewTalents(Packet packet)
        {
            if (packet.Opcode == Opcodes.GetOpcode(Opcode.CMSG_LEARN_PREVIEW_TALENTS_PET))
                packet.ReadGuid("GUID");

            var count = packet.ReadUInt32("Talent Count");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadUInt32("Talent ID", i);
                packet.ReadUInt32("Rank", i);
            }
        }

        [Parser(Opcode.CMSG_LEARN_TALENT)]
        public static void HandleLearnTalent(Packet packet)
        {
            packet.ReadUInt32("Talent ID");
            packet.ReadUInt32("Rank");
        }

        //[Parser(Opcode.CMSG_UNLEARN_TALENTS)]

        //[Parser(Opcode.CMSG_PET_LEARN_TALENT)]
        //[Parser(Opcode.CMSG_PET_UNLEARN_TALENTS)]
        //[Parser(Opcode.CMSG_SET_ACTIVE_TALENT_GROUP_OBSOLETE)]
        //[Parser(Opcode.CMSG_SET_PRIMARY_TALENT_TREE)] 4.0.6a
    }
}
