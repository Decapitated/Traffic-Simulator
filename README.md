# Traffic_Sim

## Creating a Route:
1. Create an empty object
2. Attach `Route` script to new empty object
3. Press `Ctrl + Right Click` on an object that can recieve Raycasts
4. The newly created point will be added as a child to the object

## Combining Routes
1. Create an empty object
2. Attach `RouteManager` script to new empty object
3. Specify how many points you want the route made of
4. Specify how many routes to combine
5. Attach, in order, the routes to the `Route Objects` array
