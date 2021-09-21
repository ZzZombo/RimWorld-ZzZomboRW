using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ZzZomboRW.Framework
{
	[StaticConstructorOnStartup]
	[HotSwappable]
	internal static class StartupHarmonyHelper
	{
		public static readonly List<PatchInfo> delayedPatches = new()
		{
		};
		static StartupHarmonyHelper()
		{
			Mod.Instance.ApplyPatches(delayedPatches);
			Mod.Instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
