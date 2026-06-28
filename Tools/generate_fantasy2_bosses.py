#!/usr/bin/env python3
"""Generate Fantasy 2 boss animations, controllers, stats assets, and prefabs."""

import os
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SPRITES = ROOT / "Assets/Sprites/Monsters Creatures Fantasy 2/Sprites"
ANIM_ROOT = ROOT / "Assets/Animations/Boss"
PREFAB_ROOT = ROOT / "Assets/Prefabs/Boss"
STATS_ROOT = ROOT / "Assets/ScriptableObjects/Enemies"
BOSS_CTRL_TEMPLATE = ROOT / "Assets/Animations/Boss/Boss.controller"
MINOTAUR_PREFAB = ROOT / "Assets/Prefabs/Boss/BossMinotaur.prefab"
ENEMY_HEALTH_GUID = "262d2a8427f8846848f479ce4c056396"
BOSS_CTRL_SCRIPT_GUID = "11110000222200003333000044440000"
MIMIC_CTRL_SCRIPT_GUID = "aabbccdd11223344556677889900aabb"
ENEMY_STATS_SCRIPT_GUID = "24ea95734c488a5fa9a1113acceca787"

SAMPLE_RATE = 12
FRAME_DT = 1.0 / SAMPLE_RATE

BOSSES = {
    "Rat": {
        "folder": "Rat",
        "controller_name": "Rat",
        "prefab_name": "BossRat",
        "stats_name": "BossRatStats",
        "display_name": "Rat Boss",
        "clips": {
            "Idle": ("idle.png", True),
            "Walk": ("run.png", True),
            "Attack": ("attack_bite.png", False),
            "Hurt": ("hurt.png", False),
            "Death": ("rat-death.png", False),
        },
        "stats": {"baseHP": 80, "baseATK": 8, "baseDEF": 3, "baseSpeed": 1.8, "baseExpReward": 40},
        "boss_params": {
            "moveSpeed": 1.8, "attackRange": 1.4, "attackDamage": 8,
            "attackHitDelay": 0.35, "attackAnimTailDuration": 0.5,
            "attackCooldown": 1.5, "attackAoERadius": 1.2, "introDuration": 1.0,
            "facingHitOffset": 0.5, "scale": 1.2,
        },
        "collider": {"offset": [0, 0.2], "size": [0.7, 0.5]},
        "use_mimic_controller": False,
    },
    "Slime2": {
        "folder": "Slime",
        "controller_name": "SlimeBoss",
        "prefab_name": "BossSlime",
        "stats_name": "BossSlimeStats",
        "display_name": "Slime Boss",
        "clips": {
            "Idle": ("idle.png", True),
            "Walk": ("walk.png", True),
            "Attack": ("attack.png", False),
            "Hurt": ("hurt.png", False),
            "Death": ("death.png", False),
        },
        "stats": {"baseHP": 120, "baseATK": 12, "baseDEF": 4, "baseSpeed": 1.5, "baseExpReward": 60},
        "boss_params": {
            "moveSpeed": 1.5, "attackRange": 1.5, "attackDamage": 12,
            "attackHitDelay": 0.4, "attackAnimTailDuration": 0.5,
            "attackCooldown": 1.6, "attackAoERadius": 1.4, "introDuration": 1.2,
            "facingHitOffset": 0.6, "scale": 1.3,
        },
        "collider": {"offset": [0, 0.25], "size": [0.9, 0.6]},
        "use_mimic_controller": False,
    },
    "Bat": {
        "folder": "Bat",
        "controller_name": "Bat",
        "prefab_name": "BossBat",
        "stats_name": "BossBatStats",
        "display_name": "Bat Boss",
        "clips": {
            "Idle": ("fly.png", True),
            "Walk": ("fly.png", True),
            "Attack": ("attack.png", False),
            "Hurt": ("hurt.png", False),
            "Death": ("death.png", False),
        },
        "stats": {"baseHP": 160, "baseATK": 15, "baseDEF": 5, "baseSpeed": 2.0, "baseExpReward": 80},
        "boss_params": {
            "moveSpeed": 2.0, "attackRange": 1.6, "attackDamage": 15,
            "attackHitDelay": 0.35, "attackAnimTailDuration": 0.45,
            "attackCooldown": 1.5, "attackAoERadius": 1.5, "introDuration": 1.2,
            "facingHitOffset": 0.55, "scale": 1.2,
        },
        "collider": {"offset": [0, 0.2], "size": [0.8, 0.5]},
        "use_mimic_controller": False,
    },
    "Mimic": {
        "folder": "Mimic",
        "controller_name": "Mimic",
        "prefab_name": "BossMimic",
        "stats_name": "BossMimicStats",
        "display_name": "Mimic Boss",
        "clips": {
            "IdleClosed": ("Idle_closed.png", True),
            "Opening": ("opening.png", False),
            "Transform": ("transform.png", False),
            "Idle": ("idle_transformed.png", True),
            "Walk": ("walk.png", True),
            "Attack": ("attack_1.png", False),
            "Attack2": ("attack_2.png", False),
            "Hurt": ("hurt.png", False),
            "Death": ("death.png", False),
        },
        "stats": {"baseHP": 220, "baseATK": 18, "baseDEF": 6, "baseSpeed": 1.4, "baseExpReward": 100},
        "boss_params": {
            "moveSpeed": 1.4, "attackRange": 1.7, "attackDamage": 18,
            "attackHitDelay": 0.45, "attackAnimTailDuration": 0.55,
            "attackCooldown": 1.7, "attackAoERadius": 1.6, "introDuration": 0,
            "facingHitOffset": 0.65, "scale": 1.3,
        },
        "collider": {"offset": [0, 0.3], "size": [0.9, 0.7]},
        "use_mimic_controller": True,
    },
}


