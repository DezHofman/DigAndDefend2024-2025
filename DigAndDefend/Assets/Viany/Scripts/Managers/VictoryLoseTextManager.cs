using UnityEngine;
using TMPro;

public class VictoryLoseTextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText; // Text in gameOverCanvas
    [SerializeField] private TextMeshProUGUI winText;     // Text in winCanvas
    [SerializeField] private GameManager gameManager;

    // Victory lines
    private string[] victoryLines = new string[]
    {
        "That’s one way to lose your head.",
        "You really showed that pile of pebbles who’s boss.",
        "Victory! Please return the golem’s head to lost and found.",
        "You rock.",
        "That golem really cracked under pressure.",
        "Rats, bats, shrooms? More like snacks.",
        "That’s what I call pest control.",
        "You’re the reason the golem lost his head.",
        "Their pathfinding couldn’t save them now.",
        "You're like a rock star... but for murder.",
        "You didn’t just win. You out-bricked the golem.",
        "Tower: 1, Golem: headless.",
        "Sweet, sweet pixelated justice.",
        "And the crowd goes wild... if we had one.",
        "Golem’s retirement plan starts now.",
        "Mushroom soup for dinner!",
        "Achievement unlocked: Professional Rock Smasher.",
        "The golem never saw it coming. Mostly because his eyes fell out.",
        "This win was brought to you by questionable tower placement and sheer luck.",
        "You’ve bested a golem. Time to fight taxes next.",
        "You rock. Literally. They’re everywhere.",
        "The enemies will tell legends of your tower... and how annoying it was.",
        "Perfect! Now do it again, but without crying this time."
    };

    // Lose lines
    private string[] loseLines = new string[]
    {
        "Golem used laser eyes. It was super effective.",
        "Your tower called in sick today.",
        "Laser eyes? What is this, Saturday morning cartoons?",
        "The rats are redecorating your base.",
        "Congratulations. You’ve been golem’d.",
        "Well, at least the bat’s happy.",
        "That rat is now your landlord.",
        "Hey, you almost didn’t lose. Almost.",
        "You made it to Wave... well, let’s not talk about that.",
        "Even the mushroom laughed.",
        "Don’t worry, the rats will throw you a pity party.",
        "This loss has been automatically reported to the Rat Council.",
        "On the bright side... free mushrooms?",
        "The golem now owns this game. Please forward your save file to him.",
        "Don't worry, we’ve hidden the footage from your friends.",
        "Pro tip: dodging laser eyes isn’t an actual mechanic.",
        "You were this close. By ‘this,’ we mean not close at all."
    };

    public void DisplayLoseText()
    {
            gameOverText.text = loseLines[Random.Range(0, loseLines.Length)];
    }

    public void DisplayVictoryText()
    {
            winText.text = victoryLines[Random.Range(0, victoryLines.Length)];
    }
}