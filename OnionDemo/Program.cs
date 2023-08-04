using OnionEngine.Core;
using OnionEngine.Graphics;
using OnionEngine.Physics;
using OnionEngine.IoC;
using OnionEngine.DataTypes;
using OnionEngine.Network;

public class TestNetMessage : NetMessage
{
	public string content = "";

	public override string ToString()
	{
		return "content: \"" + content + "\"";
	}
}

class Program
{

	public static void Main(string[] _)
	{
		// Turn on debug mode
		GameManager.debugMode = true;

		// Create GameManager
		GameManager gameManager = new();

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
		// gameManager.prototypeManager.LoadPrototypes(File.ReadAllText(
		// 	System.IO.Path.Join(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/Prototypes/Test2.json")));
		gameManager.prototypeManager.LoadPrototypesFromDirectory(@"Resources\Prototypes");
		Console.WriteLine();

		Console.WriteLine(gameManager.DumpEntitiesAndComponents());
		Console.WriteLine();

		// Serialization test
		NetworkMessagesSerializer netMsgSerializer = new();
		netMsgSerializer.RegisterMessageType(typeof(TestNetMessage));
		TestNetMessage testMsg = new()
		{
			content = "Hello World!"
		};
		string serializedMsg = netMsgSerializer.Serialize(testMsg);
		Console.WriteLine("Serialized message:\n" + serializedMsg);

		var (deserializedType, deserializedMsg) = netMsgSerializer.Deserialize(serializedMsg);
		Console.WriteLine("Deserialized message of type " + deserializedType + ":\n" + deserializedMsg);

		using (Window win = IoCManager.CreateInstance<Window>(new object[] { 800, 600, "Onion engine demo" }))
		{
			win.afterLoadEvent.RegisterSubscriber((_) =>
			{
				// Create and remove some entities and components
				Int64 entity1 = gameManager.AddEntity("entity1");

				RenderComponent renderComponent = new()
				{
					entityId = entity1
				};
				gameManager.AddComponent(renderComponent);

				PositionComponent positionComponent = new()
				{
					entityId = entity1,
					position = new Vec2<double>(0, 0)
				};
				gameManager.AddComponent(positionComponent);

				RotationComponent rotationComponent = new()
				{
					entityId = entity1,
					rotation = 0
				};
				gameManager.AddComponent(rotationComponent);

				SpriteComponent spriteComponent = new()
				{
					entityId = entity1,
					textureName = "human-1",
					size = new Vec2<double>(1, 1)
				};
				gameManager.AddComponent(spriteComponent);
			});

			win.Run();
		}
	}
}
