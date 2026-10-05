# Beach court - second selectable arena

Choose SAHIL / KUM in the main menu before creating a lobby or bot practice. Host arenaId is replicated to clients. All players must use the new build (beach-v1 wire protocol).

Shared octagonal sand court, four goals and rebound barriers keep existing gameplay geometry and physics. Sand texture is visual; player speed, jumping, shooting, ball friction and bounce remain identical. Animated sea is on the east side (positive X); shoreline, palms and benches sit outside the court. Wave bursts periodically accelerate the crests and launch cosmetic droplets above the barrier toward sea-side players. Droplets use gravity but do not apply forces, stun players or alter scores.

Validation: C# compilation against Unity libraries passed. Source comparison verified unchanged FixedUpdate, CheckSlideHits, ShotVelocity, Goal, ResetBall, Wall and BuildGoalNet. Existing match smoke test can run with -arenaSmoke -beach, including floor and cosmetic-collider checks. Capture mode supports -arenaCapture -beach.

Actual Unity build and physics play test are pending: headless editor license is inactive. Existing Windows EXE does not contain this arena. Open Unity and choose Kaos Arena > Build Windows Test, then run the new binary with -arenaSmoke -beach. Four-machine multiplayer verification is also pending.

Sahil-Saha-Genel.png and Sahil-Deniz-Yandan.png are Blender layout previews based on court geometry and model assets, not screenshots from Unity gameplay. Runtime wave and spray appearance may differ.

## Coastal daylight update
Procedural atmospheric sky with a visible sun, soft cloud clusters, warmer direct sunlight, blue ambient daylight and lighter haze. Palm count increased from 5 to 16 in two rows outside the court. Returning to the neighborhood restores its original sky, fog and lighting. BeachSky material is retained by the Unity build helper. C# compilation passed; actual Unity runtime and visuals remain pending due to the existing license issue. Blender previews include their own atmospheric sky approximation.

## Beach festival scenery
Added 28 cosmetic supporters with looping cheering-arm movement, 8 striped umbrellas, 16 loungers, 8 towels, wooden spectator benches, a beach cafe, 3 surfboards and 6 gently waving flags. All props use collider-free visuals and are outside the court. No crowd collisions, forces, match-rule or player-physics changes. C# compilation and spectator-layout checks passed. Runtime animation, performance and multiplayer play test remain pending in Unity. Festival images are Blender previews of the matching layout, not Unity screenshots.
