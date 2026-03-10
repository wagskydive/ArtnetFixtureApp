1. Install OnStartApps from play store

2. Enable Development mode: go to Settings > About Device, highlight Equipment model, and press the OK button 7 times rapidly. A message will confirm you are now a developer.
3. Enable ADB: go to "Other Settings" > "Developer Mode" and enable both "USB Debug" and "ADB Debug"

5. install adb on PC: download platform tools https://developer.android.com/tools/releases/platform-tools

6. connect to projector using adb: adb connect 192.168.1.???

7. Set "Display over other apps" permission using adb

adb shell pm grant com.iniro.onstartapps android.permission.SYSTEM_ALERT_WINDOW

7. Open and Configure OnStartApp: 
    - Verify that Required Permissions has "Display over other apps" enabled
    - Add ArtnetFixture to "Select Applications"
    - Check "Launch on device startup" in Automatic Launch
    - Run a test to see if it works

8. Reboot the projector and the app should start

NOTE 1: The ArtnetFixture app shows a black screen if no artnet data is received. Confirm that the app works by pressing the OK button on the remote controller to open the settings menu.

NOTE 2: some alternative Launcher apps might take longer to launch the app. FLauncher takes longer. 



