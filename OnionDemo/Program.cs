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
				// Frame frame1 = IoCManager.CreateInstance<Frame>(new object[] { userInterfaceComponent.uiRootControl });
				// frame1.backgroundColor = new(1.0f, 1.0f, 1.0f, 0.5f);
				// frame1.Position = new(0, 0.1, 0, 0.2);
				// frame1.Size = new(0, 0.4, 0, 0.6);
				// userInterfaceComponent.uiRootControl.AddChild(frame1);
				// Frame frame2 = IoCManager.CreateInstance<Frame>(new object[] { userInterfaceComponent.uiRootControl });
				// frame2.backgroundColor = new(1.0f, 0.5f, 0.0f, 0.5f);
				// frame2.Position = new(50, 0.5, 20, 0.3);
				// frame2.Size = new(-50, 0.4, 0, 0.4);
				// userInterfaceComponent.uiRootControl.AddChild(frame2);
				// Frame frame3 = IoCManager.CreateInstance<Frame>(new object[] { frame1 });
				// frame3.backgroundColor = new(0.0f, 0.2f, 1.0f, 0.5f);
				// frame3.Position = new(-100, 1.0, -100, 1.0);
				// frame3.Size = new(50, 0.0, 50, 0.0);
				// frame1.AddChild(frame3);
				gameManager.AddComponent(userInterfaceComponent);

				LightSourceComponent lightSourceComponent = new()
				{
					entityId = entity1,
					intensity = 1.0f,
					lightmapTextureName = "lightmap-radial-1",
					lightColor = new ColorRGB(1.0f, 1.0f, 1.0f),
					size = 1.0f
				};
				gameManager.AddComponent(lightSourceComponent);

				gameManager.prototypeManager.SpawnEntityPrototype("sprite-with-light");

				GL.Enable(EnableCap.Blend);
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
				foreach (Shader shader in win.shaders.Values)
				{
					shader.Use();
					shader.SetUniformMat3f("camera", eyeMatrix);
				}
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

				// Add vertices to appropriate render groups
				List<RenderData> renderDataList = new();
				foreach (Int64 entity in entitiesToRender)
				{
					Int64 renderComponentId = gameManager.GetComponent(entity, typeof(RenderComponent));
					RenderComponent renderComponent = (gameManager.components[renderComponentId] as RenderComponent) ?? throw new NullReferenceException();
					renderDataList.AddRange(renderComponent.GetVertices());
				}

				// Optimize render data
				List<RenderData> renderDataListOptimized = win.OptimizeRenderDataList(renderDataList);

				// Render to default framebuffer - onscreen
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Viewport(0, 0, win.width, win.height);

				// Clear
				GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				win.offscreenRenderTargets["offscreen-render-target-world"].Clear();

				GL.Enable(EnableCap.DepthTest);

				foreach (RenderData renderData in renderDataListOptimized)
				{
					OffscreenRenderTarget? target = null;
					if (renderData.renderGroup == "render-group-textured")
						target = win.offscreenRenderTargets["offscreen-render-target-world"];
					if (renderData.renderGroup == "render-group-ui-unicolor")
						GL.DepthFunc(DepthFunction.Lequal);
					win.renderGroups[renderData.renderGroup].Render(renderData, target);
					if (renderData.renderGroup == "render-group-ui-unicolor")
						GL.DepthFunc(DepthFunction.Less);
				}

				// Lighting
				win.offscreenRenderTargets["offscreen-render-target-world-lighted"].Clear();

				// Ambient light
				float ambientLight = 0.15f;
				win.offscreenRenderTargets["offscreen-render-target-lights"].Clear(ambientLight, ambientLight, ambientLight);
				GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // Additive blending
				win.renderGroups["render-group-lighting"].Render(new()
				{
					renderGroup = "render-group-lighting",
					textureAtlasName = "texture-atlas-lightmaps",
					vertices = new() {
						-1, -1, 1, 1, 1, 1,
						 1, -1, 1, 1, 1, 1,
						 1,  1, 1, 1, 1, 1,
						-1,  1, 1, 1, 1, 1
					},
					indices = new() {
						0, 1, 2,
						0, 2, 3
					}
				}, win.offscreenRenderTargets["offscreen-render-target-world-lighted"], new()
				{
					bindTextures = () =>
					{
						win.offscreenRenderTargets["offscreen-render-target-world"].UseTexture(TextureUnit.Texture0);
						win.offscreenRenderTargets["offscreen-render-target-lights"].UseTexture(TextureUnit.Texture1);
						win.renderGroups["render-group-lighting"].shader.Use();
						win.renderGroups["render-group-lighting"].shader.SetUniform1i("texture_world", 0);
						win.renderGroups["render-group-lighting"].shader.SetUniform1i("texture_light", 1);
					}
				});

				// Lights from sources
				HashSet<Int64> entitiesWithLightSourceComponent = gameManager.QueryEntitiesOwningComponents(new HashSet<Type>() { typeof(LightSourceComponent) });
				foreach (Int64 entity in entitiesWithLightSourceComponent)
				{
					LightSourceComponent lightSourceComponent =
						(LightSourceComponent)gameManager.components[gameManager.GetComponent(entity, typeof(LightSourceComponent))];

					win.offscreenRenderTargets["offscreen-render-target-lights"].Clear(0.0f, 0.0f, 0.0f);

					RenderData lightRenderData = gameManager.GetEntitySystem<LightSourceEntitySystem>(entity).GetLightRenderData();
					GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero); // Default blending - override
					GL.DepthFunc(DepthFunction.Lequal);
					win.renderGroups["render-group-lights"].Render(lightRenderData, win.offscreenRenderTargets["offscreen-render-target-lights"]);
					GL.DepthFunc(DepthFunction.Less);

					GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // Additive blending
					GL.Disable(EnableCap.DepthTest);
					win.renderGroups["render-group-lighting"].Render(new()
					{
						renderGroup = "render-group-lighting",
						textureAtlasName = "texture-atlas-lightmaps",
						vertices = new() {
						-1, -1, 1, lightSourceComponent.lightColor.r, lightSourceComponent.lightColor.g, lightSourceComponent.lightColor.b,
						 1, -1, 1, lightSourceComponent.lightColor.r, lightSourceComponent.lightColor.g, lightSourceComponent.lightColor.b,
						 1,  1, 1, lightSourceComponent.lightColor.r, lightSourceComponent.lightColor.g, lightSourceComponent.lightColor.b,
						-1,  1, 1, lightSourceComponent.lightColor.r, lightSourceComponent.lightColor.g, lightSourceComponent.lightColor.b
					},
						indices = new() {
						0, 1, 2,
						0, 2, 3
					}
					}, win.offscreenRenderTargets["offscreen-render-target-world-lighted"], new()
					{
						bindTextures = () =>
						{
							win.offscreenRenderTargets["offscreen-render-target-world"].UseTexture(TextureUnit.Texture0);
							win.offscreenRenderTargets["offscreen-render-target-lights"].UseTexture(TextureUnit.Texture1);
							win.renderGroups["render-group-lighting"].shader.Use();
							win.renderGroups["render-group-lighting"].shader.SetUniform1i("texture_world", 0);
							win.renderGroups["render-group-lighting"].shader.SetUniform1i("texture_light", 1);
						}
					});
					GL.Enable(EnableCap.DepthTest);
				}

				GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero); // Default blending - override

				// Render to default framebuffer - onscreen
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Viewport(0, 0, win.width, win.height);

				GL.BindVertexArray(win.vertexArrayObject);
				GL.BindBuffer(BufferTarget.ArrayBuffer, win.vertexBufferObject);
				win.shaders["shader-textured"].Use();
				win.textureAtlases["texture-atlas-1"].Use();
				win.offscreenRenderTargets["offscreen-render-target-world-lighted"].UseTexture();
				// win.offscreenRenderTargets["offscreen-render-target-lights"].UseTexture();
				// textures["floor-tile-1"].Use(TextureUnit.Texture0);
				win.shaders["shader-textured"].SetUniform1i("texture0", 0);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

				win.Context.SwapBuffers();
			};

			win.Run();
		}
	}
}
