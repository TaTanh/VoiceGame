# VoiceGame – 2D Voice-Controlled Platformer
Game cho CS106

A Unity 2D side-scrolling platformer where the player **flies upward by speaking into a microphone**.  
Louder voice = stronger lift. Silence = gravity pulls you down.

---

## Folder Structure

```
Assets/
  Scripts/
    Audio/   → VoiceInput.cs
    Player/  → PlayerController.cs
    Combat/  → WeaponController.cs, Projectile.cs
    Enemy/   → EnemyAI.cs, EnemyHealth.cs
    UI/      → DebugUI.cs
```

---

## Scene Assembly – Step by Step

### Prerequisites
- Unity 2022 LTS or newer (2D template)
- TextMeshPro package installed (Window → Package Manager)
- A working microphone connected

---

### STEP 1 – Project Layers

1. Open **Edit → Project Settings → Tags and Layers**.
2. Add a layer called **`Ground`** (e.g. User Layer 6).
3. Add tags **`Player`** and **`Projectile`** in the Tags section.

---

### STEP 2 – Create Platforms

1. **Create → 2D Object → Sprite → Square** for each platform.
2. Set the **Layer** of each platform to **`Ground`**.
3. Add a **BoxCollider2D** to each platform.
4. Scale / position them to build your level layout.

---

### STEP 3 – Create the Player

1. **Create → 2D Object → Sprite → Square** — rename it `Player`.
2. Tag it as **`Player`**.
3. Add the following components:
   | Component | Settings |
   |---|---|
   | **Rigidbody2D** | Gravity Scale = 3, Freeze Rotation Z = ✓ |
   | **BoxCollider2D** | Size it to the sprite |
   | **VoiceInput** | *(script)* |
   | **PlayerController** | *(script)* |
   | **WeaponController** | *(script)* |

4. **Create a child** of Player called **`FirePoint`**.
   - Position it at the sprite's right edge (e.g. X = 0.5, Y = 0).
   - Drag this child Transform into the **WeaponController → Fire Point** field.

5. In **PlayerController** Inspector, tune:
   - `movementSpeed` = 5
   - `voiceForceMultiplier` = 20
   - `voiceThreshold` = 0.05
   - `maxVerticalVelocity` = 10

6. In **VoiceInput** Inspector, tune:
   - `sensitivityMultiplier` = 100 *(raise if your mic is quiet)*
   - `noiseFloor` = 0.005

---

### STEP 4 – Create the Projectile Prefab

1. **Create → 2D Object → Sprite → Circle** — rename it `Projectile`.
2. Scale it small (e.g. X = 0.2, Y = 0.2). Tint it yellow/orange.
3. Tag it as **`Projectile`**.
4. Add these components:
   | Component | Settings |
   |---|---|
   | **Rigidbody2D** | Gravity Scale = 0, Collision Detection = Continuous |
   | **CircleCollider2D** | **Is Trigger = ✓** |
   | **Projectile** | *(script)* speed = 15, lifetime = 3, damage = 1 |

5. Drag the Projectile from the Hierarchy into **Assets** to make a **Prefab**.
6. Delete it from the scene.
7. Drag the Prefab into **WeaponController → Projectile Prefab** field on the Player.

---

### STEP 5 – Create the Enemy Prefab

1. **Create → 2D Object → Sprite → Square** — rename it `Enemy`. Tint it red.
2. Add these components:
   | Component | Settings |
   |---|---|
   | **Rigidbody2D** | Gravity Scale = 3, Freeze Rotation Z = ✓ |
   | **BoxCollider2D** | Sized to sprite, **Is Trigger = ✗** |
   | **EnemyAI** | patrolSpeed = 2, Ground Layer = *Ground* |
   | **EnemyHealth** | maxHealth = 3 |

3. In **EnemyAI**, set the **Ground Layer** mask to the `Ground` layer you created.
4. Place the enemy on top of a platform.
5. Drag it to **Assets** to make a **Prefab**, then you can duplicate it across the level.

---

### STEP 6 – Create the Debug UI

1. **Create → UI → Canvas** (Screen Space – Overlay).
2. Inside the Canvas, **Create → UI → Panel** — rename it `DebugPanel`.
   - Anchor it to the top-left corner; set Width = 300, Height = 120.
3. Inside DebugPanel, add **two TMP_Text** children:
   - `VolumeText`  — initial text: `Mic Volume: 0.000`
   - `ForceText`   — initial text: `Upward Force: 0.00 N`
4. (Optional) Add a **UI → Image** child named `VolumeBar`:
   - Image Type = **Filled**, Fill Method = **Horizontal**, Fill Origin = Left.
   - Set a bright colour so it's visible.
5. Add the **DebugUI** script to `DebugPanel`.
6. In the **DebugUI** Inspector:
   - **Player Controller** → drag the Player GameObject.
   - **Voice Input**       → drag the Player GameObject.
   - **Volume Label**      → drag `VolumeText`.
   - **Force Label**       → drag `ForceText`.
   - **Volume Bar**        → drag `VolumeBar` (optional).

---

### STEP 7 – Camera Setup

1. Select the **Main Camera**.
2. Set Projection = **Orthographic**, Size = 5.
3. For the prototype, you can set the camera to follow the player by adding a simple script:

```csharp
// CameraFollow.cs – attach to Main Camera
using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1, -10);
    void LateUpdate()
    {
        if (target) transform.position = target.position + offset;
    }
}
```
Drag the Player into **Camera Follow → Target**.

---

### STEP 8 – Physics Layer Collision Matrix

Open **Edit → Project Settings → Physics 2D → Layer Collision Matrix**.  
Make sure **Projectile** does NOT collide with **Player** (to prevent self-damage at close range).  
Use the Ignore Layer Collision approach: select the Projectile layer and uncheck the Player layer box.

> Alternatively, since the Projectile script already ignores objects tagged `"Player"`,  
> the default matrix works fine for the prototype.

---

### STEP 9 – Press Play and Test

| Action | Input |
|---|---|
| Move horizontally | A / D or ← / → |
| Fly upward | Speak or blow into your microphone |
| Fall | Stay silent |
| Shoot | Hold Left Mouse Button |

Watch the **DebugPanel** in the top-left corner:  
`Mic Volume` rises when you speak; `Upward Force` shows how hard you're being pushed up.

---

## Tuning Tips

| Goal | Parameter to adjust |
|---|---|
| Mic too sensitive (always flying) | Increase `VoiceInput.noiseFloor` or decrease `sensitivityMultiplier` |
| Mic not sensitive enough (barely moves) | Decrease `noiseFloor`, increase `sensitivityMultiplier` |
| Player rises too fast when shouting | Decrease `voiceForceMultiplier` or `maxVerticalVelocity` |
| Player falls too fast in silence | Decrease Rigidbody2D `Gravity Scale` on the Player |
| Enemy walks off platform | Increase `EnemyAI.edgeProbeOffset` |
| Enemy jitters at edges | Increase `EnemyAI.flipCooldown` |

---

## Script Reference

| Script | Component lives on | Purpose |
|---|---|---|
| `VoiceInput` | Player | Microphone → normalised volume [0..1] |
| `PlayerController` | Player | Keyboard movement + voice flight |
| `WeaponController` | Player | Left-click shooting |
| `Projectile` | Projectile Prefab | Travel + damage + lifetime |
| `EnemyAI` | Enemy Prefab | Patrol left/right on platforms |
| `EnemyHealth` | Enemy Prefab | HP tracking + death |
| `DebugUI` | UI Canvas Panel | Real-time volume + force display |

