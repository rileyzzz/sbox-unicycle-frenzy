using System.Collections.Generic;

internal class UnicyclePart
{
	public string Name;
	public string Model;
	public PartType Type;

	public static List<UnicyclePart> All = new()
	{
		//
		new() { Type = PartType.Wheel, Name = "Dev Wheel", Model = "models/parts/wheels/dev_wheel" },
		new() { Type = PartType.Wheel, Name = "Mini Dev Wheel", Model = "models/parts/wheels/dev_wheel_mini" },

		//
		new() { Type = PartType.Frame, Name = "Dev Frame", Model = "models/parts/frames/dev_frame" },

		//
		new() { Type = PartType.Seat, Name = "Dev Seat", Model = "models/parts/frames/dev_seat" },

		//
		new() { Type = PartType.Pedal, Name = "Dev Pedal", Model = "models/parts/frames/dev_pedal" },
	};

	public static void Add( UnicyclePart part )
	{
		All.Add( part );
	}

	// to json, from json, tool to create and modify parts

}

internal enum PartType
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Trail
}