def new_guid():
    return uuid.uuid4().hex


def parse_sprites(meta_path: Path):
    text = meta_path.read_text(encoding="utf-8")
    tex_guid = re.search(r"^guid: (.+)$", text, re.M).group(1)
    sprites = []
    for block in re.finditer(
        r"name: (\S+)\n(?:.*\n)*?      internalID: (-?\d+)",
        text,
    ):
        sprites.append((block.group(1), int(block.group(2)), tex_guid))
    return sprites


def make_anim_clip(name: str, sprites, loop: bool) -> str:
    lines = []
    for i, (sname, internal_id, tex_guid) in enumerate(sprites):
        t = round(i * FRAME_DT, 7)
        lines.append(
            f"    - time: {t}\n"
            f"      value: {{fileID: {internal_id}, guid: {tex_guid}, type: 3}}"
        )
    stop_time = round((len(sprites) - 1) * FRAME_DT + (FRAME_DT if loop else FRAME_DT), 7)
    if len(sprites) == 1:
        stop_time = FRAME_DT

    mapping = "\n".join(
        f"    - {{fileID: {iid}, guid: {g}, type: 3}}"
        for _, iid, g in sprites
    )
    curve = "\n".join(lines)

    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - curve:
{curve}
    attribute: m_Sprite
    path: 
    classID: 212
    script: {{fileID: 0}}
  m_SampleRate: {SAMPLE_RATE}
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {{fileID: 0}}
      typeID: 212
      customType: 23
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
{mapping}
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {stop_time}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: {1 if loop else 0}
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []
"""


def write_meta(path: Path, guid: str, importer="NativeFormatImporter"):
    path.with_suffix(path.suffix + ".meta").write_text(
        f"""fileFormatVersion: 2
guid: {guid}
{importer}:
  externalObjects: {{}}
  mainObjectFileID: 7400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
    )


def write_controller_meta(path: Path, guid: str):
    path.with_suffix(path.suffix + ".meta").write_text(
        f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 9100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
    )


def make_standard_controller(ctrl_name: str, clip_guids: dict) -> str:
    idle = clip_guids["Idle"]
    walk = clip_guids["Walk"]
    attack = clip_guids["Attack"]
    death = clip_guids.get("Death", idle)

    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {ctrl_name}
  serializedVersion: 5
  m_AnimatorParameters:
  - m_Name: isMoving
    m_Type: 4
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}
  - m_Name: attack
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}
  - m_Name: hurt
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}
  - m_Name: die
    m_Type: 9
    m_DefaultFloat: 0
    m_DefaultInt: 0
    m_DefaultBool: 0
    m_Controller: {{fileID: 9100000}}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {{fileID: 5678901234567890123}}
    m_Mask: {{fileID: 0}}
    m_Motions: []
    m_Behaviours: []
    m_BlendingMode: 0
    m_SyncedLayerIndex: -1
    m_DefaultWeight: 0
    m_IKPass: 0
    m_SyncedLayerAffectsTiming: 0
    m_Controller: {{fileID: 9100000}}
