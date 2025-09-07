# HibernateSmart – Automatic Hibernate Utility 💤

SmartHibernate is a lightweight Windows utility that automatically hibernates your system when idle — unless sleep blockers are active.

---

## Download
Pre‑built releases are available on the [Releases page](../../releases) — no need to build from source.

- [Download for Windows x86 (32‑bit)](../../releases/latest/download/HibernateSmart_x86.zip)
- [Download for Windows x64 (64‑bit)](../../releases/latest/download/HibernateSmart_x64.zip)
- [Download for Windows ARM64](../../releases/latest/download/HibernateSmart_ARM64.zip)

> **Note:** Please select the correct build that matches your system architecture.  
> - Most modern PCs use **x64**.  
> - **x86** is for older 32‑bit systems.  
> - **ARM64** is for devices like Surface Pro X or Windows on ARM laptops.

---

## Overview
On some modern devices or certain editions of Windows, the **"Hibernate after"** setting in the Power Options has been removed or hidden.  
HibernateSmart is a lightweight utility that monitors system idle time and automatically hibernates the computer after a configurable period of inactivity.  
This can help save power, preserve your work state, and extend battery life on portable devices.

---

## How It Works
- The application runs in the background and tracks user activity.
- When the system has been idle for the configured number of seconds, it will trigger Hibernate automatically.
- To change the idle threshold:
    1. Right‑click the application’s tray icon in the system tray.
    2. Select **Settings**.
    3. Adjust the **Idle threshold** value.
    4. Save your changes.
- Allowed values:
    - **Default value**: 3600 seconds (1 hour)
    - **Minimum value**: 60 seconds (1 minute)
    - **Maximum value**: 86400 seconds (24 hours)

If the system remains idle for the specified duration, HibernateSmart will initiate hibernation.

---

## Configuration
- Open the application’s Settings from the system tray icon.
- Two options are available:
    1. 	**Enable logging**: toggle whether to write logs to a file. **This does not affect in‑program logging**.
    2. 	**Idle threshold (seconds)**: adjust the number of seconds the system can remain idle before hibernation is triggered.
- After making changes, click Save to apply them.

---

## Running at Startup
If you want HibernateSmart to start automatically every time you log in:

**Option 1 – Run the PowerShell script**
- Right‑click `auto-startup.ps1` and select **Run with PowerShell**.
- Follow the on‑screen prompts to confirm the executable path.
- The script will add a registry entry under `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` that launches HibernateSmart at logon.
- **Important:** Run PowerShell as Administrator to avoid permission errors.

**Option 2 – Use the batch file**
- Right‑click `auto-startup.bat` and select **Run as administrator**.
- This will execute the PowerShell script.

The registry entry named `HibernateSmart` will ensure HibernateSmart starts at every user logon and will trigger a UAC prompt before launch.

---

## Requirements
- **Windows 10 or later** – Hibernate feature must be supported by the system.
- **PowerShell 5.1 or later** pre‑installed on Windows 10 and later.
- **.NET Framework 4.6.2** pre‑installed on Windows 10 version 1607 and later; For earlier versions, it may need to be installed manually.
- **Administrator privileges** are required to add HibernateSmart to startup or to run the application.

---

## Remove Startup Entry
If you no longer want HibernateSmart to start automatically at user logon, you can remove the registry entry in one of the following ways:

**Option 1 – Remove via Registry Editor**
1. Press Windows + R, type `regedit`, and press Enter.
2. Navigate to:
```
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
```
3. Locate the value named `HibernateSmart`
4. Right‑click it and select **Delete**.
5. Close Registry Editor.

**Option 2 – Run the PowerShell removal script**
1. Right‑click `remove-startup.ps1` and select **Run with PowerShell**.
2. Follow the on‑screen prompts to confirm removal.
3. **Important:** Run PowerShell as Administrator to avoid permission errors.

**Option 3 – Use the batch file**
1. Right‑click `remove-startup.bat` and select **Run as administrator**.
2. This will execute the removal script automatically without needing to open PowerShell manually.

> **Note:** Removing the startup entry does not uninstall HibernateSmart; it only prevents it from launching automatically at logon.

---

## License
MIT — feel free to use, modify, and share!
