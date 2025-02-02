﻿// Copyright (c) 2023 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Mono.Cecil;
using Mono.Cecil.Cil;
using PerformanceFish.Prepatching;
using RimWorld.Planet;

namespace PerformanceFish;

public class GetCompCaching : ClassWithFishPrepatches
{
	public class ThingCompPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.GetComp));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(GetComp<ThingComp>);

		public static T? GetComp<T>(ThingWithComps thing) where T : ThingComp
		{
			if (thing.comps != null)
			{
				ref var cache = ref Cache.ByInt<ThingWithComps, CacheValue<T>>.GetOrAddReference(thing.thingIDNumber);
				return !cache.IsDirty(thing.comps) ? cache.Comp : UpdateCache(thing, ref cache);
			}
			return null;
		}
		
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(ThingWithComps thing, ref CacheValue<T> cache)
			where T : ThingComp
		{
			for (var i = 0; i < thing.comps.Count; i++)
			{
				if (thing.comps[i] is not T comp)
					continue;

				cache.Update(thing, comp);
				return comp;
			}

			cache.Update(thing, null);
			return null;
		}

		public record struct CacheValue<T> where T : ThingComp
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<ThingComp> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(ThingWithComps thing, T? comp)
			{
				if (thing.thingIDNumber < 0)
					return;
				
				_listVersion = thing.comps._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class HediffCompPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= methodof(HediffUtility.TryGetComp<HediffComp>);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(TryGetComp<HediffComp>);

		public static T? TryGetComp<T>(Hediff hediff) where T : HediffComp
		{
			if (hediff is HediffWithComps { comps: { } } hediffWithComps)
			{
				ref var cache = ref Cache.ByInt<HediffWithComps, CacheValue<T>>.GetOrAddReference(hediffWithComps.loadID);
				return !cache.IsDirty(hediffWithComps.comps) ? cache.Comp : UpdateCache(hediffWithComps, ref cache);
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(HediffWithComps hediffWithComps, ref CacheValue<T> cache)
			where T : HediffComp
		{
			for (var i = 0; i < hediffWithComps.comps.Count; i++)
			{
				if (hediffWithComps.comps[i] is not T comp)
					continue;

				cache.Update(hediffWithComps, comp);
				return comp;
			}

			cache.Update(hediffWithComps, null);
			return null;
		}

		public record struct CacheValue<T> where T : HediffComp
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<HediffComp> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(HediffWithComps hediff, T? comp)
			{
				if (hediff.loadID < 0)
					return;
				
				_listVersion = hediff.comps._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class HediffCompPropsPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(HediffDef), nameof(HediffDef.CompProps));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(CompProps<HediffCompProperties>);

		public static T? CompProps<T>(HediffDef instance) where T : HediffCompProperties
		{
			if (instance.comps != null)
			{
				ref var cache = ref Cache.ByInt<HediffDef, CacheValue<T>>.GetOrAddReference(instance.shortHash);
				return !cache.IsDirty(instance.comps) ? cache.Comp : UpdateCache(instance, ref cache);
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(HediffDef hediffDef, ref CacheValue<T> cache)
			where T : HediffCompProperties
		{
			for (var i = 0; i < hediffDef.comps.Count; i++)
			{
				if (hediffDef.comps[i] is not T comp)
					continue;

				cache.Update(hediffDef, comp);
				return comp;
			}

			cache.Update(hediffDef, null);
			return null;
		}

		public record struct CacheValue<T> where T : HediffCompProperties
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<HediffCompProperties> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(HediffDef hediffDef, T? comp)
			{
				if (hediffDef.shortHash == 0)
					return;
				
				_listVersion = hediffDef.comps._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class AbilityCompPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(Ability), nameof(Ability.CompOfType));

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(CompOfType<AbilityComp>);

		public static T? CompOfType<T>(Ability ability) where T : AbilityComp
		{
			if (ability.comps != null)
			{
				ref var cache = ref Cache.ByInt<Ability, CacheValue<T>>.GetOrAddReference(ability.Id);
				return !cache.IsDirty(ability.comps) ? cache.Comp : UpdateCache(ability, ref cache);
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(Ability ability, ref CacheValue<T> cache)
			where T : AbilityComp
		{
			for (var i = 0; i < ability.comps.Count; i++)
			{
				if (ability.comps[i] is not T comp)
					continue;

				cache.Update(ability, comp);
				return comp;
			}

			cache.Update(ability, null);
			return null;
		}

		public record struct CacheValue<T> where T : AbilityComp
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<AbilityComp> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(Ability ability, T? comp)
			{
				if (ability.Id < 0)
					return;
				
				_listVersion = ability.comps._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class WorldObjectCompPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(WorldObject), nameof(WorldObject.GetComponent), Type.EmptyTypes);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(GetComponent<WorldObjectComp>);

		public static T? GetComponent<T>(WorldObject worldObject) where T : WorldObjectComp
		{
			ref var cache = ref Cache.ByInt<WorldObject, CacheValue<T>>.GetOrAddReference(worldObject.ID);
			return !cache.IsDirty(worldObject.comps) ? cache.Comp : UpdateCache(worldObject, ref cache);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(WorldObject worldObject, ref CacheValue<T> cache) where T : WorldObjectComp
		{
			for (var i = 0; i < worldObject.comps.Count; i++)
			{
				if (worldObject.comps[i] is not T comp)
					continue;

				cache.Update(worldObject, comp);
				return comp;
			}

			cache.Update(worldObject, null);
			return null;
		}

		public record struct CacheValue<T> where T : WorldObjectComp
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<WorldObjectComp> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(WorldObject worldObject, T? comp)
			{
				if (worldObject.ID < 0)
					return;
				
				_listVersion = worldObject.comps._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class MapComponentPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(Map), nameof(Map.GetComponent), Type.EmptyTypes);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(GetComponent<MapComponent>);

		public static T? GetComponent<T>(Map map) where T : MapComponent
		{
			ref var cache = ref Cache.ByMap<CacheValue<T>>.GetReferenceFor(map);
			return !cache.IsDirty(map.components) ? cache.Comp : UpdateCache(map, ref cache);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(Map map, ref CacheValue<T> cache) where T : MapComponent
		{
			for (var i = 0; i < map.components.Count; i++)
			{
				if (map.components[i] is not T comp)
					continue;

				cache.Update(map, comp);
				return comp;
			}

			cache.Update(map, null);
			return null;
		}

		public record struct CacheValue<T> where T : MapComponent
		{
			private int _listVersion = -1;
			public T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsDirty(List<MapComponent> comps) => comps._version != _listVersion;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public void Update(Map map, T? comp)
			{
				if (map.uniqueID < 0)
					return;
				
				_listVersion = map.components._version;
				Comp = comp;
			}
			
			public CacheValue() {}
		}
	}
	
	public class WorldComponentPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(World), nameof(World.GetComponent), Type.EmptyTypes);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(GetComponent<WorldComponent>);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T? GetComponent<T>(World world) where T : WorldComponent
			=> !Cache<T>.IsDirty(world) ? Cache<T>.Comp : UpdateCache<T>(world);

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(World world) where T : WorldComponent
		{
			for (var i = 0; i < world.components.Count; ++i)
			{
				if (world.components[i] is not T comp)
					continue;

				Cache<T>.Update(world, comp);
				return comp;
			}

			Cache<T>.Update(world, null);
			return null;
		}

		public static class Cache<T> where T : WorldComponent
		{
			private static int _listVersion = -1;
			private static World? _world;
			public static T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsDirty(World world) => world.components._version != _listVersion || _world != world;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void Update(World world, T? comp)
			{
				_listVersion = world.components._version;
				_world = world;
				Comp = comp;
			}
		}
	}
	
	public class GameComponentPatch : FishPrepatch
	{
		public override string? Description { get; }
			= "Optimizes GetComp lookups with a fast custom dictionary implementation. This is often more than 10x "
			+ "faster than the vanilla method. Replaces Performance Optimizer's error prone and slower optimization.";

		public override MethodBase TargetMethodBase { get; }
			= AccessTools.Method(typeof(Game), nameof(Game.GetComponent), Type.EmptyTypes);

		public override void Transpiler(ILProcessor ilProcessor, ModuleDefinition module)
			=> ilProcessor.ReplaceBodyWith(GetComponent<GameComponent>);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T? GetComponent<T>(Game game) where T : GameComponent
			=> !Cache<T>.IsDirty(game) ? Cache<T>.Comp : UpdateCache<T>(game);

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static T? UpdateCache<T>(Game game) where T : GameComponent
		{
			for (var i = 0; i < game.components.Count; ++i)
			{
				if (game.components[i] is not T comp)
					continue;

				Cache<T>.Update(game, comp);
				return comp;
			}

			Cache<T>.Update(game, null);
			return null;
		}

		public static class Cache<T> where T : GameComponent
		{
			private static int _listVersion = -1;
			private static Game? _game;
			public static T? Comp;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool IsDirty(Game game) => game.components._version != _listVersion || _game != game;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void Update(Game game, T? comp)
			{
				_listVersion = game.components._version;
				_game = game;
				Comp = comp;
			}
		}
	}
}