--- !u!1107 &5678901234567890123
AnimatorStateMachine:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Base Layer
  m_ChildStates:
  - serializedVersion: 1
    m_State: {{fileID: 1111111111111111111}}
    m_Position: {{x: 250, y: 50, z: 0}}
  - serializedVersion: 1
    m_State: {{fileID: 2222222222222222222}}
    m_Position: {{x: 500, y: 50, z: 0}}
  - serializedVersion: 1
    m_State: {{fileID: 3333333333333333333}}
    m_Position: {{x: 250, y: -80, z: 0}}
  - serializedVersion: 1
    m_State: {{fileID: 5555555555555555555}}
    m_Position: {{x: 50, y: -160, z: 0}}
  m_ChildStateMachines: []
  m_AnyStateTransitions:
  - {{fileID: 6666666666666666661}}
  - {{fileID: 6666666666666666663}}
  m_EntryTransitions: []
  m_StateMachineTransitions: {{}}
  m_StateMachineBehaviours: []
  m_AnyStatePosition: {{x: 50, y: -60, z: 0}}
  m_EntryPosition: {{x: 50, y: 50, z: 0}}
  m_ExitPosition: {{x: 800, y: 50, z: 0}}
  m_ParentStateMachinePosition: {{x: 800, y: 20, z: 0}}
  m_DefaultState: {{fileID: 1111111111111111111}}
--- !u!1102 &1111111111111111111
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_Name: Idle
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {{fileID: 7777777777777777771}}
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {idle}, type: 2}}
  m_Tag: 
--- !u!1102 &2222222222222222222
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_Name: Walk
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {{fileID: 7777777777777777772}}
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {walk}, type: 2}}
  m_Tag: 
--- !u!1102 &3333333333333333333
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_Name: Attack
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions:
  - {{fileID: 7777777777777777773}}
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {attack}, type: 2}}
  m_Tag: 
--- !u!1102 &5555555555555555555
AnimatorState:
  serializedVersion: 6
  m_ObjectHideFlags: 1
  m_Name: Death
  m_Speed: 1
  m_CycleOffset: 0
  m_Transitions: []
  m_StateMachineBehaviours: []
  m_Position: {{x: 50, y: 50, z: 0}}
  m_IKOnFeet: 0
  m_WriteDefaultValues: 1
  m_Mirror: 0
  m_SpeedParameterActive: 0
  m_MirrorParameterActive: 0
  m_CycleOffsetParameterActive: 0
  m_TimeParameterActive: 0
  m_Motion: {{fileID: 7400000, guid: {death}, type: 2}}
  m_Tag: 
--- !u!1101 &7777777777777777771
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: isMoving
    m_EventTreshold: 0
  m_DstState: {{fileID: 2222222222222222222}}
  m_TransitionDuration: 0.1
  m_HasExitTime: 0
--- !u!1101 &7777777777777777772
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_Conditions:
  - m_ConditionMode: 2
    m_ConditionEvent: isMoving
    m_EventTreshold: 0
  m_DstState: {{fileID: 1111111111111111111}}
  m_TransitionDuration: 0.1
  m_HasExitTime: 0
--- !u!1101 &7777777777777777773
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_Conditions: []
  m_DstState: {{fileID: 1111111111111111111}}
  m_TransitionDuration: 0.1
  m_ExitTime: 0.95
  m_HasExitTime: 1
--- !u!1101 &6666666666666666661
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: attack
    m_EventTreshold: 0
  m_DstState: {{fileID: 3333333333333333333}}
  m_TransitionDuration: 0.1
  m_HasExitTime: 0
  m_CanTransitionToSelf: 0
--- !u!1101 &6666666666666666663
AnimatorStateTransition:
  m_ObjectHideFlags: 1
  m_Conditions:
  - m_ConditionMode: 1
    m_ConditionEvent: die
    m_EventTreshold: 0
  m_DstState: {{fileID: 5555555555555555555}}
  m_TransitionDuration: 0.1
  m_HasExitTime: 0
  m_CanTransitionToSelf: 0
