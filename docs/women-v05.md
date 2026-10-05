# 0.5 - Five additional women footballers

Ada, Selin, Duru, Nehir and Yasmin extend the existing roster to 15. Existing ten FBX and Blender files are unchanged (SHA-256 verified). New hairstyles, faces and uniforms use the same stylized football design.

All five models are 1.95 m tall, with the same eight limb animation pivots. Existing CharacterController dimensions and all running, shooting, jumping, sliding, falling and recovery logic remain unchanged. Hair is attached to the head; no secondary hair simulation or new gameplay physics was introduced.

The lobby selector now has three rows of five. Duplicate uniform handling covers all 15 choices. Everyone must use the updated build when selecting the additional characters.

Validation: Unity-referenced C# compilation passed with zero errors/warnings; 50,625 four-player selection combinations and 16 partial lobbies passed. FBX import in Blender verified height and eight pivots for each new model. See women-validation.json and previews/KaosArena-5-Kadin-Karakter.png.

Unity batch build remains blocked by an inactive headless editor license. Actual Unity import, animation playback and multiplayer runtime verification remain pending. Open this project in Unity and use Kaos Arena > Build Windows Test. Existing EXE is not updated.

## Appearance refinement
The five added characters now have longer hairstyles, lipstick, blush and eyelashes, and a fitted jersey silhouette. Selin and Yasmin wear cosmetic block-heel shoes with soles on the original ground plane. All retain the same 1.95 m height and animation pivots. Gameplay code was not changed in this refinement. Hair is rigidly attached to the head, without secondary simulation. In-game clipping and heel animation still require Unity runtime inspection.
