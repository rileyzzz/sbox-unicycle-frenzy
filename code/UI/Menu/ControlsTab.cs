using Sandbox;
using Sandbox.UI;

[UseTemplate]
[NavigatorTarget("menu/controls")]
internal class ControlsTab : Panel
{

	public string BrakeBind => Input.GetButtonOrigin( InputButton.Duck ) ?? "UNSET";
	public string LeftPedalBind => Input.GetButtonOrigin( InputButton.Attack1 ) ?? "UNSET";
	public string RightPedalBind => Input.GetButtonOrigin( InputButton.Attack2 ) ?? "UNSET";
	public string JumpBind => Input.GetButtonOrigin( InputButton.Jump ) ?? "UNSET";
	public string LeanBind => GetLeanBindString();
	public string RestartBind => Input.GetButtonOrigin( InputButton.Drop ) ?? "UNSET";
	public string GoBackBind => Input.GetButtonOrigin( InputButton.Reload ) ?? "UNSET";
	public string SprayBind => Input.GetButtonOrigin( InputButton.Flashlight ) ?? "UNSET";

	private string GetLeanBindString()
	{
		var fwd = Input.GetButtonOrigin( InputButton.Forward ) ?? "UNSET";
		var left = Input.GetButtonOrigin( InputButton.Left ) ?? "UNSET";
		var back = Input.GetButtonOrigin( InputButton.Back ) ?? "UNSET";
		var right = Input.GetButtonOrigin( InputButton.Right ) ?? "UNSET";

		return $"{fwd} {left} {back} {right}".ToUpper();
	}

}

