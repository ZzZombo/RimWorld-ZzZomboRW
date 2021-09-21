using HarmonyLib;
using UnityEngine;
using Verse;

namespace ZzZomboRW.Framework
{
	[HarmonyPatch(typeof(Log), nameof(Log.Notify_MessageReceivedThreadedInternal))]
	[HotSwappable]
	internal static class Debug_Log_Notify_MessageReceivedThreadedInternal_Patch
	{
		public static int MessageCount
		{
			get;
			private set;
		}
		public static uint MessageLimit { get; set; } = 3000;
		static void Prefix()
		{
			if(!Log.reachedMaxMessagesLimit && !Debug.unityLogger.logEnabled)
			{
				Debug.unityLogger.logEnabled = true;
				var s = $"[{MOD.NAME}] Reactivating file logging.";
				Log.ErrorOnce(s, s.GetHashCode());
			}
			if(MessageCount >= MessageLimit)
			{
				Log.messageCount = 999;
			}
			else
			{
				Log.messageCount = 0;
				Log.reachedMaxMessagesLimit = false;
				Debug.unityLogger.logEnabled = true;
				if(Current.ProgramState == ProgramState.Playing)
				{
					MessageCount += 1;
				}
			}
		}
		[HarmonyPatch(typeof(Log), nameof(Log.ResetMessageCount))]
		[HotSwappable]
		static class Debug_Log_ResetMessageCount_Patch
		{
			static void Postfix()
			{
				MessageCount = 0;
				Log.messageCount = 0;
				Log.reachedMaxMessagesLimit = false;
				Debug.unityLogger.logEnabled = true;
				Log.Message($"[{MOD.NAME}] Messages cleared.");
			}
		}
		[HotSwappable]
		class DebugLog_GameComponent: GameComponent
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for RW")]
			public DebugLog_GameComponent(Game game) : base() { }
			public override void StartedNewGame()
			{
				MessageCount = Log.messageCount = 0;
				Log.reachedMaxMessagesLimit = false;
				Debug.unityLogger.logEnabled = true;
			}
		}
	}

	[HotSwappable]
	internal static class StaticConstructorOnStartupUtilityーReportProbablyMissingAttributes_Patch
	{
		public static bool Prefix()
		{
			Log.Message($"[{MOD.NAME}] Prevented `{nameof(StaticConstructorOnStartupUtility)}.{nameof(StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes)}()`.");
			return false;
		}
	}
}
