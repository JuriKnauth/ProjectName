# Project Name — Unity Template + MAX

This repo is a **Unity template** I use as a base.  
The actual sample for this assignment is in **`Project/Sample/Test`**.

It shows **AppLovin MAX** running in Unity with a clean boot flow, modular settings, and an adapter so SDK changes don’t leak into the game code.

---

## Why I set it up like this

From past projects (incl. at Travian), two things hurt the most:
- **Messy startup** → SDKs initialize at weird times, first calls get lost.
- **Tight coupling to plugins** → updating a plugin breaks the app.

So this template has:
- **BootManagerService** — starts services in order and waits until they’re ready.
- **SettingsService** — one `Settings` asset with typed sections (no magic strings).
- **AppLovinMaxService** — a small adapter that hides the MAX plugin behind our own interface.

---

## Branches
- **Core**: the reusable template (Boot, Settings, Logging, Utilities).
- **Plugin/AppLovinMax**: the MAX adapter.
- **Project/Sample/Test**: the sample app that uses Core + the Plugin/AppLovinMax branch.

---

## How to test

1. Switch to **`Project/Sample/Test`**. and open the project.
2. Open the **`00_Bootstrap`** scene and press Play.
3. Use the buttons to load/show ads.
4. Check the Console for callbacks.

---

## Features

- Startup is **predictable** (important for any SDK-heavy app).
- The **adapter** protects from breaking changes rippling through the app.
- **Typed settings** make it hard to misconfigure and easy to review.
