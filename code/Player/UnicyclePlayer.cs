﻿using Facepunch.Customization;
using Sandbox;
using System;
using System.Collections.Generic;

internal partial class UnicyclePlayer : Sandbox.Player
{

    [Net]
    public AnimEntity Terry { get; set; }
    [Net]
    public UnicycleEntity Unicycle { get; set; }
    [Net]
    public List<Checkpoint> Checkpoints { get; set; } = new();

    public const float MaxRenderDistance = 300f;
    public const float RespawnDelay = 3f;

    private TimeSince timeSinceDied;
    private Clothing.Container clothing;
    private UfNametag nametag;
    private JumpIndicator jumpindicator;
    private Particles speedParticle;

    public override void Respawn()
    {
        base.Respawn();

        // todo: not sure I like this setup, might prefer it like CarEntity
        // so the player is actually a normal terry instead of an invisible entity w/ controller
        SetModel( "models/parts/seats/dev_seat.vmdl" );
        EnableDrawing = false;
        EnableAllCollisions = true;

        Unicycle = new UnicycleEntity();
        Unicycle.SetParent( this, null, Transform.Zero );

        Terry = new AnimEntity();
        //Terry = new AnimEntity( "models/citizen/citizen.vmdl" );
        //Terry.SetParent( this, null, Transform.Zero );
		//Terry.SetAnimGraph( "models/citizen_unicycle_frenzy.vanmgrph" );
		//Terry.EnableDrawing = false;

        CameraMode = new UnicycleCamera();
        Controller = new UnicycleController();

		if (!Client.IsBot)
			Animator = new UnicycleAnimator();

        SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -12, -12, 0 ), new Vector3( 12, 12, 64 ) );
        RemoveCollisionLayer( CollisionLayer.Solid );

        if ( clothing == null )
        {
            clothing = new();
            clothing.LoadFromClient( Client );
        }

        clothing.DressEntity( Terry );

        ResetMovement();
        GotoBestCheckpoint();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        nametag = new( this );
        speedParticle = Particles.Create( "particles/player/speed_lines.vpcf" );

        if ( IsLocalPawn )
            jumpindicator = new( this );
    }

    public override void OnKilled()
    {
        base.OnKilled();

        timeSinceDied = 0;

        EnableAllCollisions = false;
        EnableDrawing = false;

        CameraMode = new SpectateRagdollCamera();

        Unicycle?.Delete();
        Terry?.Delete();

        RagdollOnClient();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        crown?.Destroy();
        jumpindicator?.Delete();
    }

    public override void Simulate( Client cl )
    {
        if ( SpectateTarget.IsValid() ) return;

        if ( LifeState == LifeState.Alive )
        {
            var controller = GetActiveController();
            controller?.Simulate( cl, this, GetActiveAnimator() );

            if ( GetActiveController() == DevController )
            {
                ResetMovement();
                ResetTimer();
            }

            if ( IsServer && InputActions.Spray.Pressed() )
            {
                Spray();
            }
        }

        if ( LifeState == LifeState.Dead )
        {
            //if ( IsServer && timeSinceDied > RespawnDelay )
            //    Respawn();
        }

        if ( InputActions.RestartAtCheckpoint.Pressed() || InputActions.RestartCourse.Pressed() )
        {
            if ( LifeState != LifeState.Dead )
                AddRespawnOnClient();

            if ( InputActions.RestartCourse.Pressed() )
                ResetTimer();

            Fall( false );
            timeSinceDied = Math.Max( timeSinceDied, RespawnDelay - .5f );
        }
    }

    [Event.Frame]
    private void UpdateRenderAlpha()
    {
        if ( Local.Pawn == this ) return;
        if ( Local.Pawn == null ) return;
        if ( !Terry.IsValid() || !Unicycle.IsValid() ) return;

        var a = GetRenderAlpha();

        Terry.RenderColor = Terry.RenderColor.WithAlpha( a );

        foreach ( var child in Terry.Children )
        {
            if ( child is not ModelEntity m || !child.IsValid() ) continue;
            m.RenderColor = m.RenderColor.WithAlpha( a );
        }

        Unicycle.SetRenderAlphaOnAllParts( a );
    }

    private float targetSpeedParticle = 0;
    private float currentSpeedParticle = 0;
    [Event.Frame]
    private void UpdateSpeedParticle()
    {
        if ( speedParticle == null ) return;

        var spd = Math.Min( Velocity.Length, 800 );
        targetSpeedParticle = spd < 400 || Fallen ? 0 : (spd - 400) / 400f;

        var lerpSpd = targetSpeedParticle == 0 ? 6 : 1;

        currentSpeedParticle = currentSpeedParticle.LerpTo( targetSpeedParticle, Time.Delta * lerpSpd );
        speedParticle.SetPosition( 1, new Vector3( currentSpeedParticle, 0, 0 ) );
    }

    public float GetRenderAlpha()
    {
        var dist = Local.Pawn.Position.Distance( Position );
        var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
        a = Math.Max( a, .15f );
        a = Easing.EaseOut( a );

        return a;
    }

    [ServerCmd]
    public static void SetSpectateTargetOnServer( int entityId )
    {
        if ( !ConsoleSystem.Caller.IsValid() ) return;

        var caller = ConsoleSystem.Caller.Pawn as UnicyclePlayer;
        if ( !caller.IsValid() ) return;

        var ent = Entity.FindByIndex( entityId ) as UnicyclePlayer;
        if ( !ent.IsValid() ) ent = null;

        caller.SpectateTarget = ent == caller ? null : ent;
    }

    [ClientRpc]
    private void AddRespawnOnClient()
    {
        if ( !IsLocalPawn ) return;
        MapStats.Local.AddRespawn();
    }

    [ClientRpc]
    private void SetAchievementOnClient( string shortname, string map = null )
    {
        Achievement.Set( Global.GameIdent, Client.PlayerId, shortname, map );
    }

    private TimeSince timeSinceSpray;
    private void Spray()
    {
        if ( GroundEntity == null || Fallen ) return;
        if ( timeSinceSpray < 3f ) return;
        timeSinceSpray = 0;

        var sprayPart = Client.Components.Get<CustomizationComponent>().GetEquippedPart( PartType.Spray.ToString() );
        var mat = Material.Load( sprayPart.AssetPath );

        Decals.Place( mat, Position, Vector3.One * 50, Rotation.LookAt( Vector3.Up ) );
    }

}

