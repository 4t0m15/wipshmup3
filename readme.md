## wipshmup3 -- a third alpha of a shmup game whose name i have not decided.
---------------------------------------------------------------------------
FILE STRUCTURE:
Player.CS
    -Controls player movement
    -Controls which animation/sprite to choose based on input
    -Controls player speed
Player2D.CS
    -Does nothing, it is linked to the class. Want to delete but it makes the game crash.
Enemy.CS
    -Controls enemy movement patterns
        -When to spawn
        -Which patterns to spawn in (Like Galaga)
    -Controls enemy types
        -How they look, move, etc.
    -Controls enemy attack patterns
        -Which bullet sprites to show
        -Bullet's effects
Bullet.CS
    -Controls bullet movement
    -Controls whether powerups are applied
Powerups.CS
    -Chooses which powerups are in the game
    -Chooses how long they last
    -Chooses how ofen/where they spawn
Background.CS
    -Controls parralax effect on background
    -Controls background's "3D" effect based on player movement.
        -Ex: Player moves up, mountains in background move down and sky takes up more of the screen since the player is higher up.
    -Controls possible graphics options to disable movement if it bothers someone ¯\_(ツ)_/¯
Score.CS
    -Controls score calculation
HUD.CS
    -Self-explanatory
    -Plans to have different HUD styles in menu
Config.CS
    -Controls resolution and graphics options if there are aspect ratio issues or someone likes vsinc (they are an idiot, but i will provide the option)
Score_UI.CS
    -I am not sure on what this is right know 100 percent but roughly:
        -Ultrakill style rank system (Hakita I love you :3)
            -Gives score based on average sps (score per second) and "Style" (Using powerups in cool/unique ways) (Will probaly have a list in in Bullet.CS)
        -Has 2 options to have a little rectangle on the bottom right like this:
                I-------------------------------------I
                I   RANK LETTER   ---  SPS            I
                I   ----------------------            I
                I   LIST  OF STUFF DONE BY PLAYER     I
                I                                     I
                I-------------------------------------I
----------------------------------------------------------------------------------------------------------------------------------------------------------------
DIRECTORY LISTINGS:
.git
.godot
.gitignore
.gitatributes
.run
.idea
art
    -background
    -player
    -etc
fonts (To try different things for font)
images (this is slop made by editor)
scripts (makes sense)
readme.md (this file)
wipshmup3.sln
wipshmup3.csproj
rest is goyslop
-------------------------------------------------------------------------------------------------------------------------------------------------------------------
Current Pipeline:

I--------------------I
I                    I
I  input             I
I     V              I
I  movement          I
I     V              I
I animation decider  I
I                    I
I--------------------I