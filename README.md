Ongoing development of a grid component for the unity engine. This plugin has initially been built for use in a custom VTT software and is currently in the process of pulling out that functionality into simple functions and components for use in any project.

Grids are not as easy as they look and this plugin aims to help with getting your grid system up and running. This grid will procedurally generate interactable cells for use in any 3D game setting and comes with methods which will allow for most interactions required of a general grid system.

This plugin currently requires the use of URP due to how it uses the render features to create a stencil which will draw the lines without interacting with the world.



There is a script in the editor folder which will take the set URP SRP settings in Graphics and will add the render features required of the system. If they are not there, there is a manual function in Tools > Simple Grid.
