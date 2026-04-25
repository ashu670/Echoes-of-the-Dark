# Echoes of the Dark

A psychological horror prototype built in Unity HDRP where player actions generate noise, attracting a reactive entity while unseen forces manipulate perception.

---

## Overview

**Echoes of the Dark** is a first-person horror experience focused on **tension, uncertainty, and psychological pressure** rather than traditional jump scares.

The core idea:

> Your actions create danger. Your mind creates fear.

---

## Core Gameplay Loop

```text
Explore → Make Noise → Enemy Reacts → Panic → Hide → Survive (or Die)
```

---

## Current Features (Prototype v1)

### Player System

* First-person movement (walk, run, jump)
* Momentum-based movement
* Responsive camera control

---

### Noise System (Core Mechanic)

* Walking → Low noise
* Running → High noise
* Dynamic `noiseVal` directly influences enemy behavior

---

### Enemy AI (Hunt Mode)

* Detects player via:

  * Sound (noise-based detection)
  * Vision (line-of-sight + field of view)
* Delayed reaction for tension
* Tracks last known player position
* NavMesh-based movement
* Eliminates player on close proximity

---

### Environment & Atmosphere

* Built using Unity HDRP
* Volumetric fog for limited visibility
* Dark, enclosed test environment
* Lighting tuned for horror mood

---

## Vision (Planned Features)

### Non-Hunt Mode (In Development)

* Paranormal event system
* Controlled, randomized disturbances:

  * Footsteps
  * Apparitions
  * Fake chase sequences

---

### Sanity System (Planned)

* Decreases over time and events
* Triggers hallucinations
* Alters player perception

---

### Multiplayer & Trust System (Future Scope)

* Co-op gameplay (up to 4 players)
* Fake teammate hallucinations
* Communication-based tension

---

### Objective System (Planned)

* Ritual-based progression
* Exploration and item collection
* Risk vs reward mechanics

---

## Tech Stack

* **Engine:** Unity (HDRP)
* **Language:** C#
* **AI:** NavMesh system
* **Rendering:** High Definition Render Pipeline

---

## Current Status

* Core gameplay loop implemented
* Expanding psychological systems (Non-Hunt mode)
* Next: sanity-driven event system

---

## Notes

This is an early-stage prototype focused on validating gameplay systems and atmosphere. Features and structure may evolve.

---

## Author

Developed by Abhay Lal

---
