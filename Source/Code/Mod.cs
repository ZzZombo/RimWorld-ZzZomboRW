using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;


namespace ZzZomboRW.Framework
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute: Attribute
	{
	}

	internal static class MOD
	{
		public const string NAME = "ZzZombo's Framework";
		public static readonly string FullID = typeof(Mod).Namespace;
		public static readonly string ID = $"{FullID.Split(".".ToCharArray(), 1)[1]}";
		public static readonly string IDNoDots = ID.Replace('.', '_');
	}
	internal record PatchInfo
	{
		public Type BaseType, DeclaringType;
		public string BaseTypeName, MethodName;
		public Type[] ArgumentTypes = null, GenericTypes = null;
		public int Priority = -1;
		public (Type PatchClass, string PatchMethod, int? Priority)? Prefix, Postfix, Transpiler, Finalizer;

		public Exception Ex
		{
			get;
			private set;
		}
		public MethodBase TargetMethod
		{
			get;
			private set;
		}
		public MethodInfo PatchedMethod
		{
			get;
			private set;
		}
		public HarmonyMethod PrefixMethod
		{
			get;
			private set;
		}
		public HarmonyMethod PostfixMethod
		{
			get;
			private set;
		}
		public HarmonyMethod TranspilerMethod
		{
			get;
			private set;
		}
		public HarmonyMethod FinalizerMethod
		{
			get;
			private set;
		}

		public MethodBase Resolve()
		{
			try
			{
				if(BaseType is null && BaseTypeName.NullOrEmpty())
				{
					throw new ArgumentNullException(nameof(BaseTypeName),
						$"`{nameof(BaseType)}` and `{nameof(BaseTypeName)}` can't be both `null`/empty.");
				}
				if(this.TargetMethod is null)
				{
					var mi = AccessTools.Method($"{this.BaseType?.FullName ?? BaseTypeName}:{MethodName}",
						ArgumentTypes, GenericTypes);
					BaseType ??= mi?.DeclaringType;
					BaseTypeName ??= BaseType?.FullName;
					while(mi?.IsVirtual is true && (!AccessTools.IsDeclaredMember(mi) || mi.IsAbstract || mi.GetMethodBody() is null))
					{
						mi = AccessTools.Method(mi.DeclaringType.BaseType, MethodName, ArgumentTypes, GenericTypes);
					}
					this.TargetMethod = mi;
					DeclaringType = mi.DeclaringType;
					this.PrefixMethod = Prefix is null ? null : new HarmonyMethod(AccessTools.Method(Prefix?.PatchClass,
						Prefix?.PatchMethod), Prefix?.Priority ?? Priority);
					this.PostfixMethod = Postfix is null ? null : new HarmonyMethod(AccessTools.Method(Postfix?.PatchClass,
						Postfix?.PatchMethod), Postfix?.Priority ?? Priority);
					this.TranspilerMethod = Transpiler is null ? null : new HarmonyMethod(AccessTools.Method(Transpiler?.PatchClass,
						Transpiler?.PatchMethod), Transpiler?.Priority ?? Priority);
					this.FinalizerMethod = Finalizer is null ? null : new HarmonyMethod(AccessTools.Method(Finalizer?.PatchClass,
						Finalizer?.PatchMethod), Finalizer?.Priority ?? Priority);
				}
				return this.TargetMethod;
			}
			catch(Exception ex)
			{
				this.Ex = ex;
				return null;
			}
		}

		public void ApplyPatch(Harmony instance)
		{
			this.PatchedMethod = instance.Patch(this.TargetMethod, this.PrefixMethod, this.PostfixMethod, this.TranspilerMethod, this.FinalizerMethod);
		}

		public void Unpatch(Harmony instance)
		{
			foreach(var m in new[]
			{
				this.PrefixMethod?.method,
				this.PostfixMethod?.method,
				this.TranspilerMethod?.method,
				this.FinalizerMethod?.method,
			}.OfType<MethodInfo>())
			{
				instance.Unpatch(this.TargetMethod, m);
			}
			this.PatchedMethod = null;
		}

		public override string ToString()
		{
			if(this.Resolve() is MethodBase mi)
			{
				return mi.FullDescription();
			}
			else
			{
				var desc = $"`{BaseType?.FullDescription() ?? BaseTypeName}.{MethodName}{ArgumentTypes?.Description() ?? "(???)"}`";
				return BaseType is null
					? $"{desc} (in a missing type)"
					: this.Ex switch
					{
						null => $"missing method {desc}",
						AmbiguousMatchException => $"{desc} (ambiguous match)",
						_ => $"erroneous method {desc}: {this.Ex}"
					};
			}
		}
	}

	[HotSwappable]
	internal class Mod: Verse.Mod
	{
		public static Mod Instance;
		public readonly Harmony harmony;
		private readonly ModSettings settings;
		public static readonly List<PatchInfo> immediatePatches = new()
		{
			{
				new PatchInfo()
				{
					BaseType = typeof(StaticConstructorOnStartupUtility),
					MethodName = nameof(StaticConstructorOnStartupUtility.ReportProbablyMissingAttributes),
					Prefix = (typeof(StaticConstructorOnStartupUtilityーPatch),
						nameof(StaticConstructorOnStartupUtilityーPatch.ReportProbablyMissingAttributesーPrefix), Priority.First)
				}
			},
			{
				new PatchInfo()
				{
					BaseType = typeof(Log),
					MethodName = nameof(Log.Notify_MessageReceivedThreadedInternal),
					Prefix = (typeof(LogーPatch), nameof(LogーPatch.Notify_MessageReceivedThreadedInternalーPrefix), null)
				}
			},
			{
				new PatchInfo()
				{
					BaseType = typeof(Log),
					MethodName = nameof(Log.ResetMessageCount),
					Postfix = (typeof(LogーPatch), nameof(LogーPatch.ResetMessageCountーPostfix), null)
				}
			}
		};
		public Mod(ModContentPack content) : base(content)
		{
			Instance ??= this;
			this.harmony = new Harmony(MOD.FullID);
			this.settings = this.GetSettings<ModSettings>();
			this.ApplyPatches(immediatePatches);
		}

		internal void ApplyPatches(IEnumerable<PatchInfo> patchList)
		{
			foreach(var (type, patches) in patchList.ToDictionary((a) =>
			{
				a.Resolve();
				return a.BaseType;
			}, (a) => patchList.Where((b) =>
			{
				b.Resolve();
				return b.BaseType == a.BaseType;
			})))
			{
				Log.Message($"[{MOD.NAME}] Applying harmony patches to " +
					$"{type?.FullDescription() ?? $"a missing type `{patches.FirstOrFallback()?.BaseTypeName ?? "<unknown>"}` (skipped)"}" +
					$"{(type is null ? '.' : ":")}");
				if(type is null)
				{
					continue;
				}
				foreach(var patchInfo in patches)
				{
					try
					{
						// It's assumed `hMethod.declaringType` must be non-null, that is, the patch class exists.
						static string stringify(HarmonyMethod hMethod) => hMethod.method?.FullDescription() ??
							$"missing method `{hMethod.methodName}` in type {hMethod.declaringType.FullDescription()}";
						patchInfo.Resolve();
						var msg = new[] {
							$"\tpatch: {patchInfo}",
							patchInfo.PrefixMethod is null ? "" : $"prefix: {stringify(patchInfo.PrefixMethod)}",
							patchInfo.PostfixMethod is null ? "" : $"postfix: {stringify(patchInfo.PostfixMethod)}",
							patchInfo.TranspilerMethod is null ? "" : $"transpiler: {stringify(patchInfo.TranspilerMethod)}",
							patchInfo.FinalizerMethod is null ? "" : $"finalizer: {stringify(patchInfo.FinalizerMethod)}",
						}.Where((s) => !s.NullOrEmpty()).Join(delimiter: "\n\t\t");
						Log.Message(msg);
						patchInfo.ApplyPatch(this.harmony);
					}
					catch(Exception ex)
					{
						Log.Error($"\tException thrown during application of the patch: {ex}");
					}
				}
			}
		}
#if MOD_SHOW_SETTINGS
		public override void DoSettingsWindowContents(UnityEngine.Rect rect)
		{
			var listingStandard = new Listing_Standard();
			listingStandard.Begin(rect);
			// TODO: add code.
			listingStandard.End();
			base.DoSettingsWindowContents(rect);
		}
		public override string SettingsCategory()
		{
			return MOD.FullID.TryTranslate(out var r) ? r : MOD.NAME;
		}
#endif
	}
}
