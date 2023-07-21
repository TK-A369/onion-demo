using OnionEngine.Core;
using OnionEngine.Graphics;
using OnionEngine.Physics;
using OnionEngine.IoC;
using OnionEngine.DataTypes;

class Program
{
	public static void Main(string[] args)
	{
		// Turn on debug mode
		GameManager.debugMode = true;

		// Create GameManager
		GameManager gameManager = new GameManager();

		// Register component types
		gameManager.AutoRegisterComponentTypes();
		Console.WriteLine();

		// Register entity systems
		gameManager.AutoRegisterEntitySystemTypes();
		Console.WriteLine();

		// Load prototypes
		gameManager.prototypeManager.AutoregisterPrototypeTypes();
		// gameManager.prototypeManager.LoadPrototypes(File.ReadAllText("Resources/Prototypes/Test1.xml"));
		// gameManager.prototypeManager.LoadPrototypes(File.ReadAllText("Resources/Prototypes/Test2.json"));
		gameManager.prototypeManager.LoadPrototypes(File.ReadAllText(
			System.IO.Path.Join(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/Prototypes/Test2.json")));
		Console.WriteLine();

		Console.WriteLine(gameManager.DumpEntitiesAndComponents());
		Console.WriteLine();

		using (Window win = IoCManager.CreateInstance<Window>(new object[] { 800, 600, "Onion engine demo" }))
		{
			win.afterLoadEvent.RegisterSubscriber((_) =>
			{
				// Create and remove some entities and components
				Int64 entity1 = gameManager.AddEntity("entity1");

				RenderComponent renderComponent = new RenderComponent();
				renderComponent.entityId = entity1;
				gameManager.AddComponent(renderComponent);

				PositionComponent positionComponent = new PositionComponent();
				positionComponent.entityId = entity1;
				positionComponent.position = new Vec2<double>(0, 0);
				gameManager.AddComponent(positionComponent);

				RotationComponent rotationComponent = new RotationComponent();
				rotationComponent.entityId = entity1;
				rotationComponent.rotation = 0;
				gameManager.AddComponent(rotationComponent);

				SpriteComponent spriteComponent = new SpriteComponent();
				spriteComponent.entityId = entity1;
				spriteComponent.textureName = "human-1";
				spriteComponent.size = new Vec2<double>(1, 1);
				gameManager.AddComponent(spriteComponent);
			});

			win.Run();
		}
	}
}