"""


def make_mimic_controller(ctrl_name: str, clip_guids: dict) -> str:
    # States: IdleClosed (default), Opening, Transform, Idle, Walk, Attack, Attack2, Death
    g = clip_guids
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!91 &9100000
AnimatorController:
  m_ObjectHideFlags: 0
  m_Name: {ctrl_name}
  serializedVersion: 5
  m_AnimatorParameters:
  - {{m_Name: isMoving, m_Type: 4, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: attack, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: attack2, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: opening, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: transform, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: hurt, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  - {{m_Name: die, m_Type: 9, m_DefaultBool: 0, m_Controller: {{fileID: 9100000}}}}
  m_AnimatorLayers:
  - serializedVersion: 5
    m_Name: Base Layer
    m_StateMachine: {{fileID: 5678901234567890123}}
    m_DefaultWeight: 0
    m_Controller: {{fileID: 9100000}}
--- !u!1107 &5678901234567890123
AnimatorStateMachine:
  serializedVersion: 6
  m_Name: Base Layer
  m_ChildStates:
  - {{serializedVersion: 1, m_State: {{fileID: 1001}}, m_Position: {{x: 250, y: 0, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1002}}, m_Position: {{x: 250, y: 80, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1003}}, m_Position: {{x: 250, y: 160, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1004}}, m_Position: {{x: 500, y: 0, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1005}}, m_Position: {{x: 750, y: 0, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1006}}, m_Position: {{x: 500, y: -80, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1007}}, m_Position: {{x: 500, y: -160, z: 0}}}}
  - {{serializedVersion: 1, m_State: {{fileID: 1008}}, m_Position: {{x: 50, y: -80, z: 0}}}}
  m_AnyStateTransitions:
  - {{fileID: 2001}}
  - {{fileID: 2002}}
  - {{fileID: 2003}}
  - {{fileID: 2004}}
  m_DefaultState: {{fileID: 1001}}
--- !u!1102 &1001
AnimatorState:
  m_Name: IdleClosed
  m_Motion: {{fileID: 7400000, guid: {g['IdleClosed']}, type: 2}}
--- !u!1102 &1002
AnimatorState:
  m_Name: Opening
  m_Motion: {{fileID: 7400000, guid: {g['Opening']}, type: 2}}
--- !u!1102 &1003
AnimatorState:
  m_Name: Transform
  m_Motion: {{fileID: 7400000, guid: {g['Transform']}, type: 2}}
--- !u!1102 &1004
AnimatorState:
  m_Name: Idle
  m_Transitions:
  - {{fileID: 3001}}
  m_Motion: {{fileID: 7400000, guid: {g['Idle']}, type: 2}}
--- !u!1102 &1005
AnimatorState:
  m_Name: Walk
  m_Transitions:
  - {{fileID: 3002}}
  m_Motion: {{fileID: 7400000, guid: {g['Walk']}, type: 2}}
--- !u!1102 &1006
AnimatorState:
  m_Name: Attack
  m_Transitions:
  - {{fileID: 3003}}
  m_Motion: {{fileID: 7400000, guid: {g['Attack']}, type: 2}}
--- !u!1102 &1007
AnimatorState:
  m_Name: Attack2
  m_Transitions:
  - {{fileID: 3004}}
  m_Motion: {{fileID: 7400000, guid: {g['Attack2']}, type: 2}}
--- !u!1102 &1008
AnimatorState:
  m_Name: Death
  m_Motion: {{fileID: 7400000, guid: {g['Death']}, type: 2}}
--- !u!1101 &2001
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 1, m_ConditionEvent: opening}}
  m_DstState: {{fileID: 1002}}
  m_HasExitTime: 0
--- !u!1101 &2002
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 1, m_ConditionEvent: attack}}
  m_DstState: {{fileID: 1006}}
  m_HasExitTime: 0
  m_CanTransitionToSelf: 0
--- !u!1101 &2003
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 1, m_ConditionEvent: attack2}}
  m_DstState: {{fileID: 1007}}
  m_HasExitTime: 0
  m_CanTransitionToSelf: 0
--- !u!1101 &2004
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 1, m_ConditionEvent: die}}
  m_DstState: {{fileID: 1008}}
  m_HasExitTime: 0
  m_CanTransitionToSelf: 0
--- !u!1101 &3001
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 1, m_ConditionEvent: isMoving}}
  m_DstState: {{fileID: 1005}}
  m_HasExitTime: 0
--- !u!1101 &3002
AnimatorStateTransition:
  m_Conditions:
  - {{m_ConditionMode: 2, m_ConditionEvent: isMoving}}
  m_DstState: {{fileID: 1004}}
  m_HasExitTime: 0
--- !u!1101 &3003
AnimatorStateTransition:
  m_Conditions: []
  m_DstState: {{fileID: 1004}}
  m_ExitTime: 0.95
  m_HasExitTime: 1
--- !u!1101 &3004
AnimatorStateTransition:
  m_Conditions: []
  m_DstState: {{fileID: 1004}}
  m_ExitTime: 0.95
  m_HasExitTime: 1
"""


