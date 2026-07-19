using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movement;
    public byte buttons;
    
    public const byte ATTACK_BIT    = 1;
    public const byte SPRINT_BIT    = 2;
    public const byte DODGE_BIT     = 4;
    public const byte INTERACT_BIT  = 8;

    public bool IsAttackSet   => (buttons & ATTACK_BIT)   != 0;
    public bool IsSprintSet   => (buttons & SPRINT_BIT)   != 0;
    public bool IsDodgeSet    => (buttons & DODGE_BIT)    != 0;
    public bool IsInteractSet => (buttons & INTERACT_BIT) != 0;
}
