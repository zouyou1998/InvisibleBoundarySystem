# Utilising Spatial Audio to Enhance Invisible Boundary System in Virtual Reality

## Overview

This repository contains the Unity project developed for the master’s thesis:

**“Utilising Spatial Audio to Enhance Invisible Boundary System in Virtual Reality”**

The project implements an experimental VR system that uses spatial audio warnings to notify users when they approach real-world boundaries during immersive virtual reality experiences.

The system was evaluated through a user study in which participants performed a balloon-hitting task in a VR game environment while different boundary warning systems were tested.

The goal of the project is to investigate how spatial audio cues influence:

* user workload
* perceived presence
* safety
* task performance in VR environments

---

## System Description

The project implements an **Invisible Boundary System (IBS)** based on spatial audio feedback.

The system consists of two main modules:

**1. Boundary Generation Module**

This module reads real-world boundary data provided by the VR headset and generates corresponding virtual walls inside the Unity environment.

**2. Position Monitoring Module**

The system continuously tracks the user’s head, hands, and body position.
When the user approaches a boundary, a spatialized warning sound is generated at the closest point on the boundary.

The volume of the warning sound increases as the user gets closer to the boundary, allowing intuitive perception of distance.

---

## VR Game for Experiment

To evaluate the system, a simple VR game was implemented.

Participants perform a **balloon-hitting task**:

* Balloons appear in a virtual room
* Players use a controller to hit them
* Each balloon burst increases the score
* The game lasts **90 seconds**

The task was designed to encourage natural body movement and interaction within a limited play space.

---

## Experimental Conditions

Participants completed the task under two boundary conditions:

* **Visible Boundary System (VBS)** – traditional visual boundary grid
* **Invisible Boundary System (IBS)** – spatial audio warning system

Four different auditory configurations were tested, varying in:

* sound frequency
* pulse duration
* pulse interval

The study measured:

* task performance (balloon hits)
* boundary breaches
* workload (NASA TLX)
* presence
* perceived safety

---

## Repository Structure

```
UnityProject/      Unity VR project
experiment/        Experiment protocol and questionnaires
data/              Sample experimental data
media/             Demo videos and screenshots
docs/              System diagrams and documentation
```

---

## Requirements

Software:

* Unity (recommended version used in this project)
* Meta XR Audio SDK
* VR headset compatible with Unity (e.g., Meta Quest)

Hardware:

* VR headset
* VR controllers
* play area with boundary tracking

---

## Running the Project

1. Clone the repository

2. Open the project using Unity Hub

3. Open the main scene:

```
Assets/Scenes/MainScene.unity
```

4. Connect the VR headset

5. Press **Play** to start the experiment environment.

---

## Demo

A demonstration video of the system can be found in the `media/` folder.

---

## Author

You Zou
Master’s Program: Audiokommunikation und -technologie
Technische Universität Berlin

Supervisors:

* Prof. Dr. Ceenu George
* Prof. Dr. Stefan Weinzierl

---

## License

This project is provided for academic and research purposes.