def make_stats_asset(name: str, stats: dict, stats_guid: str) -> str:
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {ENEMY_STATS_SCRIPT_GUID}, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
  enemyName: {name.replace('Stats', '')}
  enemyType: 7
  baseHP: {stats['baseHP']}
  baseATK: {stats['baseATK']}
  baseDEF: {stats['baseDEF']}
  baseSpeed: {stats['baseSpeed']}
  baseExpReward: {stats['baseExpReward']}
  hpScale: 1.5
  atkScale: 1.4
  defScale: 1.3
  speedScale: 1.1
  expScale: 1.7
"""


def get_first_sprite_ref(folder: str, png: str):
    meta_path = SPRITES / folder / png
    if not str(meta_path).endswith(".meta"):
        meta_path = Path(str(meta_path) + ".meta")
    sprites = parse_sprites(meta_path)
    _, iid, guid = sprites[0]
    return iid, guid


def make_prefab(cfg, ctrl_guid, stats_guid, anim_folder_key):
    folder = cfg["folder"]
    idle_png = cfg["clips"]["Idle"][0] if "Idle" in cfg["clips"] else cfg["clips"]["IdleClosed"][0]
    sprite_id, sprite_guid = get_first_sprite_ref(folder, idle_png)
    bp = cfg["boss_params"]
    col = cfg["collider"]
    scale = bp["scale"]
    script_guid = MIMIC_CTRL_SCRIPT_GUID if cfg["use_mimic_controller"] else BOSS_CTRL_SCRIPT_GUID
    script_class = "MimicBossController" if cfg["use_mimic_controller"] else "BossController"

    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100000
GameObject:
  m_Component:
  - component: {{fileID: 400000}}
  - component: {{fileID: 21200000}}
  - component: {{fileID: 9500000}}
  - component: {{fileID: 5000000}}
  - component: {{fileID: 6100000}}
  - component: {{fileID: 11400001}}
  - component: {{fileID: 11400002}}
  m_Layer: 3
  m_Name: {cfg['prefab_name']}
  m_TagString: Enemy
--- !u!4 &400000
Transform:
  m_GameObject: {{fileID: 100000}}
  m_LocalScale: {{x: {scale}, y: {scale}, z: 1}}
--- !u!212 &21200000
SpriteRenderer:
  m_GameObject: {{fileID: 100000}}
  m_Sprite: {{fileID: {sprite_id}, guid: {sprite_guid}, type: 3}}
  m_SortingLayerID: 769353063
  m_SortingLayer: 5
--- !u!95 &9500000
Animator:
  m_GameObject: {{fileID: 100000}}
  m_Controller: {{fileID: 9100000, guid: {ctrl_guid}, type: 2}}
--- !u!50 &5000000
Rigidbody2D:
  m_GameObject: {{fileID: 100000}}
  m_BodyType: 0
  m_GravityScale: 0
  m_Constraints: 4
  m_Mass: 8
--- !u!61 &6100000
BoxCollider2D:
  m_GameObject: {{fileID: 100000}}
  m_Offset: {{x: {col['offset'][0]}, y: {col['offset'][1]}}}
  m_Size: {{x: {col['size'][0]}, y: {col['size'][1]}}}
--- !u!114 &11400001
MonoBehaviour:
  m_GameObject: {{fileID: 100000}}
  m_Script: {{fileID: 11500000, guid: {ENEMY_HEALTH_GUID}, type: 3}}
  m_EditorClassIdentifier: Assembly-CSharp::EnemyHealth
  enemyLevel: 1
  statsDefinition: {{fileID: 11400000, guid: {stats_guid}, type: 2}}
  maxHealth: {cfg['stats']['baseHP']}
  destroyDelay: 2
  defaultStunDuration: 0
--- !u!114 &11400002
MonoBehaviour:
  m_GameObject: {{fileID: 100000}}
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_EditorClassIdentifier: Assembly-CSharp::{script_class}
  bossDisplayName: {cfg['display_name']}
  triggerVictoryOnDeath: 0
  introDuration: {bp['introDuration']}
  facingHitOffset: {bp['facingHitOffset']}
  spriteFacesLeftByDefault: 1
  moveSpeed: {bp['moveSpeed']}
  attackRange: {bp['attackRange']}
  attackDamage: {bp['attackDamage']}
  attackHitDelay: {bp['attackHitDelay']}
  attackAnimTailDuration: {bp['attackAnimTailDuration']}
  attackCooldown: {bp['attackCooldown']}
  attackAoERadius: {bp['attackAoERadius']}
  playerLayer:
    serializedVersion: 2
    m_Bits: 64
"""


