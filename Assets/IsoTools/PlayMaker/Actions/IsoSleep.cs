﻿using UnityEngine;
using HutongGames.PlayMaker;
using IsoTools.PlayMaker.Internal;

namespace IsoTools.PlayMaker.Actions {
	[ActionCategory("IsoTools.Physics")]
	[HutongGames.PlayMaker.Tooltip(
		"Force a IsoRigidbody to Sleep.")]
	public class IsoSleep : IsoComponentAction<IsoRigidbody> {
		[RequiredField]
		[CheckForComponent(typeof(IsoRigidbody))]
		public FsmOwnerDefault gameObject;

		public override void Reset() {
			gameObject  = null;
		}

		public override void OnEnter() {
			DoAction();
			Finish();
		}

		void DoAction() {
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			if ( UpdateCache(go) ) {
				isoRigidbody.Sleep();
			}
		}
	}
} // IsoTools.PlayMaker.Actions