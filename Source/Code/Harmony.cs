using System.Reflection;
using HarmonyLib;
using Verse;

namespace ZzZomboRW.Framework
{
	[StaticConstructorOnStartup]
	[HotSwappable]
	internal static class StartupHarmonyHelper
	{
		static StartupHarmonyHelper()
		{
			Mod.Instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
