using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
internal class NetworkInfo : Panel
{
	public string Generation => "Generation " + UnicycleFrenzy.Game.BotGeneration.ToString();

	public Panel Vision { get; set; }



	public string LeaderName => (BotManager.Leader == null || BotManager.Leader.Name == null) ? "None" : BotManager.Leader.Name;

	Texture VisionTexture;

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		
		VisionTexture = Texture.Create( UnicycleBot.VisionRange + 1, UnicycleBot.VisionRange + 1, ImageFormat.RGB888 )
			.WithDynamicUsage()
			.Finish();

		Vision.Style.BackgroundImage = VisionTexture;
		//Vision.Style.BackgroundSizeX = 128;
		//Vision.Style.BackgroundSizeY = 128;
	}

	public override void Tick()
	{
		base.Tick();

		Client leader = BotManager.Leader;
		if (leader != null && leader.Pawn is UnicyclePlayer player)
		{
			byte[] visionData = new byte[(UnicycleBot.VisionRange + 1) * (UnicycleBot.VisionRange + 1) * 3];
			int destIndex = 0;
			//for ( int i = 0; i < visionData.Length / 3; i++ )
			//{
			//	byte val = (byte)(Rand.Float() * 255.0f);
			//	visionData[destIndex++] = val;
			//	visionData[destIndex++] = val;
			//	visionData[destIndex++] = val;
			//}
			//Log.Info($"vision size {player.BotVision.Count}");
			for ( int i = 0; i < player.BotVision.Count; i++ )
			{
				byte val = (byte)(player.BotVision[i] * 255.0f);
				visionData[destIndex++] = val;
				visionData[destIndex++] = val;
				visionData[destIndex++] = val;
			}
			VisionTexture.Update(visionData);
		}
	}
}
