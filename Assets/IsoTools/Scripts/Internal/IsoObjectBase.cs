﻿namespace IsoTools.Internal {
	public abstract class IsoObjectBase : IsoBehaviour<IsoObject> {
		IsoWorld _isoWorld = null;

		// ---------------------------------------------------------------------
		//
		// Internal
		//
		// ---------------------------------------------------------------------

		public void Internal_ResetIsoWorld() {
			if ( _isoWorld ) {
				_isoWorld.Internal_RemoveIsoObject(this as IsoObject);
				_isoWorld = null;
			}
		}

		public void Internal_RecacheIsoWorld() {
			Internal_ResetIsoWorld();
			if ( IsActive() ) {
				_isoWorld = FindFirstActiveWorld();
				if ( _isoWorld ) {
					_isoWorld.Internal_AddIsoObject(this as IsoObject);
				}
			}
		}

		// ---------------------------------------------------------------------
		//
		// Public
		//
		// ---------------------------------------------------------------------

		public IsoWorld isoWorld {
			get {
				if ( !_isoWorld ) {
					Internal_RecacheIsoWorld();
				}
				return _isoWorld;
			}
		}

		// ---------------------------------------------------------------------
		//
		// Virtual
		//
		// ---------------------------------------------------------------------

		protected override void OnEnable() {
			base.OnEnable();
			Internal_RecacheIsoWorld();
		}

		protected override void OnDisable() {
			base.OnDisable();
			Internal_ResetIsoWorld();
		}

		protected virtual void OnTransformParentChanged() {
			Internal_RecacheIsoWorld();
		}
	}
}