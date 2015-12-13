﻿using UnityEngine;
using HutongGames.PlayMaker;

namespace IsoTools.PlayMaker.Actions {
	public abstract class IsoComponentAction<T> : FsmStateAction where T : Component {
		T          _cachedComponent;
		GameObject _cachedGameObject;

		protected IsoWorld isoWorld {
			get { return _cachedComponent as IsoWorld; }
		}

		protected IsoObject isoObject {
			get { return _cachedComponent as IsoObject; }
		}

		protected IsoRigidbody isoRigidbody {
			get { return _cachedComponent as IsoRigidbody; }
		}

		protected IsoCollider isoCollider {
			get { return _cachedComponent as IsoCollider; }
		}

		protected IsoBoxCollider isoBoxCollider {
			get { return _cachedComponent as IsoBoxCollider; }
		}

		protected IsoSphereCollider isoSphereCollider {
			get { return _cachedComponent as IsoSphereCollider; }
		}

		protected bool UpdateCache(GameObject go) {
			if ( go ) {
				if ( _cachedComponent == null || _cachedGameObject != go ) {
					_cachedComponent = go.GetComponent<T>();
					_cachedGameObject = go;
					if ( !_cachedComponent ) {
						LogWarning("Missing component: " + typeof(T).FullName + " on: " + go.name);
					}
				}
			} else {
				_cachedComponent = null;
				_cachedGameObject = null;
			}
			return _cachedComponent != null;
		}
	}
} // IsoTools.PlayMaker.Actions