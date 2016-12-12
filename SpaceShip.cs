using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class SpaceShip : MonoBehaviour {

	public GameObject WorldParent;

	[Header("Game")]
	public string[] GameProgress;
	public TextAsset AiDialog;

	[Header("Coms Panel")]
	public Console ComsConsole;
	
	[Header("PersonalLog---")]
	public Console PersonalLogConsole;
	public Image PersonalLogPanel;
	public Text PersonalLogText;
	public string[] Logs;
	public Text[] LogButtons;

	[Header("Engi Panel")]
	public Console EngiConsole;
	public Image EngiPanel;
	public Text EngiText;

	public Dialogue dialogue;

	[Header("Navigation Panel")]
	public Console NavConsole;
	public Text NavigationConsoleText;
	public Image NavigationPanel;
	public Image NavigationShipImage;

	[Header("Security Console")]
	public int[] NumbersEntered;
	public int[] CorrectPassword;
	public int SafeIndex = -1;
	public Text[] SafeButtons;
	public Transform SafeDoor;

	public Console DoorConsole;
	public Text DoorText;
	public Image DoorPanel;
	public GameObject DoorLeft;
	public GameObject DoorRight;

	public LightFixture[] Lights;

	public float MoveSpeed;
	public float Angle;
	public Color PanelActive;
	public Color PanelActive2;
	public Color PanelInActive;

	public bool _navigationOn = false;
	public bool _isRotated = false;
	public bool _safeOpen = false;
	public bool _isPersonalLogLocked = true;
	public bool _isDoorLocked = true;
	public bool _isDoorOpen = false;

	public List<ICommandHandler> commandHandlers = new List<ICommandHandler>();

	public void Start() {
		ActivateDialog ("Start");

		commandHandlers.Insert(0, new Log3Handler(this));
		commandHandlers.Insert(0, new Log4Handler(this));
		commandHandlers.Add (new NavEnablerHandler (this));
		commandHandlers.Add (new HelpHandler (this));
		commandHandlers.Add (new DefaultHandler (this));


		EngiPanel.color = PanelInActive;
		EngiConsole.enabled = false;
	}

	public void FixedUpdate() {
//		WorldParent.transform.Translate (MoveSpeed * transform.forward);
	}
		
	public void SetGameProgress(int value) {

	}

	public void OpenDoor() {
		
		if (!_isDoorLocked && !_isDoorOpen) {
			SoundManager.Instance.PlayBeep2 ();
			_isDoorOpen = true;
			DoorLeft.transform.DOMove (new Vector3 (DoorLeft.transform.position.x + 1, DoorLeft.transform.position.y, DoorLeft.transform.position.z), 5f).SetEase(Ease.InCirc);
			DoorRight.transform.DOMove (new Vector3 (DoorRight.transform.position.x - 1, DoorRight.transform.position.y, DoorRight.transform.position.z), 5f).SetEase(Ease.InCirc);
		}
	}

	public void EnterSafeButton(int index) {

		if (!_safeOpen) {
			SoundManager.Instance.PlayBeep2 ();
			SafeIndex = SafeIndex + 1;
			
			if (SafeIndex > CorrectPassword.Length) {
			
				for (int i = 0; i < NumbersEntered.Length; i++) {
					NumbersEntered [i] = -1;
				}
			
				SafeIndex = -1;
				for (int i = 0; i < SafeButtons.Length; i++) {
					SafeButtons [i].color = Color.white;
				}
			
			} else {
			
				if (SafeIndex < NumbersEntered.Length) {
					NumbersEntered [SafeIndex] = index;
					
					SafeButtons [index-1].color = Color.gray;
					
					bool passed = true;
					
					for (int i = 0; i < CorrectPassword.Length; i++) {
						if (CorrectPassword [i] != NumbersEntered [i]) {
							passed = false;
						} 
					}
					
					if (passed) {
						SafeDoor.transform.DOMove (new Vector3 (SafeDoor.transform.position.x, -2f, SafeDoor.transform.position.z), 1f).SetEase(Ease.InCirc);
						_safeOpen = true;
						commandHandlers.Insert (0, new FinalHandler (this));
					}
				}
			}
		}

	}

	public void turnEngiOn() {	
		SoundManager.Instance.PlayBeep2 ();	
		EngiPanel.color = PanelActive;
		EngiConsole.enabled = true;
	}

	public void OnTurnRight() {		
		if (_isRotated && NavConsole.enabled) {
			SoundManager.Instance.PlayBeep2 ();
			WorldParent.transform.DORotate (new Vector3 (0, transform.localRotation.eulerAngles.y - 30, 0), 6f).OnComplete (TurnLightsOff);
			NavigationShipImage.transform.DORotate (new Vector3 (0, transform.localRotation.eulerAngles.y + 30, 0), 6f);
			PersonalLogConsole.enabled = false;
			_isRotated = false;
		}
	}

	public void OnTurnLeft() {

		if (!_isRotated && NavConsole.enabled) {
			SoundManager.Instance.PlayBeep2 ();
			WorldParent.transform.DORotate (new Vector3 (0, transform.localRotation.eulerAngles.y + 30, 0), 6f).OnComplete (TurnLightsOn);
			NavigationShipImage.transform.DORotate (new Vector3 (0, transform.localRotation.eulerAngles.y + 30, 0), 6f);
			_isRotated = true;
		}
		
	}

	public void OnEngiPanelEnter(InputField field) {
		SoundManager.Instance.PlayBeep2 ();
		var text = field.text;

		string[] split = text.Split (new char[]{' '}, System.StringSplitOptions.None);

		if (split.Length >= 2) {
			for (int i = 0; i < commandHandlers.Count; i++) {
				if (commandHandlers [i].RespondsToCommand (split [0]) && commandHandlers [i].RespondsToParam (split)) {
					commandHandlers [i].Do ();
					break;
				}
			}
		} else {
			SayEngi ("ERROR 0x00: Unrecognized command");
		}

		commandHandlers.RemoveAll ((i) => {
			return i.ShouldRemove ();
		});

		field.text = ">";

	}


	public void TurnLightsOff ()
	{
		for (int i = 0; i < Lights.Length; i++) {
			Lights [i].TurnOff ();
		}

		StartEngiText ("0x44: NAVIGATION SYSTEM OFFLINE");
		_navigationOn = false;
		NavigationConsoleText.text = "0x44: NAVIGATION SYSTEM OFFLINE";
		NavigationPanel.color = PanelInActive;
		NavConsole.enabled = false;
		NavigationConsoleText.color = Color.white;

		PersonalLogConsole.enabled = false;
		PersonalLogPanel.color = PanelInActive;

	}

	public void TurnLightsOn ()
	{
		for (int i = 0; i < Lights.Length; i++) {
			Lights [i].TurnOn ();
		}

		StartNavText ("SOLAR CELLS RECEIVING -> 100% OF NEEDED POWER");

		PersonalLogConsole.enabled = true;
		PersonalLogPanel.color = PanelActive2;

	}

	public void ActivateDialog(string dialog) {
		dialogue.Run (AiDialog, dialog);
	}		

	public void ClickPersonalLog(int index) {
		SoundManager.Instance.PlayBeep2 ();
		if (_isPersonalLogLocked && index != 5 && index != 6) {
			StartCoroutine (SayPersonalLog (Logs [index]));

			LogButtons [index].color = Color.gray;
		} else if (_isPersonalLogLocked) {
			StartCoroutine (SayPersonalLog ("ERROR 0xFA: ACCESS DENIED. BINARY LOCK ON LOG EXISTS")); 
		} else {
			StartCoroutine (SayPersonalLog (Logs [index])); 
			LogButtons [index].color = Color.gray;
		}
	}

	public void StartEngiText(string text) {
		StartCoroutine (SayEngi (text));
	}

	public void StartNavText(string text) {
		StartCoroutine (SayNav (text));
	}

	private IEnumerator SayNav(string text)
	{
		NavigationConsoleText.text = "";
		string textToScroll = text;
		const float timePerChar = .05f;
		float accumTime = 0f;
		int c = 0;
		while (!InputNext() && c < textToScroll.Length)
		{
			yield return null;
			accumTime += Time.deltaTime;
			while (accumTime > timePerChar)
			{
				accumTime -= timePerChar;
				if (c < textToScroll.Length)
					NavigationConsoleText.text += textToScroll[c];
				SoundManager.Instance.PlayBeep ();
				c++;
			}
		}
		NavigationConsoleText.text = textToScroll;

		while (InputNext()) yield return null;

		while (!InputNext()) yield return null;
	}

	private IEnumerator SayEngi(string text)
	{
		EngiText.text = "";
		string textToScroll = text;
		const float timePerChar = .05f;
		float accumTime = 0f;
		int c = 0;
		while (!InputNext() && c < textToScroll.Length)
		{
			yield return null;
			accumTime += Time.deltaTime;
			while (accumTime > timePerChar)
			{
				accumTime -= timePerChar;
				if (c < textToScroll.Length)
					EngiText.text += textToScroll[c];
				SoundManager.Instance.PlayBeep ();
				c++;
			}
		}
		EngiText.text = textToScroll;

		while (InputNext()) yield return null;

		while (!InputNext()) yield return null;
	}

	private IEnumerator SayPersonalLog(string text)
	{
		PersonalLogText.text = "";
		string textToScroll = text;
		const float timePerChar = .05f;
		float accumTime = 0f;
		int c = 0;
		while (!InputNext() && c < textToScroll.Length)
		{
			yield return null;
			accumTime += Time.deltaTime;
			while (accumTime > timePerChar)
			{
				accumTime -= timePerChar;
				if (c < textToScroll.Length)
					PersonalLogText.text += textToScroll[c];
				SoundManager.Instance.PlayBeep ();
				c++;
			}
		}
		PersonalLogText.text = textToScroll;

		while (InputNext()) yield return null;

		while (!InputNext()) yield return null;
	}

	public bool InputNext()
	{
		return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
	}


}


