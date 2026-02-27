# OmniTransfer v0.9.0
### High-Performance Multi-Threaded File Migration Utility

OmniTransfer is a professional-looking WPF wrapper for Robocopy, designed to handle high-volume data migrations with a focus on speed, safety, and system-level efficiency. 

## Key Features
**Thread Optimization:** Hard-capped, selectable thread counts to maximize I/O throughput without saturating system resources.

**Windows Shell Integration:** Custom Registry-based context menu for "Right-Click to Move" functionality.

**Instance Aware:** (In Development) 
* Handle multi-file selections via the Windows 'Player' MultiSelectModel.

## Built With
* **C# / WPF** (.NET 10.0)
* **Windows Registry API** for shell extensions
* **Robocopy Engine** for robust, resumable file operations

## Project Context
This tool was developed to bridge the gap between complex command-line utilities and user-friendly GUIs. Drawing from my experience with slow Windows Explorer file transfers, OmniTransfer prioritizes speed, yet maintains accuracy using robocopy to move files at 3-10x the rate of a normal folder move done in Explorer.

## ⚠️ Known Issues (v0.9.0)
* **Multi-Instance Bug:** Right-clicking multiple files currently opens one window per file. (Fix: Implementing `MultiSelectModel = Player` in next patch).
* **Progress Tracking:** Progress bar currently tracks top-level items. Granular file-by-file percentage parsing is in development.

## License
Distributed under the MIT License. See `LICENSE` for more information.

---
**Author:** Karl Vonderhaar  
**Contact:** karlwvonderhaar@gmail.com
