using System.Reflection;
using HarmonyLib;
namespace ScannerRoomBeacons
{
    public class MainPatcher
    {
        public static void Patch()
        {
            Harmony harmony = new Harmony("com.magix39.subnautica.ScannerRoomBeacons.mod");   // DO I really need this long mf name? Who cares.
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
