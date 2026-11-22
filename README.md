# Recoil Range

A small Unity sample about one thing: how much of "game feel" is a layer on top
of an action, and what the action looks like once that layer is removed.

There is one scene, one character, one shot and one switch. The switch is the
sample. Press it and the same shot stops feeling like a shot.

Play it in the browser: the built player lives in `docs/`, which is what GitHub
Pages serves, so the page for this repository is the sample itself. To run it
locally, serve that folder with any static web server and open it; opening the
file directly from disk will not work, a browser blocks the fetches.

## What the switch changes

The four values the layer is made of are printed on screen, and they go to zero
when the layer is off. They are not a mood, they are numbers:

| Value | Layer on | Layer off |
|---|---|---|
| Recoil push on the player | 0.69 units, measured in play | 0 |
| Hitstop on contact | 45 ms at 6 percent speed | none |
| Camera shake | 0.25 units for 180 ms, decaying | 0 |
| Target flash | 120 ms, fading out on a cubic ease | 0 |
| Target scale punch | 1.18x over 220 ms | 1.00x |
| Muzzle burst | 24 particles | none |
| Impact sparks | 20 particles at the contact point | none |
| Shot and hit sounds | both | silent |
| Tracer fade | 90 ms taper | one frame, no taper |

The shot still registers with the layer off: the tracer appears, the raycast
hits, the target counts the hit. Nothing about the mechanic changed. Only the
reading of it did.

## Controls

WASD to move, Q and E to turn, Space or left click to fire, Tab or the button
in the corner to flip the layer.

## How it is put together

- `Assets/Scripts/FeelSettings.cs` holds every tunable number in one component,
  so the feel can be retuned in the inspector while the game runs.
- `Assets/Scripts/PlayerRig.cs` drives movement and the shot. Movement sets
  horizontal velocity directly, so the recoil lives in its own decaying velocity
  term and is added on top; otherwise the movement code would erase the push in
  the same physics step.
- `Assets/Scripts/CameraRig.cs` follows the player and shakes with a decay, so a
  longer shake is not a louder shake.
- `Assets/Scripts/TargetBlock.cs` answers a hit: flash, scale punch, knockback.
- `Assets/Scripts/ShotAudio.cs` generates both sounds in code at startup, so
  the repository carries no sample packs and no licence questions: a shot is a
  noise burst over a short downward sweep, a hit is a low body with a click on
  its front, and both envelopes are as tunable as the rest of the feel.
- `Assets/Scripts/Hitstop.cs` slows time to a crawl for a few tens of
  milliseconds on contact, on unscaled time so it is not waiting inside the
  slowdown it caused.
- `Assets/Scripts/MicroTween.cs` is a small tween helper written for this
  sample. It is not DOTween and does not pretend to be: three easing curves and
  a coroutine are all this scene needs, and keeping it in the repository means
  the timings can be read next to the code that uses them.
- `Assets/Editor/SceneBuilder.cs` builds the whole scene from script, so it can
  be rebuilt from scratch with one command and reviewed as code rather than as
  a binary file.
- `Assets/Editor/BuildCommands.cs` runs the builds headlessly.

## Building it

Unity 6000.3.29f1. From the editor use the `Range` menu, or headlessly:

```
Unity -batchmode -nographics -quit -projectPath . -executeMethod SceneBuilder.Build
Unity -batchmode -nographics -quit -projectPath . -executeMethod BuildCommands.Web
```

Run from the `RecoilRange` folder. The WebGL command writes straight into
`docs/`. Measured on an M-series Mac: that build takes about 80 seconds and
produces 9.4 MB.

## One note on hosting WebGL

The WebGL build sets `PlayerSettings.WebGL.decompressionFallback = true`. Unity
compresses its output and expects the server to send `Content-Encoding: gzip`.
A plain static host does not, GitHub Pages included, and without the fallback
the page shows a loader error instead of the scene. The fallback unpacks in
JavaScript instead, which costs a little load time and works anywhere.

## What this sample is not

No multiplayer and no networking. No third party packages, no asset store
content and no sound files: the scene is primitives, the two sounds are
generated in code. It is a feel sample, not a game.
