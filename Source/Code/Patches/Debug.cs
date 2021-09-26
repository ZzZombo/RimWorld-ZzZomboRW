using UnityEngine;
using Verse;

namespace ZzZomboRW.Framework
{
	[HotSwappable]
	public static class LogーPatch
	{
		static bool notified;
		public static int MessageCount
		{
			get;
			private set;
		}
		public static uint MessageLimit { get; set; } = 3000;
		internal static void Notify_MessageReceivedThreadedInternalーPrefix()
		{
			if(!notified && !Log.reachedMaxMessagesLimit && !Debug.unityLogger.logEnabled)
			{
				Debug.unityLogger.logEnabled = true;
				Log.Warning($"[{MOD.NAME}] Reactivating file logging.");
				notified = true;
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
		internal static void ResetMessageCountーPostfix()
		{
			MessageCount = 0;
			Log.messageCount = 0;
			Log.reachedMaxMessagesLimit = false;
			Debug.unityLogger.logEnabled = true;
			Log.Message($"[{MOD.NAME}] Messages cleared.");
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
	internal static class StaticConstructorOnStartupUtilityーPatch
	{
		public static bool ReportProbablyMissingAttributesーPrefix()
		{
			Log.Message($"[{MOD.NAME}] Prevented executing `{nameof(StaticConstructorOnStartupUtility)}.{nameof(StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes)}()`.");
			return false;
		}
	}
}
