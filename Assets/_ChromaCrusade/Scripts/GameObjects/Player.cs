using UnityEngine;

public class Player : MonoBehaviour
{
    /**
     * Space velocities should be perpetual, there is no drag in space. this allows for player to 
     * set course of their ship and not need to hold a bunch of buttons down to travel a long distance
     * the player will be able to hold w to bring their ship up to any speed within their max speed, and when they let go of the key it will stay that speed
     * the player can hold their turbo to further increase the speed, and even after they let go of turbo their speed will be purpetual. 
     * ships wont naturally slow down as if there were drag, instead they will have to use counter thrust to stop, 
     * or use their brake key to come to a complete stop without adding force in the opposite direction
    /**
     * Player inputs:
     * 
     * W - Fly Forward                               [W]                    [I]
     * A - Fly Left ("strafe")                    [A] [S] [D]            [J] [K] [L]
     * S - Fly Backward                                         [Space]
     * D - Fly Right ("strafe")
     * WASD will also navigate menus
     * 
     * I - Turbo (incrase acceleration and max speed)
     * J - Turn left
     * K - Break (raise drag from 0 to bring to full stop)
     * L - Turn right
     * 
     * Space - Fire weapons
     * Alternatively
     * 
     * Esc - Pause or Back (depend on current screen state. paused? in ship editor? flying?)
     * 
     * Combining inputs will offer unique movement, likes curves.
     * 
     * say the player is heading north because they previously used input in that direction but they are holding no inputs, and they turn, they will turn in place
     * however if they are holding inputs to move that direction and turn, they will start taking a curved course 
     */
}
