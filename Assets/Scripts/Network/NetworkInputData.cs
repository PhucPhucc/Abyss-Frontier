using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movement;
    public byte buttons;
    
    public const byte ATTACK_BIT = 1;
    public const byte SPRINT_BIT = 2;

    public bool IsAttackSet => (buttons & ATTACK_BIT) != 0;
    public bool IsSprintSet => (buttons & SPRINT_BIT) != 0;
}
