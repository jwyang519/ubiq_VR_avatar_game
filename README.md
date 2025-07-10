# 🧙‍♂️ House of Echoes  
**An Immersive Social VR Experience**  
By Rachel Yang, Jiale Li, Julian Marchington, Xiaojing Zhang

![Unity](https://img.shields.io/badge/Unity-2022.3.10f1-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

---

## 🎯 Project Goal  
To create an immersive multiplayer VR space focused on collaborative avatar customization and playful transformation mechanics, using the Ubiq networking system in Unity.

---

## 📌 Key Features

- **👕 Real-time Outfit Customization**  
  Players can enter a virtual fitting room to customize their avatars on the fly and see their changes in real time.

- **🪄 Environment-Triggered Avatar Transformation**  
  Interact with magical objects to transform into fun avatars like guitars, spray cans, or cacti — each with unique reactions and sounds.

- **🎭 Advanced Avatar Assets**  
  Custom avatars from 3D Characters Pro were integrated into a shared networked environment for a richer visual experience.

- **🔊 Avatar-Specific Interactions**  
  Transformed avatars respond to interaction with sounds (e.g., strumming a guitar, shaking a spray can, "ouch!" from a cactus).

---

## 🕹️ Gameplay Overview

You're invited to a magical house party where you:
- Customize your avatar in a shared fitting room
- Interact with a mysterious NPC wizard
- Discover enchanted objects that let you transform
- Engage in playful, sound-driven social interactions

---

## 🛠️ Development Highlights

- **Built in Unity** using XR Toolkit + Ubiq for multiplayer support  
- Real-time customization via UI, prefab manipulation, and JSON sync  
- Avatar transformation system with object tracking and sound feedback  
- Networked player presence and interactions across clients  

---

## 📜 Key Scripts

- `SimpleAvatarCustomizer` & `AvatarCustomizationUI` — Mesh switching and UI for outfit choices  
- `AvatarPartNetworkSync` — JSON-based real-time outfit syncing  
- `AvatarSoundInteraction` — Sound triggers for transformed avatars  
- `ObjectAvatarControl` — Tracks head and hands while in object form  
- `AvatarControlUbiq` — Maps body + head movement from XR input

---

## 👥 Credits

| Role | Team Members |
|------|--------------|
| Game Design | Rachel Yang, Jiale Li, Julian Marchington, Xiaojing Zhang |
| Avatar Features | Julian Marchington, Jiale Li, Rachel Yang |
| Environment & Assets | Rachel Yang, Xiaojing Zhang, Julian Marchington |
| Networking (Ubiq) | Jiale Li, Julian Marchington |
| UI & Interaction | Julian Marchington, Jiale Li |
| Sound Effects | Jiale Li, Rachel Yang |
| Animation | Julian Marchington |
| Report Writing | Xiaojing Zhang, Rachel Yang |

Special thanks to our TAs and Unity Asset Store creators 🎨

---

## 📚 References

- Gonzalez-Franco et al., *The impact of first-person avatar customization on embodiment in immersive virtual reality*, Frontiers in VR, 2024  
- [Transformed Social Interaction – Wikipedia](https://en.wikipedia.org/wiki/Transformed_social_interaction)  
- [3D Characters Pro – Casual (Unity Asset Store)](https://assetstore.unity.com/packages/3d/characters/humanoids/3d-characters-pro-casual-287455)  
- [Low Poly 30 Rooms + Interiors – Unity Asset](https://assetstore.unity.com/packages/3d/props/interior/low-poly-30-rooms-interiors-1000-objects-213318)

---

## 🧾 License

This project is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for details.
