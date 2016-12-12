using UnityEngine;
using System.Collections;

public class LightTriggerer : MonoBehaviour {

	public void OnTriggerEnter(Collider other) {
		var v = other.GetComponent<LightTrigger>();

		if (v != null) {
			v.OnTrigger ();
		}
	}
}