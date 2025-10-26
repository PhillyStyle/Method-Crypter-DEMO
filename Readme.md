# Method Crypter DEMO
Encrypt Your Methods ***Inside Your EXEs!*** (In C# 4.8)

## This Project has been superseded by Method Crypter DEMO 2 [https://github.com/PhillyStyle/Method-Crypter-DEMO-2](https://github.com/PhillyStyle/Method-Crypter-DEMO-2)

![Method Crypter GUI](MethodCrypterScreenShot.png?raw=true)

Also do other fun stuff like: 
* Randomize Method names (and variable names)
* Inject junk code at the start of your encrypted methods
* Randomize your Assembly GUID
* Play Console Snake! (The encrypted payload in the Demo.)

What is it?
Method Crypter is a suite of 3 programs:
* Method Crypter.exe
* Crypted Demo.exe
* Server.exe

What each program does:
* **Method Crypter.exe** is the program with the GUI I created (seen above) for encrypting the payload EXE (Crypted Demo.exe).
  * This is where you list the Types (namespace.class) and Methods that you are going to encrypt.
  * Set the AES Key and IV (I recommend using the Random AES Key & IV Button every time you encrypt.)
  * Pick the paths to the Crypted Demo.exe, Server.exe, and Output EXE.
  * If you are going to encrypt strings then you must define the Type and Array Name for your encrypted strings as defined in Crypted Demo.exe.
  * Choose options like Randomizing Method Names (recommended), Injecting junk code at the start of encrypted methods (To obfuscate method size), Randomizing the Assembly GUID (How often do we forget to do this as developers?), and Show the Method Inspector (A cool little Form that shows you insights about what methods are in your file, their names, and where, and how big, and stuff like that.)
  * ***Now it is time for the secret sauce.***  How does Method Crypter encrypt the methods ***INSIDE*** the EXE without totally corrupting everything?  The answer my friend is Junk Methods.  After every encrypted method in the EXE, MethodCypter creates a junk method.  The Junk method is simply there to get destroyed with the encryption overhead from encrypting the method before it.  The Junk methods never get referenced in the EXE so the corruption doesn't cause an issue.  So if you see junk methods after your encrypted methods in the Method Inspector, that is what it is, and what they are there for.  Now this is just a demo.  An example let's call it of how it can be done.  It could also theoretically be done without the junk methods if you used compression on the methods so that they could fit right back where they were in the first place.  But we don't do that here. We are using junk methods.
 
* **Crypted Demo.exe** is the program with the payload. (The part that gets encrypted.)  In this case the payload is a Console Snake game, (It's pretty fun if I must say so myself.) but the payload can be whatever you want it to be.  Use your imagination.  How does crypted demo do it?
  * In this case, when you type "run" into the console, crypted demo connects to the Server.exe.  Server.exe then sends The AES Key/IV and tells Crypted Demo what methods to decrypt.  I will get into "Why use Server.exe at all" later.  Crypted demo then finds the methods in memory (by doing some wizardry), and decrypts them at runtime.  Then it executes the payload (The Snake Game).

* **Server.exe**.  Server.exe's only mission in life is to wait for a connection, and when a connection comes in, send the AES Key, and IV, and tell it what methods to decrypt. (Also what strings to decrypt if you encrypted any strings.)  The Server.exe traffic is not encrypted in the demo, but in real life I recommend encrypting it.  I simply employed a simple text based protocol for communication since it is just a demo.  It is a pretty flexible little text based protocol though.  Feel free to use it in your own projects.
  * Now "Why use a Server.exe at all?"  This is a very important thing to understand.  Using a Server.exe ensures that the payload is secure from any decryption before it is needed.  It can't be decrypted in a virtual environment, or sandbox or anything, because even it doesn't know the Key/IV making this a pretty secure way of doing things.

## Some Notes:
1) This is just a Demo showing how things ***can*** be done.  Not necessarily how they ***have to*** be done.
2) The Crypted Demo.exe using this method cannot be used in any crypters or programs that do process injection, because it has to be able to find the methods in memory, and if you use it in process injection, it won't be able to find the methods and will probably crash.
3) If you run Crypted Demo.exe without encrypting it first, it will crash when you type "run".

## Thanks and Shoutout
Thanks to ChatGPT for helping me code this.

Shoutout to everyone on Hack Forums where I plan on publishing this.

## Sad News
At the time of writing this, it appears that Windows Defender may be detecting all encrypted files that are encrypted using this method as viruses.  I guess they don't like that this is possible.  (Maybe they don't like Console Snake?)  Before encrypting, Crypted Demo.exe is not detected as a virus, but after encrypting it is detected as a virus.  Oh well.  I still have more ideas for method encrypting.  I'm not done yet.  It's not a virus by the way.  Look through the source code for yourself.  Test it on a VM.  It's not a virus.
