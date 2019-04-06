
=== Installation ===
Requirements: [url=https://github.com/PartialityModding/PartialityLauncher/releases]Partiality Launcher[/url]

[list=1] 
[*] Launch Partiality and hit "File > Open File/Exe" and select Outward.exe in your Steam install folder
[*] If this is the first time running Partiality, click 'Apply Mods', even though there are no mods selected
[*] Unzip and drag mod .DLL file into Outward/Mods folder
[*] Refresh mods on Partiality, check the new mod, and click 'Apply Mods'
[*] Run the game!
[/list]


=== A Note on Security ===
Right now the way mods are written is C# code is written that hooks on to Unity methods, a feature provided by Partiality, then the code is compiled down to a .DLL file. 
DLL files can be very dangerous. There are no limitations on what modders can put in them (that means deleting files, keyloggers, rootkits, everything), so be sure to trust who wrote the mods and where you're downloading them from.
I [i]strongly[/i] suggest never installing a mod that has been given to you personally via some chat platform, unless you trust the person giving you the mod. 
If, for some reason, you do have to download one of my mods from somewhere other than the Nexus, [i]please[/i] run a MD5 checksum on it to ensure the file hasn't been modified.
The command to run a checksum on Windows is (run it in Powershell or Command Prompt)
[code]
certUtil -hashfile MOD_FILE.dll MD5
[/code]

This will give you a string of what appears to be garbage (for example: ca2c4ee75ebb80f83bc8d29f09a78a1c). Compare the string you got with the string on the Nexus website and make sure they are EXACTLY the same. No two different files will generate the same hash, so if the hash is different in any way then something has change inside the file, and you should not trust it anymore.