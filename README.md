A woodland generator for Root: The Roleplaying Game made in unity.

In Root: The Roleplaying Game the world is a woodland inhabited by animals. Many factions in the woodland are at war with each other though each player plays as a vagabond not tied to any faction in particular. The woodland is split into 12 clearings with paths between them and each clearing has a name; a dominant community of either foxes, mice or rabbits and each clearing may be under the control of or have precense of any of the major factions.

This project aims to automate and streamline the creation of the woodland as described in section 9: The Woodland at War in the core book and section 7: The War Advances in the Travellers & Outsiders supplement book.

Currently it generates 12 clearings in random locations and randomly chooses paths between them. The clearings are named and a major denizen is chosen from fox, mouse and rabbit. The clearings are able to be moved around by clicking and dragging them and a new layout can be generated by pressing R. Faction control of each clearing is also randomly determied using the rules described in the core book for the 3 core factions.

There are three implemented edit modes:
- (Modify) the user may drag around clearings to change their position or double click on clearings in order to modify their name, major denizen and faction.
- (Create) clicking on empty space creates a new clearing in that location and clicking on a clearing creates a new path from that clearing that may be dragged to another clearing to make it permenant.
- (Destroy) clicking on paths destroys them and clicking on clearings destroys them along with all adjacent paths.

Planned Features:
- Randomly determining faction control in each clearing (DONE).
- Editing names and denizens and control of each clearing (DONE).
- Adding and Deleting clearings and paths to existing woodlands (DONE),
- Saving and loading clearing layouts.
- Options to reroll each element (clearing locations, paths, denizens, names, faction control) seperately.
- The river going between non-adjacent clearings that is both able to be randomly generated and editable.
- Choosing which factions you want to be involved in faction control setup.
