#!/usr/bin/env python3
"""Inject floor boss prefab instances into Unity scenes."""

import random
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCENES = ROOT / "Assets/Scenes"

# prefab_guid, parent_fileID (0 = scene root), local position
FLOOR_SETUP = {
    "floor_1.unity": {
        "boss": "BossRat",
        "guid": "d1ed1acc1a6843758df3b9f216bc3a33",
        "parent": 1429661553,
        "pos": (2.5, 0.0, 0.0),
    },
    "floor_2.unity": {
        "boss": "BossSlime",
        "guid": "bffad672b1104541a1bfbb46df31e634",
        "parent": 650364097,
        "pos": (2.0, 0.0, 0.0),
    },
    "floor_3.unity": {
        "boss": "BossBat",
        "guid": "848b97a604174e588a97f3cd25388127",
        "parent": 0,
        "pos": (4.0, -1.0, 0.0),
    },
    "floor_4.unity": {
        "boss": "BossMimic",
        "guid": "86416d906abc4f3dadfe8fd0cbb3480b",
        "parent": 0,
        "pos": (5.0, 0.0, 0.0),
    },
    "floor_5.unity": {
        "boss": "BossMinotaur",
        "guid": "e207e145c13606d4c9684711f2d852b8",
        "parent": 0,
        "pos": (0.0, 0.0, 0.0),
    },
}

GO_ID = 100000
TRANSFORM_ID = 400000


def rand_id():
    return random.randint(900000000, 1999999999)


def make_prefab_block(cfg):
    inst_id = rand_id()
    strip_id = rand_id()
    boss = cfg["boss"]
    guid = cfg["guid"]
    parent = cfg["parent"]
    x, y, z = cfg["pos"]

    mods = [
        f"    - target: {{fileID: {GO_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_Name\n      value: {boss}\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalPosition.x\n      value: {x}\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalPosition.y\n      value: {y}\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalPosition.z\n      value: {z}\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalRotation.w\n      value: 1\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalRotation.x\n      value: 0\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalRotation.y\n      value: 0\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalRotation.z\n      value: 0\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalEulerAnglesHint.x\n      value: 0\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalEulerAnglesHint.y\n      value: 0\n      objectReference: {{fileID: 0}}",
        f"    - target: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n      propertyPath: m_LocalEulerAnglesHint.z\n      value: 0\n      objectReference: {{fileID: 0}}",
    ]

    block = (
        f"\n--- !u!1001 &{inst_id}\n"
        "PrefabInstance:\n"
        "  m_ObjectHideFlags: 0\n"
        "  serializedVersion: 2\n"
        "  m_Modification:\n"
        "    serializedVersion: 3\n"
        f"    m_TransformParent: {{fileID: {parent}}}\n"
        "    m_Modifications:\n"
        + "\n".join(mods)
        + "\n    m_RemovedComponents: []\n"
        "    m_RemovedGameObjects: []\n"
        "    m_AddedGameObjects: []\n"
        "    m_AddedComponents: []\n"
        f"  m_SourcePrefab: {{fileID: 100100000, guid: {guid}, type: 3}}\n"
        f"--- !u!4 &{strip_id} stripped\n"
        "Transform:\n"
        f"  m_CorrespondingSourceObject: {{fileID: {TRANSFORM_ID}, guid: {guid}, type: 3}}\n"
        f"  m_PrefabInstance: {{fileID: {inst_id}}}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
    )
    return block, inst_id, strip_id, parent


def inject(scene_name, cfg):
    path = SCENES / scene_name
    text = path.read_text(encoding="utf-8")

    if cfg["guid"] in text and cfg["boss"] in text:
        print(f"SKIP {scene_name}: {cfg['boss']} already present")
        return

    block, inst_id, strip_id, parent = make_prefab_block(cfg)
    text = text.rstrip() + block

    if parent != 0:
        # Add child reference to parent transform's m_Children list
        pattern = (
            rf"(--- !u!4 &{parent}\nTransform:.*?m_Children:\n)"
            r"((?:  - \{fileID: \d+\}\n)*)"
        )
        match = re.search(pattern, text, re.S)
        if match:
            children = match.group(2) + f"  - {{fileID: {strip_id}}}\n"
            text = text[: match.start(2)] + children + text[match.end(2) :]
        else:
            print(f"WARN: could not find parent {parent} children in {scene_name}")

    path.write_text(text + "\n", encoding="utf-8")
    print(f"Added {cfg['boss']} to {scene_name}")


def main():
    random.seed(42)
    for scene, cfg in FLOOR_SETUP.items():
        inject(scene, cfg)


if __name__ == "__main__":
    main()
