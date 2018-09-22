# RealmsOfFerality

Unity project hirearchy

World (No gameobject should be placed in this parent (doesnt count for hirearchy objects))
-> _design (Only gameobject with logic and without visible meshes should be here (Game logic holders and stuff...)
-> _graphics (Only gameobjects with visible meshes and without logic should be here (Trees, grass, buildings...)
-> _script (Only gameobjects with logic and visible meshes should be here (Entities, Pickable objects...)
-> _terrain (Only terrain here)

	Lion Mountains
	-> _design
	-> _graphics
	-> ...
	
	Ocean of beer
	-> _design
	-> _graphics
	-> ...
		
		Island of naked fish
		-> _design
		-> _graphics
		-> ...
	
	Green Lands
	-> _design
	-> _graphics
	-> ...