public class NavEnablerHandler : AbstractHandler, ICommandHandler {

	private bool _isOn = false;
	private string command;
	private string[] parms;

	public NavEnablerHandler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		this.command = command;
		return command.Equals ("SET", System.StringComparison.CurrentCultureIgnoreCase) || command.Equals ("GET", System.StringComparison.CurrentCultureIgnoreCase) || command.Equals ("LIST", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public bool RespondsToParam (string[] param)
	{
		parms = param;

		return param[1].Equals ("0x44", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public void Do ()
	{

		if (parms.Length == 3) {
			_isOn = parms [2].Equals ("ON", System.StringComparison.CurrentCultureIgnoreCase) ? true : false;
		}

		if (command.Equals ("SET", System.StringComparison.CurrentCultureIgnoreCase)) {
			if (_isOn) {
				_spaceShip.StartEngiText ("0x44: NAVIGATION SYSTEM ONLINE");
				_spaceShip._navigationOn = true;
				_spaceShip.NavigationConsoleText.text = "0x44: NAVIGATION SYSTEM ONLINE";
				_spaceShip.NavigationPanel.color = _spaceShip.PanelActive;
				_spaceShip.NavConsole.enabled = true;
				_spaceShip.NavigationConsoleText.color = Color.black;
			} else {
				_spaceShip.StartEngiText ("0x44: NAVIGATION SYSTEM OFFLINE");
				_spaceShip._navigationOn = false;
				_spaceShip.NavigationConsoleText.text = "0x44: NAVIGATION SYSTEM OFFLINE";
				_spaceShip.NavigationPanel.color = _spaceShip.PanelInActive;
				_spaceShip.NavConsole.enabled = false;
				_spaceShip.NavigationConsoleText.color = Color.white;
			}
		} else if (command.Equals ("GET", System.StringComparison.CurrentCultureIgnoreCase)) {
			_spaceShip.StartEngiText ("0x44 -> " + (_spaceShip._navigationOn ? "ON" : "OFF"));
		} else {
			_spaceShip.StartEngiText ("0x44 { Navigation system console power }");
		}


	}

	#endregion
}

public class Log3Handler : AbstractHandler, ICommandHandler {

	private string[] parms;

	public Log3Handler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return command.Equals ("SET", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public bool RespondsToParam (string[] param)
	{
		parms = param;
		return param [1].Equals ("LogLock", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public void Do ()
	{
		bool isOn = false;
		if (parms.Length == 3) {
			isOn = parms [2].Equals ("OFF", System.StringComparison.CurrentCultureIgnoreCase);
		} else {
			_spaceShip.StartEngiText ("ON/OFF required");
		}

		if (isOn) {
			_spaceShip.StartEngiText ("PERSONAL LOG LOCK REMOVED from following logs : [0x006, 0x007]");
			_spaceShip._isPersonalLogLocked = false;
			_spaceShip.commandHandlers [0].SetShouldRemove ();
		} 
	}

	#endregion
}

public class Log4Handler : AbstractHandler, ICommandHandler {

	private string[] parms;

	public Log4Handler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return command.Equals ("Run", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public bool RespondsToParam (string[] param)
	{
		parms = param;
		return param [1].Equals ("Logentry", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public void Do ()
	{		
		_spaceShip.StartEngiText ("xxæø xx: \n I'm having trouble sleeping. I keep having the same nightmare over and over. I'm on the ship and I awaken out of hibernation. The ship needs me to fix the so¬ar paƒ∂ls  ¶•ª˙£≈≈ª•£ø™  ª•™xxxxxxx ERROR: USER INPUT ERROR ");
	}

	#endregion
}






public class DefaultHandler : AbstractHandler, ICommandHandler {

	public DefaultHandler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return true;
	}

	public bool RespondsToParam (string[] param)
	{
		return true;
	}

	public void Do ()
	{
		_spaceShip.StartEngiText ("ERROR 0xFFE: BAD EXEC \n type 'help me' for commands");
	}

	#endregion
}

public class HelpHandler : AbstractHandler, ICommandHandler {

	public HelpHandler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return command.Equals ("Help", System.StringComparison.CurrentCultureIgnoreCase);
	}
	public bool RespondsToParam (string[] param)
	{
		return true;
	}
	public void Do ()
	{
		_spaceShip.StartEngiText ("HELP >>>>> \nRUN x - exec prgm at memory address x\nSET v ON/OFF - change state of var v\nGET v - See state of var v\nList x - Find variables at mem address x");
	}
		
	#endregion
}

public class Final2Handler : AbstractHandler, ICommandHandler {

	private string[] parms;

	public Final2Handler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return command.Equals ("SET", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public bool RespondsToParam (string[] param)
	{
		parms = param;
		return param [1].Equals ("B-BB1a", System.StringComparison.CurrentCultureIgnoreCase);
	}

	public void Do ()
	{
		bool isOn = false;
		if (parms.Length == 3) {
			isOn = parms [2].Equals ("OFF", System.StringComparison.CurrentCultureIgnoreCase);
		} else {
			_spaceShip.StartEngiText ("ON/OFF required");
		}

		if (isOn) {
			_spaceShip.StartEngiText ("Door Lock disengaged");
			_spaceShip._isDoorLocked = false;
			_spaceShip.DoorText.text = "OPEN";
			_spaceShip.DoorPanel.color = _spaceShip.PanelActive;
			_spaceShip.commandHandlers [0].SetShouldRemove ();
		} 
	}

	#endregion
}

public class FinalHandler : AbstractHandler, ICommandHandler {

	public FinalHandler (SpaceShip spaceship) : base (spaceship)
	{
	}

	#region ICommandHandler implementation
	public bool RespondsToCommand (string command)
	{
		return command.Equals ("Run", System.StringComparison.CurrentCultureIgnoreCase);
	}
	public bool RespondsToParam (string[] param)
	{
		return param[1].Equals ("0x99E", System.StringComparison.CurrentCultureIgnoreCase);
	}
	public void Do ()
	{
		if (_spaceShip._isRotated) {
			_spaceShip.StartEngiText ("ERROR 0xF5 : DOOR OVERRIDE ONLY Active during Low power events");
		} else {
			_spaceShip.StartEngiText ("ERROR 0xF5 : SET DOOR LOCK ENABLED. SET [DOORNUM] OFF to open");
			_spaceShip.commandHandlers [0].SetShouldRemove ();
			_spaceShip.commandHandlers.Insert (0, new Final2Handler (_spaceShip));

		}
	}

	#endregion
}


public abstract class AbstractHandler {

	protected SpaceShip _spaceShip;
	protected bool _shouldRemove;

	public void SetShouldRemove () {
		_shouldRemove = true;
	}

	public bool ShouldRemove() {
		return _shouldRemove;
	}

	public AbstractHandler(SpaceShip spaceship) {
		_spaceShip = spaceship;
	}

}

public interface ICommandHandler {
	bool RespondsToCommand (string command);
	bool RespondsToParam (string[] param);
	bool ShouldRemove();
	void SetShouldRemove ();
	void Do();
}
