using Sandbox;

internal partial class ClientConfig : EntityComponent
{

	public UnicycleEnsemble Ensemble { get; set; }

	protected override void OnActivate()
	{
		base.OnActivate();

		Ensemble = UnicycleEnsemble.Default;
	}

}

