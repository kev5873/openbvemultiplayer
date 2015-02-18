OpenBVE Multiplayer
---

This is a fork of the original OpenBve, the original website has been taken down, and there are no longer any official websites to download it from.  Since this product came out of [BVEStation.com](http://www.bvestation.com), we recommend you visit [BVEStation](http://www.bvestation.com) for a copy of the game.

This add-on will enable multiplayer support.

History
---
BVEStation used to have other versions of a multiplayer implementation.  The first version was a very buggy client-server model, with little to no interaction with the game itself.  The second version utilized a centralized approach for a MMO styled experience.  The version in this repository once again utilizes the client-server model, because of the low numbers of players, and increased costs of keeping a centralized server up.

How it works
---
Using the game's pretrain, which is usually set by the RunInterval in the route file, Multiplayer takes over this pretrain and reads data from the closest player in the front.  This data is then applied to the pretrain, which simulates the location of the other player.

Caveats
---
There are a large number of caveats that go with playing multiplayer.

Because BVE is inherently a single line, it is impossible to simulate a "network" of trains/routes.  Do not request this feature, it is impossible.

Players must be connected in a sequence, each with a delay in order to prevent collisions.  The first player should connect, and begin moving.  The second player should then connect after an x amount of minutes/or distance after the first player.  This second player will see the location of the first player if they get too close. Same for the other players.

The pretrain is still controlled by the AI, but its position gets forced to change by Multiplayer.

This assumes that all players are using the exact same route.  Do not use a different route with different players.  1 Server, 1 Route, everyone plays the same.

The first player when done, should disconnect when the run is over.  Players in between other players should not disconnect (although this is now mitigated, this was a major problem in the first version of multiplayer)

Important Files
---

Included is the tcpServer.py, which is a server written in Python.  The client code is within the repository.  The bulk of the code is in openBVE/OpenBve/OldCode/Multiplayer.cs.


License
---
OpenBVE is in public domain, but this multiplayer add-on is not.  This means tcpServer.py and Multiplayer.cs are not in public domain.  These files are instead under the [Creative Commons License](http://creativecommons.org/licenses/by-nc-sa/3.0/).  Please read it if you plan to make additional add-ons to this.

Feel free to fork this repo, submit a pull request, etc.

Visit [BVEStation.com](http://www.bvestation.com)
