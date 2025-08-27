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
- The idle timeout can be adjusted by editing the configuration file: `config\hibernateSmartConfig.con` Inside this file, locate the setting: `IdleSecondsBeforeHibernate=3600`
    - **3600** = number of seconds before hibernation (default: 1 hour).
    - **Minimum value:** 60 seconds (1 minute).
    - **Maximum value:** 86400 seconds (24 hours).

If the system remains idle for the specified duration, HibernateSmart will initiate hibernation.

---

## Configuration
- Open `config\hibernateSmartConfig.conf` in a text editor.
- Modify the `IdleSecondsBeforeHibernate` value to your preferred idle timeout.
- Save the file and restart HibernateSmart for changes to take effect.

---

## Running at Startup
If you want HibernateSmart to start automatically every time you log in:

**Option 1 – Run the PowerShell script**
- Right‑click `add_task.ps1` and select **Run with PowerShell**.
- Follow the on‑screen prompts to confirm the executable path and create the scheduled task.
- **Important:** Run PowerShell as Administrator to avoid permission errors.

**Option 2 – Use the batch file**
- Right‑click `add_task.bat` and select **Run as administrator**.
- This will execute the PowerShell script.

The scheduled task named `HibernateSmartTask` will launch HibernateSmart at every user logon.

---

## Requirements
- **Windows 10 version 1803 or later** (Hibernate feature must be supported by the system).
- **PowerShell 5.1 or later** (pre‑installed on Windows 10 and newer).
- **.NET Framework 4.7.2** (pre‑installed on Windows 10 version 1803 and later; older versions may require manual installation).
- **Administrator privileges** are required to create the startup task.

---

## Remove Startup Task
If you no longer want HibernateSmart to start automatically at user logon, you can remove the scheduled task in one of the following ways:

**Option 1 – Remove via Task Scheduler**
1. Open **Task Scheduler**.
2. Navigate to **Task Scheduler Library**.
3. Locate the task named `HibernateSmartTask`.
4. Right‑click it and select **Delete**.

**Option 2 – Run the PowerShell removal script**
1. Right‑click `remove_task.ps1` and select **Run with PowerShell**.
2. Follow the on‑screen prompts to confirm removal.
3. **Important:** Run PowerShell as Administrator to avoid permission errors.

**Option 3 – Use the batch file**
1. Right‑click `remove_task.bat` and select **Run as administrator**.
2. This will execute the removal script automatically without needing to open PowerShell manually.

> **Note:** Removing the startup task does not uninstall HibernateSmart; it only prevents it from launching automatically at logon.

---

## License
MIT — feel free to use, modify, and share!
