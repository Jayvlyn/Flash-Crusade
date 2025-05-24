# 🚀 Flash Crusade  
*A retro-style, top-down fleet fighter game set in the wake of the mysterious Chromaclysm.*

---

## 🧭 Overview  

**Flash Crusade** is a 2D top-down space combat game built in Unity (C#), where you pilot a ship in a colorful, retro-inspired galaxy. Take command of a customizable fleet, engage in dynamic battles, and fight for your version of the truth in the aftermath of a galactic event that shattered civilizations.

Players control a flagship in real time, managing movement, rotation, and speed while giving strategic commands to AI-controlled allies. The game combines twitch reflexes with tactical coordination in a world shaped by myth, misinformation, and chromatic chaos.

---

## 🎮 How to Play  

| Action         | Key(s)                        |
|----------------|-------------------------------|
| Thrust         | **W** (Forward), **S** (Reverse) |
| Strafe         | **A** (Left), **D** (Right)      |
| Turn           | **J** (Counter-Clockwise), **L** (Clockwise) |
| Boost Thrusters| **K**                           |

### Gameplay Loop  
- Pilot your ship through hostile sectors.  
- Engage enemy fleets in dogfights.  
- Issue commands to friendly ships in your fleet.  
- Earn upgrades and adapt your playstyle.  
- Fight under your color’s banner to claim control of the galactic narrative.

---

## 🌌 Backstory: The Chromaclysm  

In the year **3699**, the galaxy of **Virellia** was forever changed by a radiant flash that soared from its core—**The Chromaclysm**. But what each civilization saw was different. Red. Blue. Green. Violet. Gold.

The truth became fragmented, distorted by distance, perception, and belief. Formerly united systems splintered into factions, each convinced their color was the true vision and the key to galactic destiny.

Now, fleets wage war not for resources or power, but to prove their color was the one the galaxy *was meant* to see. Amid the stars, a crusade burns—a **Flash Crusade**—fueled by belief, beauty, and betrayal.

---

## 🧠 Code Structure & Design Patterns  

This project emphasizes **clean architecture** and **scalability** through the use of key object-oriented and game-specific design patterns:
**TBD**
Examples: 
- **State Pattern**: For ship AI behaviors (idle, follow, attack, flee), allowing easily expandable enemy/fleet behaviors.  
- **Command Pattern**: Enables a clean system for fleet control and player-issued commands without tight coupling.  
- **Singletons**: Used sparingly for globally accessible systems like the input manager and game state manager.  
- **Factory Pattern**: For spawning ships, weapons, and projectiles in a modular and configurable way.  
- **Observer/Event System**: Decouples input, UI, and game logic, ensuring that gameplay events (like damage or powerup collection) can be handled cleanly and safely.

The project is designed for modular growth, with future plans for additional ship types, factions, campaign systems, and more complex fleet AI systems.

