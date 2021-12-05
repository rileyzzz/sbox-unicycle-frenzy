using System;
using System.Collections.Generic;
using System.Linq;

public class UnicyclePart
{

	public int Id { get; set; }
	public string Name { get; set; }
	public string Model { get; set; }
	public PartType Type { get; set; }
	public bool IsDefault { get; set; }

	// todo: to json, from json, tool to create and modify parts
	// also need to increment id in the tool so it's always unique
	public static List<UnicyclePart> All = new()
	{
		//
		new() { Id = 0, Type = PartType.Wheel, Name = "Dev Wheel", Model = "models/parts/wheels/dev_wheel", IsDefault = true },
		new() { Id = 2, Type = PartType.Wheel, Name = "Mini Dev Wheel", Model = "models/parts/wheels/dev_wheel_mini" },

		//
		new() { Id = 3, Type = PartType.Frame, Name = "Dev Frame", Model = "models/parts/frames/dev_frame", IsDefault = true },

		//
		new() { Id = 4, Type = PartType.Seat, Name = "Dev Seat", Model = "models/parts/seats/dev_seat", IsDefault = true },

		//
		new() { Id = 5, Type = PartType.Pedal, Name = "Dev Pedal", Model = "models/parts/pedals/dev_pedal", IsDefault = true },
	};

}

public enum PartType
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Trail
}

