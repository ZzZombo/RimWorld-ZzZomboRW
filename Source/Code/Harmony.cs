using System.Collections.Generic;
using System.Reflection;
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
			FrameworkMod.ApplyPatches(FrameworkMod.Instance.harmony, delayedPatches);
			FrameworkMod.Instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
