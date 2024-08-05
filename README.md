# Skirmish
Game and prototypes with SharpDX and Directx 11

![Build](https://github.com/Selinux24/Skirmish/actions/workflows/dev.yml/badge.svg)
[![SonarQube](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=alert_status)](https://sonarcloud.io/dashboard?id=Selinux24_Skirmish_dev) 

### What is it?

Skirmish is a collection of projects created for testing a simple game engine, that is being built tailor-made for a real game.

### What contains?

There are several prototypes:

 ## 01 - Basic Samples

 Several samples of built-in components drawing and input capture.

 ![screenshot](/Docs/Images/01_Menu.png?raw=true)
 
 # Cascaded Shadows

 Cascaded shadow mapping.

 ![screenshot](/Docs/Images/01_Cascaded.png?raw=true)

 # Lights

 Point lights and spot lights.
 
 ![screenshot](/Docs/Images/01_Lights.png?raw=true)

 # Materials

 Built-in materials.

 ![screenshot](/Docs/Images/01_Materials.png?raw=true)

 # Normal Maps

 Simple normal maps.

 ![screenshot](/Docs/Images/01_NormalMaps.png?raw=true)

 # Particle Systems

 CPU & GPU particle systems.

 ![screenshot](/Docs/Images/01_Particles.png?raw=true)

 # Stencil Pass

 Stencil pass test.

 ![screenshot](/Docs/Images/01_StencilPass.png?raw=true)

 # Test Scene

 Full scene test with all Built-in components.

 ![screenshot](/Docs/Images/01_TestScene.png?raw=true)

 # User Interface

 Dynamic user interface test.

 ![screenshot](/Docs/Images/01_UI.png?raw=true)

 # Water

 Water shader test.

 ![screenshot](/Docs/Images/01_Water.png?raw=true)

 ## 02 - Intermediate Samples

 Intermediate level samples. Including animation, deferred rendering, and instancing.

 ![screenshot](/Docs/Images/02_Menu.png?raw=true)

 # Simple Animation

 Simple animation test.

 ![screenshot](/Docs/Images/02_SimpleAnimation.png?raw=true)

 # Transforms

 Custom controller transforms test.

 ![screenshot](/Docs/Images/02_Transforms.png?raw=true)

 # Gardener

 Dynamic gardener test.

 You can define a foliage map and a heightmap, and the gardener will plant trees and grass on the map.

 ![screenshot](/Docs/Images/02_Gardener_1.png?raw=true)

 The gardener will show the foliage map, based on the player point of view.

 ![screenshot](/Docs/Images/02_Gardener_2.png?raw=true)

 # Mixamo Models

 Mixamo models test.

 ![screenshot](/Docs/Images/02_MixamoModels.png?raw=true)

 # Deferred Lighting

 Sample scene for deferred lighting tests.

 ![screenshot](/Docs/Images/02_DeferredLighting.png?raw=true)
 
 # Smooth Transitions

 Interpolation between two animation clips of the same model.

 ![screenshot](/Docs/Images/02_SmoothTransitions.png?raw=true)

 # Animation Parts

 Independant model part transforms test.

 ![screenshot](/Docs/Images/02_AnimationParts.png?raw=true)

 # Instancing

 Instancing test.

 ![screenshot](/Docs/Images/02_Instancing.png?raw=true)

 ## 03 - Terrain Samples

 Terrain samples. Including crowd navigation, heightmap, and sky scattering.

 ![screenshot](/Docs/Images/03_Menu.png?raw=true)

 This project contains a full c# conversion of the [Detour & Recast](https://github.com/recastnavigation/recastnavigation) libraries of Mikko Mononen:
 
 > copyright (c) 2009 Mikko Mononen memon@inside.org.
 
 Thanks a lot, Mikko. You are a genius!

 # Crowds

 Crowd navigation

 ![screenshot](/Docs/Images/03_Crowds.png?raw=true)

 # A* Grid

 A* grid test in a turn based game.

 ![screenshot](/Docs/Images/03_Grid.png?raw=true)

 # Heightmap

 Heightmap test with dynamic chunk load and path finding navigation

 ![screenshot](/Docs/Images/03_Heightmap.png?raw=true)

 # Modular Dungeon

 Modular terrain component test, with One Page Dungeon integration.

 See https://watabou.itch.io/one-page-dungeon

 ![screenshot](/Docs/Images/03_ModularDungeon_1.png?raw=true)

 ![screenshot](/Docs/Images/03_ModularDungeon_2.png?raw=true)

 JSon customizable dungeons too.

 ![screenshot](/Docs/Images/03_ModularDungeon_3.png?raw=true)
 ![screenshot](/Docs/Images/03_ModularDungeon_4.png?raw=true)
 ![screenshot](/Docs/Images/03_ModularDungeon_5.png?raw=true)
 ![screenshot](/Docs/Images/03_ModularDungeon_6.png?raw=true)
 ![screenshot](/Docs/Images/03_ModularDungeon_7.png?raw=true)
 ![screenshot](/Docs/Images/03_ModularDungeon_8.png?raw=true)

 # Navigation Mesh

 Navigation mesh test page. The navigation mesh is a custom porting of the original Recast Navigation

 See https://recastnav.com/

 ![screenshot](/Docs/Images/03_PathFinding.png?raw=true)

 # Perlin Noise

 Perlin noise generator.

 ![screenshot](/Docs/Images/03_PerlinNoise.png?raw=true)

 # Real Time Strategy

 Real time strategy game test. AI with simple state machines.

 ![screenshot](/Docs/Images/03_RTS.png?raw=true)

 # Skybox

 Skybox test scene with spatial audio. The audio is based on the player position.

 ![screenshot](/Docs/Images/03_Skybox.png?raw=true)

 ## 04 - Physincs Samples

 Physics sample, including rigid bodies and joints.

 ![screenshot](/Docs/Images/04_Physics.png?raw=true)

 ## 05 - AI Samples

 AI samples inspired by Radu Mariescu-Istodor YouTube series.

 See https://www.youtube.com/@Radu

 # Self-Driving Car

 [Self-driving Car :: Phase 1](https://youtube.com/playlist?list=PLB0Tybl0UNfYoJE7ZwsBQoDIG4YN9ptyY&si=jTJ4qaLbHhTqh0U8)

 ![screenshot](/Docs/Images/05_SelfDrivingCar.png?raw=true)

 # A Virtual World

 [A Virtual World :: Phase 2](https://www.youtube.com/playlist?list=PLB0Tybl0UNfZtY5IQl1aNwcoOPJNtnPEO)

 ![screenshot](/Docs/Images/05_VirtualWorld.png?raw=true)
 
 ## Other Credits

 This project is a long time work, and I have used several resources from the internet. I want to thank all the people that have shared their knowledge and resources.
 
 Although I have tried to keep track of all the resources, by adding the credits in the source code, I am sure that I have missed some of them. If you see your work here, please let me know, and I will add you to the credits.

 ## SonarQube Status

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)

[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Selinux24_Skirmish_dev&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Selinux24_Skirmish_dev)
