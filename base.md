ABYSS FRONTIER — DEMO DESIGN DOCUMENT
1. Game Overview

Tên game: Abyss Frontier
Genre: 2D Top-down dungeon crawler + light base building
Platform: PC
Mode: Singleplayer
Engine: Unity 2D

Core fantasy

Người chơi khám phá dungeon nhiều tầng để thu thập tài nguyên, đánh quái và dần khôi phục một khu trú ẩn nhỏ trên mặt đất.

Gameplay loop:

Enter dungeon
→ Fight monsters
→ Collect resources
→ Return base
→ Build/upgrade
→ Unlock next dungeon floor


2. Scope Demo 

Dungeon
3 tầng dungeon
mỗi tầng:
quái thường
1 mini boss / boss
Combat
di chuyển WASD
attack melee
enemy AI đơn giản
HP system
Loot
gold
stone
wood
crystal
Base
1 khu base nhỏ

Player có thể:

build House
build Farm Plot
build Forge
Story progression
tầng 1: dungeon only
clear boss tầng 1 → unlock surface/base
NPC
2 NPC:
Blacksmith
Farmer

3. Simplified Story
Background

Một thành phố cổ tên Eldhollow được xây quanh một vực sâu bí ẩn.

Sau một thảm họa, thành phố sụp đổ và chỉ còn lại dungeon bên dưới.

Opening

Người chơi tỉnh dậy trong tầng hầm với ký ức mơ hồ:

“Do not let the Abyss awaken.”

Ban đầu:

chỉ có kiếm gãy
khám phá dungeon
Progression
Chapter 1

Dungeon Floor 1

Goal:

đánh quái
lấy crystal
defeat boss

Reward:

mở thang máy cổ
Chapter 2

Unlock Surface

Người chơi lên mặt đất và phát hiện tàn tích.

Mở khóa:

base building
NPC
Chapter 3

Dungeon deeper

Người chơi xuống tầng tiếp theo để lấy resource hiếm nâng cấp base.

Demo kết thúc sau boss tầng 3.

Ending text:

“The deeper truth still awaits below...”

4. Features
Player
movement
attack
health
inventory đơn giản

Scripts:

PlayerController
PlayerCombat
PlayerHealth
InventorySystem
Enemy

Types:

Slime
chase player
Skeleton
melee attack
Boss
larger HP
simple attack pattern

Scripts:

EnemyAI
EnemyHealth
EnemyAttack
BossController
Dungeon System
Scene riêng hoặc tilemap rooms

Dungeon manager:

DungeonManager
RoomTrigger
FloorManager
Base Building (light)

Buildables:

House

cost:

wood x10
stone x5

effect:

cosmetic/demo unlock
Farm Plot

cost:

wood x5

effect:

generate food
Forge

cost:

stone x10
crystal x3

effect:

upgrade sword

Scripts:

BuildSystem
BuildingData
ResourceManager
UI

Need:

HP bar
inventory
resource count
build menu
dialogue box

Scripts:

UIManager
InventoryUI
BuildMenuUI
DialogueUI
5. Art Direction

Style:

pixel art 2D
dark fantasy
muted colors

Palette:

dark blue
gray
brown
green glow

Assets needed:

Character
idle
walk
attack
Enemy
slime
skeleton
boss
Environment
dungeon tiles
walls
rocks
trees
ruins
UI
health bar
buttons
inventory slots

6. Folder Structure
...

