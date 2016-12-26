﻿using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace IsoTools.Internal {
	public class IsoSortingSolver {
		List<Renderer> _tmpRenderers = new List<Renderer>();

		// ---------------------------------------------------------------------
		//
		// Callbacks
		//
		// ---------------------------------------------------------------------

		public void OnAddInstance(IsoObject iso_object) {
			if ( iso_object.cacheRenderers ) {
				iso_object.UpdateCachedRenderers();
			}
		}

		public void OnRemoveInstance(IsoObject iso_object) {
			if ( iso_object.cacheRenderers ) {
				iso_object.ClearCachedRenderers();
			}
		}

		public bool OnMarkDirtyInstance(IsoObject iso_object) {
			return false;
		}

	#if UNITY_EDITOR
		public void OnDrawGizmos() {
		}
	#endif

		// ---------------------------------------------------------------------
		//
		// Functions
		//
		// ---------------------------------------------------------------------

		public bool StepSortingAction(IsoWorld iso_world, IsoScreenSolver screen_solver) {
			Profiler.BeginSample("IsoSortingSolver.ResolveVisibles");
			var dirty = ResolveVisibles(screen_solver);
			Profiler.EndSample();
			if ( dirty ) {
				Profiler.BeginSample("IsoSortingSolver.PlaceVisibles");
				PlaceVisibles(iso_world, screen_solver);
				Profiler.EndSample();
			}
			return dirty;
		}

		public void Clear() {
		}

		// ---------------------------------------------------------------------
		//
		// ResolveVisibles
		//
		// ---------------------------------------------------------------------

		bool ResolveVisibles(IsoScreenSolver screen_solver) {
			var mark_dirty   = false;
			var old_visibles = screen_solver.oldVisibles;
			var cur_visibles = screen_solver.curVisibles;

			for ( int i = 0, e = cur_visibles.Count; i < e; ++i ) {
				var iso_object = cur_visibles[i];
				if ( iso_object.Internal.Dirty ) {
					screen_solver.SetupIsoObjectDepends(iso_object);
					iso_object.Internal.Dirty = false;
					mark_dirty = true;
				}
				if ( iso_object.mode == IsoObject.Mode.Mode3d ) {
					if ( UpdateIsoObjectBounds3d(iso_object) ) {
						mark_dirty = true;
					}
				}
			}

			for ( int i = 0, e = old_visibles.Count; i < e; ++i ) {
				var iso_object = old_visibles[i];
				if ( !cur_visibles.Contains(iso_object) ) {
					screen_solver.ClearIsoObjectDepends(iso_object);
					iso_object.Internal.Dirty = true;
					mark_dirty = true;
				}
			}

			_tmpRenderers.Clear();
			return mark_dirty;
		}

		bool UpdateIsoObjectBounds3d(IsoObject iso_object) {
			var minmax3d = IsoObjectMinMax3D(iso_object);
			var offset3d = iso_object.Internal.Transform.position.z - minmax3d.center;
			if ( iso_object.Internal.MinMax3d.Approximately(minmax3d) ||
				 !Mathf.Approximately(iso_object.Internal.Offset3d, offset3d) )
			{
				iso_object.Internal.MinMax3d = minmax3d;
				iso_object.Internal.Offset3d = offset3d;
				return true;
			}
			return false;
		}

		IsoMinMax IsoObjectMinMax3D(IsoObject iso_object) {
			bool inited    = false;
			var  result    = IsoMinMax.zero;
			var  renderers = GetIsoObjectRenderers(iso_object);
			for ( int i = 0, e = renderers.Count; i < e; ++i ) {
				var bounds  = renderers[i].bounds;
				var extents = bounds.extents;
				if ( extents.x > 0.0f || extents.y > 0.0f || extents.z > 0.0f ) {
					var center    = bounds.center.z;
					var minbounds = center - extents.z;
					var maxbounds = center + extents.z;
					if ( inited ) {
						if ( result.min > minbounds ) {
							result.min = minbounds;
						}
						if ( result.max < maxbounds ) {
							result.max = maxbounds;
						}
					} else {
						inited = true;
						result.Set(minbounds, maxbounds);
					}
				}
			}
			return inited ? result : IsoMinMax.zero;
		}

		List<Renderer> GetIsoObjectRenderers(IsoObject iso_object) {
			if ( iso_object.cacheRenderers ) {
				return iso_object.Internal.Renderers;
			} else {
				iso_object.GetComponentsInChildren<Renderer>(_tmpRenderers);
				return _tmpRenderers;
			}
		}

		// ---------------------------------------------------------------------
		//
		// PlaceVisibles
		//
		// ---------------------------------------------------------------------

		void PlaceVisibles(IsoWorld iso_world, IsoScreenSolver screen_solver) {
			var step_depth   = iso_world.stepDepth;
			var start_depth  = iso_world.startDepth;
			var cur_visibles = screen_solver.curVisibles;
			for ( int i = 0, e = cur_visibles.Count; i < e; ++i ) {
				start_depth = RecursivePlaceIsoObject(
					cur_visibles[i], start_depth, step_depth);
			}
		}

		float RecursivePlaceIsoObject(IsoObject iso_object, float depth, float step_depth) {
			if ( iso_object.Internal.Placed ) {
				return depth;
			}
			iso_object.Internal.Placed = true;
			var self_depends = iso_object.Internal.SelfDepends;
			for ( int i = 0, e = self_depends.Count; i < e; ++i ) {
				depth = RecursivePlaceIsoObject(self_depends[i], depth, step_depth);
			}
			if ( iso_object.mode == IsoObject.Mode.Mode3d ) {
				var zoffset = iso_object.Internal.Offset3d;
				var extents = iso_object.Internal.MinMax3d.size;
				PlaceIsoObject(iso_object, depth + extents * 0.5f + zoffset);
				return depth + extents + step_depth;
			} else {
				PlaceIsoObject(iso_object, depth);
				return depth + step_depth;
			}
		}

		void PlaceIsoObject(IsoObject iso_object, float depth) {
			var iso_internal = iso_object.Internal;
			var old_position = iso_internal.LastTrans;
			iso_internal.Transform.position =
				IsoUtils.Vec3FromVec2(old_position, depth);
		}
	}
}