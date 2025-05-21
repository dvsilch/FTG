using System;

[Flags]
public enum ButtonMapping
{
    None,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    LeftUp = 1 << 4,
    LeftDown = 1 << 5,
    RightUp = 1 << 6,
    RightDown = 1 << 7,
    Punch = 1 << 8,
    Kick = 1 << 9,
    Block = 1 << 10,
    Stop = 1 << 11,
}