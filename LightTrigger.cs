using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using ProTransitions;


public class LightTrigger : MonoBehaviour {

	public LightFixture Light;

	private bool _isTriggered = false;

	public void OnTrigger() {
		if (!_isTriggered) {
			_isTriggered = true;

			Light.TurnOn ();

			StartCoroutine (WaitThenEndGame ());
		}
	}

	public IEnumerator WaitThenEndGame () {
		yield return new WaitForSeconds (5);
		Camera.main.GetComponent<Camera>().TransitionOut (2f, TransitionType.GLITCH, 0, Load);
	}

	public void Load() {
		SceneManager.LoadScene ("Error");
	}

}