def main():
    generated = {}
    for key, cfg in BOSSES.items():
        anim_dir = ANIM_ROOT / key
        anim_dir.mkdir(parents=True, exist_ok=True)
        clip_guids = {}

        for clip_name, (png, loop) in cfg["clips"].items():
            meta = SPRITES / cfg["folder"] / f"{png}.meta"
            sprites = parse_sprites(meta)
            clip_path = anim_dir / f"{clip_name}.anim"
            clip_guid = new_guid()
            clip_path.write_text(make_anim_clip(clip_name, sprites, loop), encoding="utf-8")
            write_meta(clip_path, clip_guid)
            clip_guids[clip_name] = clip_guid
            print(f"  clip {clip_name}: {clip_path}")

        ctrl_path = anim_dir / f"{cfg['controller_name']}.controller"
        ctrl_guid = new_guid()
        if cfg["use_mimic_controller"]:
            ctrl_text = make_mimic_controller(cfg["controller_name"], clip_guids)
        else:
            ctrl_text = make_standard_controller(cfg["controller_name"], clip_guids)
        ctrl_path.write_text(ctrl_text, encoding="utf-8")
        write_controller_meta(ctrl_path, ctrl_guid)

        stats_path = STATS_ROOT / f"{cfg['stats_name']}.asset"
        stats_guid = new_guid()
        stats_path.write_text(make_stats_asset(cfg["stats_name"], cfg["stats"], stats_guid), encoding="utf-8")
        stats_path.with_suffix(".asset.meta").write_text(
            f"fileFormatVersion: 2\nguid: {stats_guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n",
            encoding="utf-8",
        )

        prefab_path = PREFAB_ROOT / f"{cfg['prefab_name']}.prefab"
        prefab_guid = new_guid()
        prefab_path.write_text(make_prefab(cfg, ctrl_guid, stats_guid, key), encoding="utf-8")
        prefab_path.with_suffix(".prefab.meta").write_text(
            f"fileFormatVersion: 2\nguid: {prefab_guid}\nPrefabImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 100000\n",
            encoding="utf-8",
        )

        generated[cfg["prefab_name"]] = prefab_guid
        print(f"Boss {cfg['prefab_name']} -> {prefab_guid}")

    # Update Minotaur prefab
    mino = MINOTAUR_PREFAB.read_text(encoding="utf-8")
    if "triggerVictoryOnDeath" not in mino:
        mino = mino.replace(
            "  m_EditorClassIdentifier: Assembly-CSharp::BossController\n  moveSpeed:",
            "  m_EditorClassIdentifier: Assembly-CSharp::BossController\n  bossDisplayName: Minotaur\n  triggerVictoryOnDeath: 1\n  introDuration: 1.5\n  facingHitOffset: 0.8\n  spriteFacesLeftByDefault: 1\n  moveSpeed:",
        )
        MINOTAUR_PREFAB.write_text(mino, encoding="utf-8")
        print("Updated BossMinotaur.prefab")

    print("GENERATED_PREFAB_GUIDS", generated)


if __name__ == "__main__":
    main()
