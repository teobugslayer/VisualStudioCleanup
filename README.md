Visual Studio 2012+ Setup Cleanup Tool
======================================

## The problem

Since version 2012, Microsoft has severely cut the customization options of
Visual Studio setup. It tries to work in every scenario. But many developers 
use only some of its features. Nowadays, people use laptops with extremely
quick but severely cramped SSDs. Visual Studio caches its installation files,
then Windows Installer caches them again, then they are installed and during
this process nearly half GB of logs are created.

## The Solution

Bug Microsoft until they fix it. Seriously!

## The Workaround

In the mean time, you can use my little program. Used on a minimal installation
of VS 14 CTP3, it saved me 4 GB of space (but that is for my use case).

## What does it do

1. It scouts the registry and finds out all potentially uninstallable items, 
which are installed from the VS Package Cache and offers them to you. You can 
choose what to uninstall. **Note**: although many packages register themselves 
with uninstall command, they actually cannot be uninstalled. You will see the 
uninstall UI but the package will stay afterwards. Please suggest workarounds.

2. Part of installation of Visual Studio 2013 Update 2 and later is 
Windows Phone 8 SDK, including emulator images. This not only takes space, but
silently turns on Hyper-V feature of Windows. It slightly slows down the machine
and renders some other virtualization software inoperable. You can turn off
Hyper-V with one click. Note, that you still need to restart your machine
after that. This is tested on Windows 8.1 but may need update for Windows Server.

3. Clean up your temporary folder from setup logs.

4. Move the Package Cache directory off your precious SSD drive. Use this command
if you have second drive (usually slower and larger rotating hard disk).
The program creates a directory junction and links the old place and the new one,
so Visual Studio updates continue working.