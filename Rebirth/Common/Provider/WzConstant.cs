using reWZ;

namespace Common.Provider
{
    public static class WzConstant
    {
        public static readonly WZReadSelection WZReadSelection
            = WZReadSelection.NeverParseCanvas;

        public static readonly string[] Files =
        {
            "Base.wz",
            "Character.wz",
            "Effect.wz",
            "Etc.wz",
            "Item.wz",
            "List.wz",
            "Map.wz",
            "Mob.wz",
            "Morph.wz",
            "Npc.wz",
            "Quest.wz",
            "Reactor.wz",
            "Skill.wz",
            "Sound.wz",
            "String.wz",
            "TamingMob.wz",
            "UI.wz"
        };

        public static string GetFilePath(string wzFile)
        {
            return $"C:\\Nexon\\MapleStory\\{wzFile}";
        }
    }
}
