using System.Reflection;
using HarmonyLib;
using Verse;

namespace ZzZomboRW.Template //*FIXME*
{
	[StaticConstructorOnStartup]
	internal static class StartupHarmonyHelper
	{
		static StartupHarmonyHelper()
		{
			Mod.Instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
