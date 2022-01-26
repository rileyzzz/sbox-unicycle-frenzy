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
		// wheels
		new() { Id = 0, Type = PartType.Wheel, Name = "Dev Wheel", Model = "models/parts/wheels/dev_wheel.vmdl", IsDefault = true },
		new() { Id = 2, Type = PartType.Wheel, Name = "Mini Dev Wheel", Model = "models/parts/wheels/dev_wheel_mini.vmdl" },
		new() { Id = 24, Type = PartType.Wheel, Name = "Dev Wheel Cyan", Model = "models/parts/wheels/colours/dev_wheel_cyan.vmdl", IsDefault = false },
		new() { Id = 36, Type = PartType.Wheel, Name = "Dev Wheel Blue", Model = "models/parts/wheels/colours/dev_wheel_blue.vmdl", IsDefault = false },
		new() { Id = 37, Type = PartType.Wheel, Name = "Dev Wheel Green", Model = "models/parts/wheels/colours/dev_wheel_green.vmdl", IsDefault = false },
		new() { Id = 38, Type = PartType.Wheel, Name = "Dev Wheel Orange", Model = "models/parts/wheels/colours/dev_wheel_orange.vmdl", IsDefault = false },
		new() { Id = 39, Type = PartType.Wheel, Name = "Dev Wheel Pink", Model = "models/parts/wheels/colours/dev_wheel_pink.vmdl", IsDefault = false },
		new() { Id = 40, Type = PartType.Wheel, Name = "Dev Wheel Purple", Model = "models/parts/wheels/colours/dev_wheel_purple.vmdl", IsDefault = false },
		new() { Id = 41, Type = PartType.Wheel, Name = "Dev Wheel Red", Model = "models/parts/wheels/colours/dev_wheel_red.vmdl", IsDefault = false },
		new() { Id = 42, Type = PartType.Wheel, Name = "Dev Wheel Teal", Model = "models/parts/wheels/colours/dev_wheel_teal.vmdl", IsDefault = false },
		new() { Id = 43, Type = PartType.Wheel, Name = "Dev Wheel Yellow", Model = "models/parts/wheels/colours/dev_wheel_yellow.vmdl", IsDefault = false },
		new() { Id = 44, Type = PartType.Wheel, Name = "Dev Wheel Black", Model = "models/parts/wheels/colours/dev_wheel_black.vmdl", IsDefault = false },
		new() { Id = 45, Type = PartType.Wheel, Name = "Dev Wheel White", Model = "models/parts/wheels/colours/dev_wheel_white.vmdl", IsDefault = false },

		// frames
		new() { Id = 3, Type = PartType.Frame, Name = "Dev Frame", Model = "models/parts/frames/dev_frame.vmdl", IsDefault = true },
		new() { Id = 25, Type = PartType.Frame, Name = "Dev Frame Cyan", Model = "models/parts/frames/colours/dev_frame_cyan.vmdl", IsDefault = false },
		new() { Id = 26, Type = PartType.Frame, Name = "Dev Frame Blue", Model = "models/parts/frames/colours/dev_frame_blue.vmdl", IsDefault = false },
		new() { Id = 27, Type = PartType.Frame, Name = "Dev Frame Green", Model = "models/parts/frames/colours/dev_frame_green.vmdl", IsDefault = false },
		new() { Id = 28, Type = PartType.Frame, Name = "Dev Frame Orange", Model = "models/parts/frames/colours/dev_frame_orange.vmdl", IsDefault = false },
		new() { Id = 29, Type = PartType.Frame, Name = "Dev Frame Pink", Model = "models/parts/frames/colours/dev_frame_pink.vmdl", IsDefault = false },
		new() { Id = 30, Type = PartType.Frame, Name = "Dev Frame Purple", Model = "models/parts/frames/colours/dev_frame_purple.vmdl", IsDefault = false },
		new() { Id = 31, Type = PartType.Frame, Name = "Dev Frame Red", Model = "models/parts/frames/colours/dev_frame_red.vmdl", IsDefault = false },
		new() { Id = 32, Type = PartType.Frame, Name = "Dev Frame Teal", Model = "models/parts/frames/colours/dev_frame_teal.vmdl", IsDefault = false },
		new() { Id = 33, Type = PartType.Frame, Name = "Dev Frame Yellow", Model = "models/parts/frames/colours/dev_frame_yellow.vmdl", IsDefault = false },
		new() { Id = 34, Type = PartType.Frame, Name = "Dev Frame Black", Model = "models/parts/frames/colours/dev_frame_black.vmdl", IsDefault = false },
		new() { Id = 35, Type = PartType.Frame, Name = "Dev Frame White", Model = "models/parts/frames/colours/dev_frame_white.vmdl", IsDefault = false },

		// seats
		new() { Id = 4, Type = PartType.Seat, Name = "Dev Seat", Model = "models/parts/seats/dev_seat.vmdl", IsDefault = true },

		// pedals
		new() { Id = 5, Type = PartType.Pedal, Name = "Dev Pedal", Model = "models/parts/pedals/dev_pedal.vmdl", IsDefault = true },

		// particles
		new() { Id = 6, Type = PartType.Trail, Name = "Default", Model = "particles/trails/default_trail.vpcf", IsDefault = true },
		new() { Id = 7, Type = PartType.Trail, Name = "Rainbow", Model = "particles/trails/rainbow_trail.vpcf", IsDefault = false },
		new() { Id = 8, Type = PartType.Trail, Name = "Fire", Model = "particles/trails/fire_trail.vpcf", IsDefault = false },
		new() { Id = 9, Type = PartType.Trail, Name = "Electric", Model = "particles/trails/electric_trail.vpcf", IsDefault = false },
		new() { Id = 10, Type = PartType.Trail, Name = "Dot", Model = "particles/trails/dot_trail.vpcf", IsDefault = false },
		new() { Id = 11, Type = PartType.Trail, Name = "Pixel", Model = "particles/trails/pixel_trail.vpcf", IsDefault = false },
		new() { Id = 12, Type = PartType.Trail, Name = "Pixel Fire", Model = "particles/trails/fire_pixel_trail.vpcf", IsDefault = false },
		new() { Id = 13, Type = PartType.Trail, Name = "Water", Model = "particles/trails/water_trail.vpcf", IsDefault = false },
		new() { Id = 14, Type = PartType.Trail, Name = "Shapes", Model = "particles/trails/Shapes_trail.vpcf", IsDefault = false },
		new() { Id = 15, Type = PartType.Trail, Name = "Binary", Model = "particles/trails/binary_trail.vpcf", IsDefault = false },
		new() { Id = 16, Type = PartType.Trail, Name = "Coin", Model = "particles/trails/coins_trail.vpcf", IsDefault = false },
		new() { Id = 17, Type = PartType.Trail, Name = "Trash", Model = "particles/trails/Trash_trail.vpcf", IsDefault = false },
		new() { Id = 18, Type = PartType.Trail, Name = "Miami", Model = "particles/trails/Miami_trail.vpcf", IsDefault = false },
		new() { Id = 19, Type = PartType.Trail, Name = "Clouds", Model = "particles/trails/cloud_trail.vpcf", IsDefault = false },
		new() { Id = 20, Type = PartType.Trail, Name = "Musical", Model = "particles/trails/music_trail.vpcf", IsDefault = false },
		new() { Id = 21, Type = PartType.Trail, Name = "Black Hole", Model = "particles/trails/blackhole_trail.vpcf", IsDefault = false },

		// sprays
		new() { Id = 22, Type = PartType.Spray, Name = "Facepunch", Model = "materials/sprays/spray_facepunch.vmat", IsDefault = true },
		new() { Id = 23, Type = PartType.Spray, Name = "Unicycle Frenzy", Model = "materials/sprays/spray_unicycle_frenzy.vmat", IsDefault = false },
		new() { Id = 46, Type = PartType.Spray, Name = "Arrow", Model = "materials/sprays/spray_arrow.vmat", IsDefault = false },
		new() { Id = 47, Type = PartType.Spray, Name = "Bomb", Model = "materials/sprays/spray_bomb.vmat", IsDefault = false },
		new() { Id = 48, Type = PartType.Spray, Name = "Checkpoint", Model = "materials/sprays/spray_checkpoint.vmat", IsDefault = false },
		new() { Id = 49, Type = PartType.Spray, Name = "Head", Model = "materials/sprays/spray_head.vmat", IsDefault = false },
		new() { Id = 50, Type = PartType.Spray, Name = "Stop", Model = "materials/sprays/spray_stop.vmat", IsDefault = false },
	};

}

public enum PartType
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Trail,
	Spray
}

