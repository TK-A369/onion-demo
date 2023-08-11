using OnionEngine.Core;
using OnionEngine.Graphics;
using OnionEngine.Physics;
using OnionEngine.IoC;
using OnionEngine.DataTypes;
using OnionEngine.Network;
using OnionEngine.UserInterface;

using OpenTK.Graphics.OpenGL4;

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
	private static EventSubscriber<object?>? afterLoadSubscriber;

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
			afterLoadSubscriber = (_) =>
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

				PhysicalBodyComponent physicalBodyComponent = new()
				{
					entityId = entity1,
					mass = 1,
					velocity = new(0.001, 0)
				};
				gameManager.AddComponent(physicalBodyComponent);

				RigidBodyComponent rigidBodyComponent = new()
				{
					entityId = entity1,
					momentOfInertia = 1,
					angularVelocity = 0.005
				};
				gameManager.AddComponent(rigidBodyComponent);

				SpriteComponent spriteComponent = new()
				{
					entityId = entity1,
					textureName = "human-1",
					size = new Vec2<double>(1, 1)
				};
				gameManager.AddComponent(spriteComponent);

				UserInterfaceComponent userInterfaceComponent = new()
				{
					entityId = entity1,
					uiRootControl = IoCManager.CreateInstance<RootControl>(new object[] { })
				};
				Frame frame = IoCManager.CreateInstance<Frame>(new object[] { userInterfaceComponent.uiRootControl });
				frame.backgroundColor = new(1.0f, 1.0f, 1.0f, 0.5f);
				frame.Position = new(-50, 0.1, 0, 0.2);
				frame.Size = new(0, 0.4, 0, 0.6);
				userInterfaceComponent.uiRootControl.AddChild(frame);
				gameManager.AddComponent(userInterfaceComponent);
			};
			win.afterLoadEvent.RegisterSubscriber(afterLoadSubscriber);

			win.renderCallback = () =>
			{
				// Determine eye location
				HashSet<Int64> eyeComponentsIds = gameManager.QueryEntitiesOwningComponents(new HashSet<Type>() { typeof(EyeComponent) });
				Mat<float> eyeMatrix;
				if (eyeComponentsIds.Count >= 1)
				{
					Int64 eyeComponentId = eyeComponentsIds.First();
					EyeComponent eyeComponent = (EyeComponent)gameManager.components[eyeComponentId];
					eyeMatrix = eyeComponent.GetEyeMatrix();
				}
				else
				{
					eyeMatrix = Mat<float>.RotationMatrix(0.0);
				}

				// Set "camera" uniform in all shaders
				// foreach (Shader shader in win.shaders.Values)
				// {
				// 	shader.Use();
				// 	shader.SetUniformMat3f("camera", eyeMatrix);
				// }
				// win.shaders["shader-textured"].Use();
				// win.shaders["shader-textured"].SetUniformMat3f("camera", eyeMatrix);

				// Get list of entities containing data to be rendered
				HashSet<Int64> entitiesToRender = gameManager.QueryEntitiesOwningComponents(new HashSet<Type>() { typeof(RenderComponent) });
				foreach (Int64 entity in entitiesToRender)
				{
					Int64 renderComponentId = gameManager.GetComponent(entity, typeof(RenderComponent));
					RenderComponent renderComponent = (gameManager.components[renderComponentId] as RenderComponent) ?? throw new NullReferenceException();
					renderComponent.renderData.Clear();
				}

				win.drawSpritesEvent.Fire(null);

				// Clear render groups' vertices data
				foreach (RenderGroup renderGroup in win.renderGroups.Values)
				{
					renderGroup.vertices.Clear();
					renderGroup.indices.Clear();
				}

				// Add vertices to appropriate render groups
				foreach (Int64 entity in entitiesToRender)
				{
					Int64 renderComponentId = gameManager.GetComponent(entity, typeof(RenderComponent));
					RenderComponent renderComponent = (gameManager.components[renderComponentId] as RenderComponent) ?? throw new NullReferenceException();
					List<RenderData> dataToRender = renderComponent.GetVertices();
					foreach (RenderData renderData in dataToRender)
					{
						RenderGroup renderGroup = win.renderGroups[renderData.renderGroup];
						int indexOffset = renderGroup.vertices.Count / renderGroup.shader.vertexDescriptorSize;
						foreach (float vertex in renderData.vertices)
						{
							renderGroup.vertices.Add(vertex);
						}
						foreach (int index in renderData.indices)
						{
							renderGroup.indices.Add(index + indexOffset);
						}
					}
				}

				// Render to default framebuffer - onscreen
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Viewport(0, 0, win.width, win.height);

				// Clear
				GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				win.offscreenRenderTargets["offscreen-render-target-1"].Clear();

				win.renderGroups["render-group-basic"].Render();
				win.renderGroups["render-group-textured"].Render(win.offscreenRenderTargets["offscreen-render-target-1"]);
				// win.renderGroups["render-group-textured"].Render();
				// GL.Disable(EnableCap.DepthTest);
				win.renderGroups["render-group-ui-unicolor"].Render();
				// Console.WriteLine(string.Join(", ", win.renderGroups["render-group-ui-unicolor"].vertices));
				// Console.WriteLine(string.Join(", ", win.renderGroups["render-group-ui-unicolor"].indices));
				// GL.Enable(EnableCap.DepthTest);

				// Render to default framebuffer - onscreen
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Viewport(0, 0, win.width, win.height);

				GL.BindVertexArray(win.vertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, win.vertexBufferObject);
				win.shaders["shader-textured"].Use();
				win.textureAtlases["texture-atlas-1"].Use();
				win.offscreenRenderTargets["offscreen-render-target-1"].UseTexture();
				// textures["floor-tile-1"].Use(TextureUnit.Texture0);
				win.shaders["shader-textured"].SetUniform1i("texture0", 0);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

				win.Context.SwapBuffers();

				Console.WriteLine("Render callback");
			};

			win.Run();
		}
	}
}
