using System;
using System.Collections.Generic;

public partial class UnicyclePart
{

	public string Name { get; set; }
	public string Model { get; set; }
	public PartType Type { get; set; }
	public bool IsDefault { get; set; }

	public override int GetHashCode()
	{
		return HashCode.Combine( Name, Model, Type, IsDefault );
	}

	// there should be 1 default for each part type
	public static List<UnicyclePart> All = new()
	{
		//
		new() { Type = PartType.Wheel, Name = "Dev Wheel", Model = "models/parts/wheels/dev_wheel", IsDefault = true },
		new() { Type = PartType.Wheel, Name = "Mini Dev Wheel", Model = "models/parts/wheels/dev_wheel_mini" },

		//
		new() { Type = PartType.Frame, Name = "Dev Frame", Model = "models/parts/frames/dev_frame", IsDefault = true },

		//
		new() { Type = PartType.Seat, Name = "Dev Seat", Model = "models/parts/seats/dev_seat", IsDefault = true },

		//
		new() { Type = PartType.Pedal, Name = "Dev Pedal", Model = "models/parts/pedals/dev_pedal", IsDefault = true },
	};

	public static void Add( UnicyclePart part )
	{
		All.Add( part );
	}

	// to json, from json, tool to create and modify parts

}

public enum PartType
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Trail
}

