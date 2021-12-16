using Sandbox;
using Sandbox.UI;

[UseTemplate]
[NavigatorTarget("menu/controls")]
internal class ControlsTab : Panel
{

	public string BrakeBind => Input.GetKeyWithBinding( "+iv_duck" ) ?? "UNSET";
	public string LeftPedalBind => Input.GetKeyWithBinding( "+iv_attack" ) ?? "UNSET";
	public string RightPedalBind => Input.GetKeyWithBinding( "+iv_attack2" ) ?? "UNSET";
	public string JumpBind => Input.GetKeyWithBinding( "+iv_jump" ) ?? "UNSET";
	public string LeanBind => GetLeanBindString();
	public string RestartBind => Input.GetKeyWithBinding( "+iv_drop" ) ?? "UNSET";
	public string GoBackBind => Input.GetKeyWithBinding( "+iv_reload" ) ?? "UNSET";

	private string GetLeanBindString()
	{
		var fwd = Input.GetKeyWithBinding( "+iv_forward" ) ?? "UNSET";
		var left = Input.GetKeyWithBinding( "+iv_left" ) ?? "UNSET";
		var back = Input.GetKeyWithBinding( "+iv_back" ) ?? "UNSET";
		var right = Input.GetKeyWithBinding( "+iv_right" ) ?? "UNSET";

		return $"{fwd} {left} {back} {right}".ToUpper();
	}

}

